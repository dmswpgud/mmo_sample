using GameServer;
using Newtonsoft.Json.Linq;

namespace CSampleServer
{
    public class DummyUtils
    {
        public static void DummyLoad()
        {
            // MonsterSpawnDummyData();
            // MonsterDummyData();
            // MonsterAiDummyData();
            // ItemDummyData();
        }
        
        public static void MonsterAiDummyData()
        {
            MonsterAiData data1;
            data1 = new MonsterAiData();
            data1.dataId = 1;
            data1.behaviorPatternSpeed = 1;
            data1.searchTargetRange = 3;
            data1.attackFirst = 0;
            
            MonsterAiData data2;
            data2 = new MonsterAiData();
            data2.dataId = 2;
            data2.behaviorPatternSpeed = 1;
            data2.searchTargetRange = 3;
            data2.attackFirst = 0;
            
            var datas = new MonsterAiDatas();
            datas.datas.Add(data1);
            datas.datas.Add(data2);

            var obj = JObject.FromObject(datas);
            SystemUtils.SaveJson(Program.monsterAiInfoJsonPath, obj);
        }
        public static void MonsterSpawnDummyData()
        {
            MonsterSawnData data1;
            MonsterSawnData data2;
            
            data1 = new MonsterSawnData()
            {
                SpawnId = 1, MonsterId = 1, SpawnZonePosX = 15, SpawnZonePosY = 15, currentSpawnCount = 0, SpawnZoneRange = 3, SpawnMaxCount = 3, SpawnRemainSec = 5
            };
            
            data2 = new MonsterSawnData()
            {
                SpawnId = 2, MonsterId = 2, SpawnZonePosX = 10, SpawnZonePosY = 10, currentSpawnCount = 0, SpawnZoneRange = 3, SpawnMaxCount = 4, SpawnRemainSec = 8
            };

            var datas = new MonsterSpawnDatas();
            datas.datas.Add(data1);
            datas.datas.Add(data2);
            var obj = JObject.FromObject(datas);
            SystemUtils.SaveJson(Program.monsterSpawnInfoJsonPath, obj);
        }
        
        public static void MonsterDummyData()
        {
            var data = new PlayerData
            {
                tableId = 1,
                playerId = 0,
                name = "오크",
                unitType = (byte)UnitType.MONSTER,
                moveSpeed = 1,
            };
            var state = new PlayerStateData
            {
                playerId = 1,
                direction = 0,
                unitType = (byte)UnitType.MONSTER,
                posX = 0,
                posY = 0,
            };
            var hpMp = new HpMp()
            {
                MaxHp = 50,
                MaxMp = 10,
                Hp = 50,
                Mp = 10,
            };
            var dataPack = new PlayerDataPackage()
            {
                data = data,
                state = state,
                hpMp = hpMp,
            };
            
            //
            var data2 = new PlayerData
            {
                tableId = 2,
                playerId = 0,
                name = "늑대인간",
                unitType = (byte)UnitType.MONSTER,
                moveSpeed = 1,
            };
            var state2 = new PlayerStateData
            {
                playerId = 2,
                direction = 0,
                unitType = (byte)UnitType.MONSTER,
                posX = 0,
                posY = 0,
            };
            var hpMp2 = new HpMp()
            {
                MaxHp = 80,
                MaxMp = 10,
                Hp = 80,
                Mp = 10,
            };
            var dataPack2 = new PlayerDataPackage()
            {
                data = data2,
                state = state2,
                hpMp = hpMp2,
            };

            var packages = new UnitInfosPackage();
            packages.datas.Add(dataPack);
            packages.datas.Add(dataPack2);
            
            var obj2 = JObject.FromObject(packages);
            SystemUtils.SaveJson(Program.monsterInfoJsonPath, obj2);
        }

        public static void ItemDummyData()
        {
            var pack = new ItemInfoPackage();
            
            var weapone = new ItemInfo
            {
                tableId = 1,
                dataId = 0,
                itemName = "식칼",
                ac = 0,
                damage = 10,
                hitmodifier = 2,
                itemType = (byte)ItemType.WEAPONE,
                material = 0,
                price = 20000,
                sellPrice = 5000,
                safenchant = 6,
            };
            
            var armor = new ItemInfo
            {
                tableId = 2,
                dataId = 0,
                itemName = "앞치마",
                ac = 3,
                damage = 0,
                hitmodifier = 0,
                itemType = (byte)ItemType.ARMOR,
                material = 0,
                price = 10000,
                sellPrice = 2000,
                safenchant = 4,
            };
            
            var etc = new ItemInfo
            {
                tableId = 2,
                dataId = 0,
                itemName = "박카스",
                itemType = (byte)ItemType.POTION,
                consume_type = 1,
                count = 1,
                stackable = 1,
                price = 10000,
                sellPrice = 2000,
            };

            pack.datas.Add(weapone);
            pack.datas.Add(armor);
            pack.datas.Add(etc);
            
            var obj2 = JObject.FromObject(pack);
            SystemUtils.SaveJson(Program.itemInfoJsonPath, obj2);
        }
    }
}