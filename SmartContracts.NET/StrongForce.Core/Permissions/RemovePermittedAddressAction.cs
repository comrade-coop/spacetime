using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class RemovePermittedAddressAction : Action
	{
		public RemovePermittedAddressAction(
			Address target,
			Permission permission,
			Address permittedAddress,
			Address receiver)
			: base(target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver;
			this.Permission = permission;
		}

		public Address PermittedAddress { get; }

		public Address Receiver { get; }

		public Permission Permission { get; }
	}
}