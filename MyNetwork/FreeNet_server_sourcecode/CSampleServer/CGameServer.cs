using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CGameUser> userList = new List<CGameUser>();

        public bool ExistsUser(int userId)
        {
            return userList.Exists(user => user.player.playerData.playerId == userId);
        }

        // 서버 접속.
        public void UserEntedServer(CGameUser user)
        {
            // 서버에 유저 추가.
            userList.Add(user);

            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.player.playerData.playerId);
            user.send(response);
        }
        
        // 서버 접속 종료.
        public void DisconnectedUser(CGameUser user)
        {
            userList.Remove(user);
            
            foreach (var otherUser in userList)
            {
                CPacket response = CPacket.create((short)PROTOCOL.DISCONECTED_PLAYER_RES);
                response.push(user.player.playerData.playerId);
                otherUser.send(response);
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
                user.send(response);
            }
        }

        // 내 케릭 가져다주기.
        public void ResponseGetMyPlayer(CGameUser user)
        {
            CPacket response = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_RES);
            user.player.playerData.PushData(response);
            user.player.stateData.PushData(response);
            user.player.HpMp.PushData(response);
            user.send(response);
        }
        
        // 여러명에게 보내기.
        public static void ResponsePacketToUsers(List<CGameUser> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user.send(response);
            }
        }
    }
}