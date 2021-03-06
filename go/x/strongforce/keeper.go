package strongforce

import (
	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"
)

// Keeper is the keeper for strongforce
type Keeper struct {
	cdc          *codec.Codec
	storeKey     types.StoreKey
	typeStoreKey types.StoreKey
}

// NewKeeper creates a new keeper for strongforce
func NewKeeper(cdc *codec.Codec, storeKey types.StoreKey, typeStoreKey types.StoreKey) Keeper {
	return Keeper{
		cdc:          cdc,
		storeKey:     storeKey,
		typeStoreKey: typeStoreKey,
	}
}

// SetState sets the state of a contract
func (k Keeper) SetState(ctx types.Context, id []byte, data []byte, typeName []byte) {

	store := ctx.KVStore(k.storeKey)

	store.Set(id, data)

	typeStore := ctx.KVStore(k.typeStoreKey)
	typeStore.Set(id, typeName)

}

// GetState sets the state of a contract
func (k Keeper) GetState(ctx types.Context, id []byte) []byte {
	store := ctx.KVStore(k.storeKey)
	return store.Get(id)
}

// GetState sets the state of a contract
func (k Keeper) GetType(ctx types.Context, id []byte) []byte {
	store := ctx.KVStore(k.typeStoreKey)
	return store.Get(id)
}

// GetContractsIterator returns an iterator over all stored contracts
func (k Keeper) GetContractsStateIterator(ctx types.Context) types.Iterator {
	store := ctx.KVStore(k.storeKey)
	return types.KVStorePrefixIterator(store, []byte{})
}

func (k Keeper) GetContractsTypeIterator(ctx types.Context) types.Iterator {
	store := ctx.KVStore(k.typeStoreKey)
	return types.KVStorePrefixIterator(store, []byte{})
}

// // DelegateCoins implements github.com/cosmos/cosmos-sdk/blob/master/x/staking/types BankKeeper
// func (k Keeper) DelegateCoins(ctx types.Context, addr types.AccAddress, amt types.Coins) (types.Tags, types.Error) {
// 	// return nil, types.ErrInternal("Unimplemented")
// 	return types.EmptyTags(), nil
// }

// // UndelegateCoins implements github.com/cosmos/cosmos-types/blob/master/x/staking/types BankKeeper
// func (k Keeper) UndelegateCoins(ctx types.Context, addr types.AccAddress, amt types.Coins) (types.Tags, types.Error) {
// 	// return nil, types.ErrInternal("Unimplemented")
// 	return types.EmptyTags(), nil
// }

// AddCoins implements github.com/cosmos/cosmos-types/blob/master/x/distribution/types BankKeeper
func (k Keeper) AddCoins(ctx types.Context, addr types.AccAddress, amt types.Coins) (types.Coins, types.Error) {
	// return nil, types.ErrInternal("Unimplemented")
	return types.NewCoins(), nil
}
