using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;

namespace StrongForce.Core
{
	public class ContractRegistry
	{
		private IDictionary<Address, Contract> addressesToContracts = new SortedDictionary<Address, Contract>();

		private IDictionary<ulong, Action> forwardedActions = new SortedDictionary<ulong, Action>();

		private ulong actionNonce = 0;

		public ContractRegistry(IAddressFactory addressFactory)
		{
			this.AddressFactory = addressFactory;
		}

		public ContractRegistry()
			: this(new RandomAddressFactory())
		{
		}

		public IAddressFactory AddressFactory { get; set; }

		public virtual Contract GetContract(Address address)
		{
			return this.addressesToContracts.TryGetValue(address, out Contract contract) ? contract : null;
		}

		public void RegisterContract(Address address, Contract contract)
		{
			if (contract == null)
			{
				throw new ArgumentNullException(nameof(contract));
			}

			this.SetContract(address, contract);

			contract.Address = address;
			contract.SendActionEvent += (targets, type, payload) => this.SendAction(address, targets, type, payload);
			contract.ForwardActionEvent += (id) => this.SendAction(address, id);
			contract.CreateContractEvent += this.CreateContract;
		}

		public bool SendAction(Address sender, Address target, string type, IDictionary<string, object> payload)
		{
			return this.SendAction(sender, new Address[] { target }, type, payload);
		}

		public bool SendAction(Address sender, Address[] targets, string type, IDictionary<string, object> payload)
		{
			var action = this.CreateAction(sender, sender, targets, type, payload);

			return this.ExecuteAction(action);
		}

		public bool SendAction(Address sender, ulong id)
		{
			var action = this.forwardedActions[id];

			if (action.Sender != sender)
			{
				throw new ArgumentOutOfRangeException(nameof(sender));
			}

			return this.ExecuteAction(action);
		}

		public Address CreateContract(Type contractType, params object[] parameters)
		{
			if (!typeof(Contract).IsAssignableFrom(contractType))
			{
				throw new ArgumentOutOfRangeException(nameof(contractType));
			}

			var newAddress = this.AddressFactory.Create();

			// HACK: Needed so that the constructor knows about the "self" address
			Contract.CurrentlyCreatingAddress = newAddress;
			var newContract = (Contract)Activator.CreateInstance(contractType, parameters);
			Contract.CurrentlyCreatingAddress = null;

			this.RegisterContract(newAddress, newContract);

			return newAddress;
		}

		public Address CreateContract<T>(params object[] parameters)
		{
			return this.CreateContract(typeof(T), parameters);
		}

		protected virtual void SetContract(Address address, Contract contract)
		{
			if (this.addressesToContracts.ContainsKey(address))
			{
				throw new InvalidOperationException(
					$"Contract with same address: {address.ToBase64String()} has already been registered");
			}

			this.addressesToContracts[address] = contract;
		}

		private bool ExecuteAction(Action action)
		{
			var contract = this.GetContract(action.Target);

			return contract != null && contract.Receive(action);
		}

		private Action CreateAction(Address origin, Address sender, Address[] targets, string type, IDictionary<string, object> payload)
		{
			if (targets == null || targets.Length == 0)
			{
				throw new ArgumentNullException(nameof(targets));
			}

			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			if (targets.Length == 1)
			{
				return new PayloadAction(
					targets[0],
					origin,
					sender,
					type,
					payload);
			}
			else
			{
				var nextId = this.actionNonce;
				this.actionNonce++;

				var nextTargets = targets.Skip(1).ToArray();

				this.forwardedActions.Add(nextId, this.CreateAction(origin, targets[0], nextTargets, type, payload));

				return new ForwardAction(
					targets[0],
					nextTargets,
					origin,
					sender,
					nextId,
					type,
					payload);
			}
		}
	}
}