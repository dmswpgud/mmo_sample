using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CUnit> userList = new List<CUnit>();

        public void Initialized()
        {
            DummyUtils.DummyLoad();
            
            //
            MapManager.I.Initialized();
            MonsterManager.I.Initialized();
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
        public void DisconnectedUnit(CUnit unit)
        {
            lock (unit)
            {
                foreach (var otherUser in userList)
                {
                    CPacket response = CPacket.create((short)PROTOCOL.DISCONECTED_PLAYER_RES);
                    response.push(unit.playerData.playerId);
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
            var playerInstance = new CPlayer(user, new PlayerDataPackage(user.userDataPackage.data, user.userDataPackage.state, user.userDataPackage.hpMp));
            // 유닛을 유저에 등록.
            user.player = playerInstance;
            // 월드에 유닛 추가.
            PlayerManager.I.AddPlayer(playerInstance);
            // 포지션 셋팅.
            playerInstance.SetPosition(playerInstance.stateData.posX, playerInstance.stateData.posY, playerInstance.stateData.direction);
            
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
        public static void ResponseToUsers(List<CUnit> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user?.owner?.send(response);
            }
        }
    }
}