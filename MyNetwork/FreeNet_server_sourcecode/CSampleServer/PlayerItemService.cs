using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public class PlayerItemService
    {
        List<ItemInfo> _items = new List<ItemInfo>();
        int[] _equipmentedItem = new int[7];

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
    }
}