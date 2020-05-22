using System;
using GameServer;

namespace CSampleServer
{
    namespace DefaultNamespace
    {
        public class ItemManager
        {
            private static ItemManager instance = null;
            public static ItemManager I
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ItemManager();
                    }
                    return instance;
                }
            }
            
            public ItemInfoPackage itemTableDatas = new ItemInfoPackage();

            public void Initialized()
            {
                // 몬스터 테이블 로드.
                var jObj = SystemUtils.LoadJson(Program.itemInfoJsonPath);
                itemTableDatas = jObj.ToObject<ItemInfoPackage>();
            }

            public void CreateItem(int itemId, int posX, int posY)
            {
                var itemInfo = itemTableDatas.datas.Find(info => info.tableId == itemId);
                
                if (itemInfo == null)
                    return;
                
                var instance = InitializedObjectData(itemInfo);

                var data = new UnitData {UniqueId = instance.uniqueId, name = instance.itemName, unitType = (byte)UnitType.ITEM};
                var state = new UnitStateData {posX = (short)posX, posY = (short)posY};
                var hpMp = new HpMp();
                
                var item = new CItem(data, state, hpMp);
                item.SetItemInfo(instance);
            }

            public void RemoveItem(CItem item)
            {
                item = null;
            }

            public CItem GetItemFromPosition(int uniqueItemId, int posX, int posY)
            {
                var units = MapManager.I.GetAllUnitByNearRange(posX, posY, 1);

                if (units != null)
                {
                    var items = units.FindAll(p => p.TYPE == UnitType.ITEM);

                    if (items != null && items.Count != 0)
                    {
                        return (CItem)items.Find(p => p.UNIQUE_ID == uniqueItemId);        
                    }
                }
                
                return null;
            }

            public ItemInfo InitializedObjectData(ItemInfo info)
            {
                var uniqueId = Guid.NewGuid().GetHashCode();

                var item = new ItemInfo
                {
                    uniqueId = uniqueId,
                    tableId = info.tableId,
                    itemName = info.itemName,
                    itemType = info.itemType,
                    price = info.price,
                    sellPrice = info.sellPrice,
                    material = info.material,
                    damage = info.damage,
                    hitmodifier = info.hitmodifier,
                    ac = info.ac,
                    useX = info.useX,
                    safenchant = info.safenchant,
                    consume_type = info.consume_type,
                    count = info.count,
                    stackable = info.stackable,
                };
                return item;
            }
        }
    }
}