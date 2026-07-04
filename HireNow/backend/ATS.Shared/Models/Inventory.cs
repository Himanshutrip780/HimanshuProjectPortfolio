using System;
using System.Collections.Generic;

namespace Core.Systems.Inventory
{
    [Serializable]
    public struct ItemInstance
    {
        public int ItemId;
        public int Quantity;
        public int Durability;
        public Guid InstanceId;

        public static ItemInstance Empty => new ItemInstance
        {
            ItemId = 0,
            Quantity = 0,
            Durability = 0,
            InstanceId = Guid.Empty
        };

        public bool IsEmpty => ItemId == 0 || Quantity <= 0;
    }

    [Serializable]
    public class ItemDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxStackSize { get; set; } = 99;
        public bool IsStackable => MaxStackSize > 1;
        public int MaxDurability { get; set; } = 100;
    }

    public interface IItemDatabase
    {
        ItemDefinition GetDefinition(int itemId);
    }

    public class InventoryTransaction
    {
        private readonly ItemInstance[] _originalSlots;
        private readonly ItemInstance[] _bufferedSlots;
        private readonly IItemDatabase _database;
        private bool _completed;

        public ItemInstance[] Slots => _bufferedSlots;

        public InventoryTransaction(ItemInstance[] currentSlots, IItemDatabase database)
        {
            _originalSlots = currentSlots;
            _bufferedSlots = (ItemInstance[])currentSlots.Clone();
            _database = database;
            _completed = false;
        }

        public bool AddItem(int itemId, int quantity)
        {
            if (_completed) throw new InvalidOperationException("Transaction already completed.");
            var def = _database.GetDefinition(itemId);
            if (def == null) return false;

            int remaining = quantity;

            // Step 1: Fill existing stackable slots
            if (def.IsStackable)
            {
                for (int i = 0; i < _bufferedSlots.Length; i++)
                {
                    if (_bufferedSlots[i].ItemId == itemId && _bufferedSlots[i].Quantity < def.MaxStackSize)
                    {
                        int space = def.MaxStackSize - _bufferedSlots[i].Quantity;
                        int addAmount = Math.Min(space, remaining);
                        _bufferedSlots[i].Quantity += addAmount;
                        remaining -= addAmount;

                        if (remaining <= 0) return true;
                    }
                }
            }

            // Step 2: Create new slots
            for (int i = 0; i < _bufferedSlots.Length; i++)
            {
                if (_bufferedSlots[i].IsEmpty)
                {
                    int addAmount = Math.Min(def.MaxStackSize, remaining);
                    _bufferedSlots[i] = new ItemInstance
                    {
                        ItemId = itemId,
                        Quantity = addAmount,
                        Durability = def.MaxDurability,
                        InstanceId = Guid.NewGuid()
                    };
                    remaining -= addAmount;

                    if (remaining <= 0) return true;
                }
            }

            return remaining == 0; // Returns false if inventory is full and couldn't fit the remainder
        }

        public bool RemoveItem(int itemId, int quantity)
        {
            if (_completed) throw new InvalidOperationException("Transaction already completed.");
            
            // Validate availability
            int count = 0;
            for (int i = 0; i < _bufferedSlots.Length; i++)
            {
                if (_bufferedSlots[i].ItemId == itemId)
                {
                    count += _bufferedSlots[i].Quantity;
                }
            }

            if (count < quantity) return false;

            // Deduct
            int remainingToRemove = quantity;
            for (int i = _bufferedSlots.Length - 1; i >= 0; i--)
            {
                if (_bufferedSlots[i].ItemId == itemId)
                {
                    if (_bufferedSlots[i].Quantity > remainingToRemove)
                    {
                        _bufferedSlots[i].Quantity -= remainingToRemove;
                        remainingToRemove = 0;
                        break;
                    }
                    else
                    {
                        remainingToRemove -= _bufferedSlots[i].Quantity;
                        _bufferedSlots[i] = ItemInstance.Empty;
                    }
                }
            }

            return remainingToRemove == 0;
        }

        public void Commit(out ItemInstance[] liveStorage)
        {
            if (_completed) throw new InvalidOperationException("Transaction already completed.");
            _completed = true;
            Array.Copy(_bufferedSlots, _originalSlots, _bufferedSlots.Length);
            liveStorage = _originalSlots;
        }
    }

    public class InventoryController
    {
        private ItemInstance[] _slots;
        private readonly IItemDatabase _database;

        public ItemInstance[] GetSlots() => _slots;

        public InventoryController(int size, IItemDatabase database)
        {
            _slots = new ItemInstance[size];
            for (int i = 0; i < size; i++)
            {
                _slots[i] = ItemInstance.Empty;
            }
            _database = database;
        }

        public bool TryProcessTransaction(Action<InventoryTransaction> transactionBlock)
        {
            var tx = new InventoryTransaction(_slots, _database);
            try
            {
                transactionBlock(tx);
                tx.Commit(out _slots);
                return true;
            }
            catch (Exception)
            {
                // Rollback automatically: do not commit the transaction, discard buffered copy
                return false;
            }
        }
    }
}
