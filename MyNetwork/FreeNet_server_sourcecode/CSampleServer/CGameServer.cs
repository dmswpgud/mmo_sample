using System.Collections.Generic;
using CSampleServer.DefaultNamespace;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CUnit> _listUnit = new List<CUnit>();

        public void Initialized()
        {
            DummyUtils.DummyLoad();
            
            //
            MapManager.I.Initialized();
            MonsterManager.I.Initialized();
            ItemManager.I.Initialized();
        }

        public bool ExistsUser(int userId)
        {
            return _listUnit.Exists(user => user.UnitData.UniqueId == userId);
        }

        // 서버 접속.
        public void UserEntedServer(CGameUser user)
        {
            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.userDataPackage.data.UniqueId);
            user?.send(response);
        }
        
        // 서버 접속 종료.
        public void DisconnectedUnit(CUnit unit)
        {
            lock (unit)
            {
                foreach (var otherUser in _listUnit)
                {
                    CPacket response = CPacket.create((short)PROTOCOL.DISCONECTED_PLAYER_RES);
                    response.push(unit.UnitData.UniqueId);
                    otherUser?.Owner?.send(response);
                }
            }
        }

        // 다른 유저에게 채팅 보내기
        public void SendChatMessage(CGameUser owner, string text)
        {
            foreach (var user in _listUnit)
            {
                CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
                response.push(owner.player.UnitData.UniqueId);
                response.push(text);
                user?.Owner?.send(response);
            }
        }

        // 내 케릭 가져다주기.
        public void ResponseGetMyPlayer(CGameUser user)
        {
            // 유닛 생성
            var playerInstance = new CPlayer(user.userDataPackage.data, user.userDataPackage.state, user.userDataPackage.hpMp);
            playerInstance.Initialized(user);
            // 유닛을 유저에 등록.
            user.player = playerInstance;
            // 월드에 유닛 추가.
            PlayerManager.I.AddPlayer(playerInstance);
            // 포지션 셋팅.
            playerInstance.SetPosition(playerInstance.StateData.posX, playerInstance.StateData.posY, playerInstance.DIRECTION);
            
            // 통신.
            CPacket response = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_RES);

            var list = MapManager.I.GetAllOtherUnit(user.player);
            var count = list.Count + 1;
            response.push(count);
            user.player.UnitData.PushData(response);
            user.player.StateData.PushData(response);
            user.player.HpMp.PushData(response);
            
            foreach (var unit in user.player.prevNearUnits)
            {
                unit.UnitData.PushData(response);
                unit.StateData.PushData(response);
                unit.HpMp.PushData(response);
            }

            user.send(response);

            foreach (var otherUnit in list)
            {
                otherUnit.ResponseAddNearUnit(new List<CUnit>(){user.player});
            }
            
            Program.PrintLog($"{user.player.UnitData.UniqueId} 케릭 생성.");
        }
        
        // 여러명에게 보내기.
        public static void ResponseToUsers(List<CUnit> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user?.Owner?.send(response);
            }
        }
    }
}