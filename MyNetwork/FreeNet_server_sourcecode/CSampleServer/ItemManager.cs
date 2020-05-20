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

                new CItem(instance, posX, posY);
            }

            public ItemInfo InitializedObjectData(ItemInfo info)
            {
                var uniqueId = Guid.NewGuid().GetHashCode();

                var item = new ItemInfo
                {
                    dataId = uniqueId,
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