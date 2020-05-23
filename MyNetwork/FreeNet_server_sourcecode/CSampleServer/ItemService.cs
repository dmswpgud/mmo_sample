using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class ItemService
    {
        private CUnit _owner; 
        List<ItemInfo> _items = new List<ItemInfo>();
        int[] _equipmentedItem = new int[7];

        public ItemService(CUnit unit = null)
        {
            _owner = unit;
        }

        public ItemInfo AddItem(ItemInfo item)
        {
            // 중첩 가능한 아이템이라면 갯수만 늘려준다.
            if (item.stackable == 1)
            {
                var targetItem = _items.Find(p => p.tableId == item.tableId);
                
                if (targetItem != null)
                {
                    targetItem.count += item.count;
                    return targetItem;
                }
            }
            
            _items.Add(item);
            return item;
        }

        public ItemInfo GetItem(int uniqueId)
        {
            return _items.Find(p => p.uniqueId == uniqueId);
        }

        public void RemoveItem(ItemInfo item)
        {
            _items.Remove(item);
        }

        public void EquipItem(ItemType type, int tableId)
        {
            if (_items.Exists(p => p.tableId == tableId))
            {
                _equipmentedItem[(int) type] = tableId;    
            }
        }

        public void UseItem(int itemId)
        {
            var useItem = _items.Find(p => p.uniqueId == itemId);
            
            if (useItem == null)
                return;

            switch ((ItemType)useItem.itemType)
            {
                case ItemType.POTION:
                {
                    useItem.count-= 1;
                    _owner.RecoveryHp(50);
                    CPacket response = CPacket.create((short)PROTOCOL.USE_ITEM_RES);
                    useItem.PushData(response);
                    _owner.UnitData.PushData(response);
                    _owner.StateData.PushData(response);
                    _owner.HpMp.PushData(response);
                    _owner.Owner.send(response);
                    break;
                }
            }
        }
    }
}