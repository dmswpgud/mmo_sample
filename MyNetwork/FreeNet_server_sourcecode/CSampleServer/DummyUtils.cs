using GameServer;
using Newtonsoft.Json.Linq;

namespace CSampleServer
{
    public class DummyUtils
    {
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
                SpawnId = 2, MonsterId = 2, SpawnZonePosX = 1,SpawnZonePosY = 10, currentSpawnCount = 0, SpawnZoneRange = 3, SpawnMaxCount = 4, SpawnRemainSec = 8
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
    }
}