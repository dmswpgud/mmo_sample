using System;
using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CUnit> userList = new List<CUnit>();
        public Random random = new Random();

        private delegate void OnSpawnMonster();

        private OnSpawnMonster OnSpawnMonsterEvent;
        public MonsterSpawnDatas monsterSpawnDatas = new MonsterSpawnDatas();
        public UnitInfosPackage monsterInfoDatas = new UnitInfosPackage();

        public void Initialized()
        {
            Program.Tick = Tick;

            MapManager.I.Initialized();

            // DummyUtils.MonsterSpawnDummyData();
            // DummyUtils.MonsterDummyData();
            
            //
            InitializedMonsterDatas();
            InitializedSpawnMonster();
        }

        private void InitializedMonsterDatas()
        {
            var jObj = SystemUtils.LoadJson(Program.monsterInfoJsonPath);
            monsterInfoDatas = jObj.ToObject<UnitInfosPackage>();
        }

        private void InitializedSpawnMonster()
        {
            var jObj = SystemUtils.LoadJson(Program.monsterSpawnInfoJsonPath);
            monsterSpawnDatas = jObj.ToObject<MonsterSpawnDatas>();
            OnSpawnMonsterEvent += MonsterSpawn;
        }

        public void Tick()
        {
            OnSpawnMonsterEvent?.Invoke();
        }

        public bool ExistsUser(int userId)
        {
            return userList.Exists(user => user.playerData.playerId == userId);
        }

        // 서버 접속.
        public void UserEntedServer(CGameUser user)
        {
            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.userDataPackage.data.playerId);
            user?.send(response);
        }
        
        // 서버 접속 종료.
        public void DisconnectedUser(CUnit user)
        {
            lock (user)
            {
                userList.Remove(user);
            
                foreach (var otherUser in userList)
                {
                    CPacket response = CPacket.create((short)PROTOCOL.DISCONECTED_PLAYER_RES);
                    response.push(user.playerData.playerId);
                    otherUser?.owner?.send(response);
                }
            }
        }

        // 다른 유저에게 채팅 보내기
        public void SendChatMessage(CGameUser owner, string text)
        {
            foreach (var user in userList)
            {
                CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
                response.push(owner.player.playerData.playerId);
                response.push(text);
                user?.owner?.send(response);
            }
        }

        // 내 케릭 가져다주기.
        public void ResponseGetMyPlayer(CGameUser user)
        {
            // 유닛 생성
            var sponeUnis = new CPlayer(user, new PlayerDataPackage(user.userDataPackage.data, user.userDataPackage.state, user.userDataPackage.hpMp));
            // 유닛을 유저에 등록.
            user.player = sponeUnis;
            // 월드에 유닛 추가.
            userList.Add(sponeUnis);
            // 포지션 셋팅.
            sponeUnis.SetPosition(sponeUnis.stateData.posX, sponeUnis.stateData.posY, sponeUnis.stateData.direction);
            
            // 통신.
            CPacket response = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_RES);

            var list = MapManager.I.GetAllOtherUnit(user.player);
            var count = list.Count + 1;
            response.push(count);
            user.player.playerData.PushData(response);
            user.player.stateData.PushData(response);
            user.player.HpMp.PushData(response);
            
            foreach (var unit in user.player.prevNearUnits)
            {
                unit.playerData.PushData(response);
                unit.stateData.PushData(response);
                unit.HpMp.PushData(response);
            }

            user.send(response);

            foreach (var otherUnit in list)
            {
                otherUnit.ResponseAddNearUnit(new List<CUnit>(){user.player});
            }
            
            Program.PrintLog($"{user.player.playerData.playerId} 케릭 생성.");
        }
        
        // 여러명에게 보내기.
        public static void ResponsePacketToUsers(List<CUnit> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user?.owner?.send(response);
            }
        }
        
        public void MonsterSpawn()
        {
            foreach (var data in monsterSpawnDatas.datas)
            {
                if (data.currentSpawnCount >= data.SpawnMaxCount)
                    continue;
                
                if (data.LastSpawnTime != 0 && TimeManager.I.UtcTimeStampSeconds < data.NextSpawnTime)
                    continue;
                
                data.LastSpawnTime = TimeManager.I.UtcTimeStampSeconds;
                data.NextSpawnTime = TimeManager.I.UtcTimeStampSeconds + data.SpawnRemainSec;
                
                // TODO: 리스폰 포지션이 유효한지 체크해야됨...
                var spawnPosX = random.Next(data.SpawnZonePosX - data.SpawnZoneRange, data.SpawnZonePosX + data.SpawnZoneRange);
                var spawnPosY = random.Next(data.SpawnZonePosY - data.SpawnZoneRange, data.SpawnZonePosY + data.SpawnZoneRange);

                var monsterInfo = monsterInfoDatas.datas.Find((p) => p.data.tableId == data.MonsterId);

                if (monsterInfo != null)
                {
                    var monster = new PlayerDataPackage();
                    var monsterdata = new PlayerData();
                    var monsterState = new PlayerStateData();
                    var monsterHp = new HpMp();
                    var uniqueId = Guid.NewGuid().GetHashCode();
                    monsterdata.name = monsterInfo.data.name;
                    monsterdata.moveSpeed = monsterInfo.data.moveSpeed;
                    monsterdata.playerId = uniqueId;
                    monsterdata.tableId = monsterInfo.data.tableId;
                    monsterdata.unitType = monsterInfo.data.unitType;

                    monsterState.direction = monsterInfo.state.direction;
                    monsterState.state = monsterInfo.state.state;
                    monsterState.playerId = uniqueId;
                    monsterState.posX = (short)spawnPosX;
                    monsterState.posY = (short)spawnPosY;
                    monsterState.unitType = monsterInfo.state.unitType;

                    monsterHp.Hp = monsterInfo.hpMp.Hp;
                    monsterHp.Mp = monsterInfo.hpMp.Mp;

                    monster.data = monsterdata;
                    monster.state = monsterState;
                    monster.hpMp = monsterHp;
                    
                    userList.Add(new CMonster(monster));
                    
                    data.currentSpawnCount++;
                }
            }
        }
    }
}