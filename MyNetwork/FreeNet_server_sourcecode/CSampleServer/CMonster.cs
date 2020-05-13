using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CMonster : CUnit
    {
        public CMonster(int id)
        {
            SpawnMonster(id);
        }
        
        public CMonster() : base()
        {
            playerData.playerId = 100;
            playerData.name = "몬스터" + playerData.playerId.ToString();
            playerData.unitType = 1;
            playerData.moveSpeed = 1;

            stateData.playerId = 100;
            stateData.direction = 0;
            stateData.posX = 13;
            stateData.posY = 13;

            HpMp.Hp = 100;
            HpMp.Mp = 200;
            
            listNearbyUser = GameUtils.GetNearbyUnit(playerData.playerId, stateData.posX, stateData.posY, playerData.nearRange, Program.gameServer.userList);
            
            CPacket response = CPacket.create((short)PROTOCOL.MONSTER_SPONE_RES);
            playerData.PushData(response);
            stateData.PushData(response);
            CGameServer.ResponsePacketToUsers(listNearbyUser, response);

            SetPosition(stateData.posX, stateData.posY, stateData.direction);
        }

        private void SpawnMonster(int id)
        {
            playerData.playerId = id;
            playerData.name = $"몬스터{id}";
            playerData.unitType = 1;
            playerData.moveSpeed = 1;

            stateData.playerId = id;
            stateData.direction = 2;
            stateData.posX = (short) 15;
            stateData.posY = (short) 15;

            HpMp.Hp = 100;
            HpMp.Mp = 200;
            
            listNearbyUser = GameUtils.GetNearbyUnit(playerData.playerId, stateData.posX, stateData.posY, playerData.nearRange, Program.gameServer.userList);
            
            CPacket response = CPacket.create((short)PROTOCOL.MONSTER_SPONE_RES);
            playerData.PushData(response);
            stateData.PushData(response);
            CGameServer.ResponsePacketToUsers(listNearbyUser, response);

            SetPosition(stateData.posX, stateData.posY, stateData.direction);
        }
        
        public override void SetPosition(int x, int y, int dir)
        {
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            UpdateNearUnit();
        }

        public override void UpdateNearUnit()
        {
            var updateNearUnit = GameUtils.GetNearbyUnit(playerData.playerId, stateData.posX, stateData.posY, playerData.nearRange, Program.gameServer.userList);
            var list1 = listNearbyUser.Where(i => !updateNearUnit.Contains(i)).ToList(); // 삭제된 플레이어 리스트
            var list2 = updateNearUnit.Where(i => !listNearbyUser.Contains(i)).ToList(); // 새로 추가된 플레이어 리스트
            
            // 범위내 케릭터 삭제 요청.
            foreach (var user in list1)
            {
                user.RemoveNearUnit(this);
                ResponseRemoveNearUnit(user, this);
                ResponseRemoveNearUnit(this, user);
            }

            // 범위내 케릭터 추가 요청.
            foreach (var user in list2)
            {
                user.AddNearUnit(this);
                ResponseAddNearUnit(user, this);
                ResponseAddNearUnit(this, user);
            }
           
            // 범위내 케릭터 갱신.
            listNearbyUser.Clear();
            listNearbyUser.AddRange(updateNearUnit);
        }

        public override void RemoveNearUnit(CUnit unit)
        {
        }

        public override void AddNearUnit(CUnit unit)
        {
        }

        public override void ResponseRemoveNearUnit(CUnit user, CUnit user2)
        {
        }

        public override void RequestPlayerMove()
        {
        }

        public override void ResponseAddNearUnit(CUnit user, CUnit user2)
        {
        }

        public override void RequestPlayerState(int receiveUserId)
        {
        }

        public override void DisconnectedPlayer()
        {
        }
    }
}