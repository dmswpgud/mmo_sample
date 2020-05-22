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
        public UnitInfosPackage monsterTableDatas = new UnitInfosPackage();
        public MonsterAiDatas monsterAiDatas = new MonsterAiDatas();
        public Dictionary<int, List<CMonster>> dicZonecurrentMoste = new Dictionary<int, List<CMonster>>();

        public void Initialized()
        {
            // 몬스터 테이블 로드.
            var jObj = SystemUtils.LoadJson(Program.monsterInfoJsonPath);
            monsterTableDatas = jObj.ToObject<UnitInfosPackage>();
            // 몬스터 스폰 데이터 로드.
            jObj = SystemUtils.LoadJson(Program.monsterSpawnInfoJsonPath);
            monsterSpawnDatas = jObj.ToObject<MonsterSpawnDatas>();
            // 몬스터 AI 데이터 로드.
            jObj = SystemUtils.LoadJson(Program.monsterAiInfoJsonPath);
            monsterAiDatas = jObj.ToObject<MonsterAiDatas>();

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
                var spawnPosX = (int)spawnData.SpawnZonePosX;
                var spawnPosY = (int)spawnData.SpawnZonePosY;
                MapManager.I.GetRandomPosition(spawnData.SpawnZonePosX, spawnData.SpawnZonePosY, spawnData.SpawnZoneRange, out spawnPosX, out spawnPosY);

                var monsterInfo = monsterTableDatas.datas.Find((p) => p.data.tableId == spawnData.MonsterId);

                if (monsterInfo != null)
                {
                    var monsterDataPack = CopyMonsterDataPack(monsterInfo);
                    monsterDataPack.state.posX = (short)spawnPosX;
                    monsterDataPack.state.posY = (short)spawnPosY;
                    
                    var monsterInstance = new CMonster(monsterDataPack.data, monsterDataPack.state, monsterDataPack.hpMp);
                    var aiInfo = monsterAiDatas.datas.Find((p) => p.dataId == spawnData.MonsterId);
                    monsterInstance.SetAiInfo(aiInfo);
                    AddMonster(spawnData.SpawnId, monsterInstance);
                    
                }
            }
        }

        private void AddMonster(int spawnId, CMonster instance)
        {
            if (dicZonecurrentMoste.ContainsKey(spawnId) == false)
            {
                dicZonecurrentMoste.Add(spawnId, new List<CMonster>());
            }

            dicZonecurrentMoste[spawnId].Add(instance);
            
            var spawnData = monsterSpawnDatas.datas.Find(p => p.SpawnId == spawnId);
            spawnData.currentSpawnCount += 1;
        }

        public void RemoveMonster(CMonster instance)
        {
            foreach (var data in dicZonecurrentMoste.Keys)
            {
                if (dicZonecurrentMoste[data].Contains(instance) == false)
                    continue;

                dicZonecurrentMoste[data].Remove(instance);
                var spawnData = monsterSpawnDatas.datas.Find(p => p.SpawnId == data);
                spawnData.currentSpawnCount -= 1;
            }
            
            instance = null;
        }

        public PlayerDataPackage CopyMonsterDataPack(PlayerDataPackage dataPack)
        {
            var uniqueId = Guid.NewGuid().GetHashCode();
            
            var monster = new PlayerDataPackage();
            var monsterdata = new UnitData();
            var monsterState = new UnitStateData();
            var monsterHp = new HpMp();

            monsterdata.UniqueId = uniqueId;
            monsterdata.name = dataPack.data.name;
            monsterdata.moveSpeed = dataPack.data.moveSpeed;
            monsterdata.tableId = dataPack.data.tableId;
            monsterdata.unitType = dataPack.data.unitType;

            monsterState.UniqueId = uniqueId;
            monsterState.direction = dataPack.state.direction;
            monsterState.state = dataPack.state.state;
            monsterState.posX = dataPack.state.posX;
            monsterState.posY = dataPack.state.posY;
            monsterState.unitType = dataPack.state.unitType;

            monsterHp.MaxHp = dataPack.hpMp.MaxHp;
            monsterHp.MaxMp = dataPack.hpMp.MaxMp;
            monsterHp.Hp = dataPack.hpMp.Hp;
            monsterHp.Mp = dataPack.hpMp.Mp;

            monster.data = monsterdata;
            monster.state = monsterState;
            monster.hpMp = monsterHp;
            
            return monster;
        }
    }
}