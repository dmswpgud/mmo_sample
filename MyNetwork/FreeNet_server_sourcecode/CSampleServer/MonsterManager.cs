using System;
using System.Collections.Generic;

namespace CSampleServer
{
    public class MonsterManager
    {
        private static MonsterManager instance = null;
        public static MonsterManager I
        {
            get
            {
                if (instance == null)
                {
                    instance = new MonsterManager();
                }
                return instance;
            }
        }
        
        public Random random = new Random();
        public MonsterSpawnDatas monsterSpawnDatas = new MonsterSpawnDatas();
        public UnitInfosPackage monsterInfoDatas = new UnitInfosPackage();
        public Dictionary<MonsterSawnData, List<CMonster>> dicZonecurrentMoste = new Dictionary<MonsterSawnData, List<CMonster>>();

        public void Initialized()
        {
            // 몬스터 테이블 로드.
            var jObj = SystemUtils.LoadJson(Program.monsterInfoJsonPath);
            monsterInfoDatas = jObj.ToObject<UnitInfosPackage>();
            // 몬스터 스폰 데이터 로드.
            jObj = SystemUtils.LoadJson(Program.monsterSpawnInfoJsonPath);
            monsterSpawnDatas = jObj.ToObject<MonsterSpawnDatas>();

            Program.Tick += Tick;
        }

        void Tick()
        {
            MonsterSpawnProccess();
        }
        
        public void MonsterSpawnProccess()
        {
            foreach (var spawnData in monsterSpawnDatas.datas)
            {
                // 몬스터 생성 재한.
                if (spawnData.currentSpawnCount >= spawnData.SpawnMaxCount)
                    continue;
                // 몬스터 생성 시간 간격.
                if (spawnData.LastSpawnTime != 0 && TimeManager.I.UtcTimeStampSeconds < spawnData.NextSpawnTime)
                    continue;
                // 생성 시간 기록, 다음 생성시간 셋팅.
                spawnData.LastSpawnTime = TimeManager.I.UtcTimeStampSeconds;
                spawnData.NextSpawnTime = TimeManager.I.UtcTimeStampSeconds + spawnData.SpawnRemainSec;
                
                // TODO: 리스폰 포지션이 유효한지 체크해야됨...
                var spawnPosX = random.Next(spawnData.SpawnZonePosX - spawnData.SpawnZoneRange, spawnData.SpawnZonePosX + spawnData.SpawnZoneRange);
                var spawnPosY = random.Next(spawnData.SpawnZonePosY - spawnData.SpawnZoneRange, spawnData.SpawnZonePosY + spawnData.SpawnZoneRange);

                var monsterInfo = monsterInfoDatas.datas.Find((p) => p.data.tableId == spawnData.MonsterId);

                if (monsterInfo != null)
                {
                    var monsterDataPack = CopyMonsterDataPack(monsterInfo);
                    monsterDataPack.state.posX = (short)spawnPosX;
                    monsterDataPack.state.posY = (short)spawnPosY;
                    
                    var monsterInstance = new CMonster(monsterDataPack);
                    AddMonster(spawnData, monsterInstance);
                    spawnData.currentSpawnCount += 1;
                }
            }
        }

        private void AddMonster(MonsterSawnData spawnData, CMonster instance)
        {
            if (dicZonecurrentMoste.ContainsKey(spawnData) == false)
            {
                dicZonecurrentMoste.Add(spawnData, new List<CMonster>());
            }

            dicZonecurrentMoste[spawnData].Add(instance);

            Program.gameServer.userList.Add(instance);
        }

        public void RemoveMonster(CMonster instance)
        {
            foreach (var spawnData in dicZonecurrentMoste.Keys)
            {
                dicZonecurrentMoste[spawnData].Remove(instance);
                spawnData.currentSpawnCount -= 1;
            }

            Program.gameServer.userList.Remove(instance);
            
            instance = null;
        }

        public PlayerDataPackage CopyMonsterDataPack(PlayerDataPackage dataPack)
        {
            var uniqueId = Guid.NewGuid().GetHashCode();
            
            var monster = new PlayerDataPackage();
            var monsterdata = new PlayerData();
            var monsterState = new PlayerStateData();
            var monsterHp = new HpMp();

            monsterdata.playerId = uniqueId;
            monsterdata.name = dataPack.data.name;
            monsterdata.moveSpeed = dataPack.data.moveSpeed;
            monsterdata.tableId = dataPack.data.tableId;
            monsterdata.unitType = dataPack.data.unitType;

            monsterState.playerId = uniqueId;
            monsterState.direction = dataPack.state.direction;
            monsterState.state = dataPack.state.state;
            monsterState.posX = dataPack.state.posX;
            monsterState.posY = dataPack.state.posY;
            monsterState.unitType = dataPack.state.unitType;

            monsterHp.Hp = dataPack.hpMp.Hp;
            monsterHp.Mp = dataPack.hpMp.Mp;

            monster.data = monsterdata;
            monster.state = monsterState;
            monster.hpMp = monsterHp;
            
            return monster;
        }
    }
}