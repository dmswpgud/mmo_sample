using System.Collections.Generic;
using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CGameUser> userList = new List<CGameUser>();

        public bool ExistsUser(int userId)
        {
            return userList.Exists(user => user.player.UserId == userId);
        }

        // 서버 접속.
        public void UserEntedServer(CGameUser user)
        {
            // 서버에 유저 추가.
            userList.Add(user);

            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.player.UserId);
            user.send(response);
        }
        
        // 서버 접속 종료.
        public void DisconnectedUser(CGameUser user)
        {
            userList.Remove(user);
            
            foreach (var otherUser in userList)
            {
                CPacket response = CPacket.create((short)PROTOCOL.DISCONECTED_PLAYER_RES);
                response.push(user.player.UserId);
                otherUser.send(response);
            }
        }

        // 다른 유저에게 채팅 보내기
        public void SendChatMessage(CGameUser owner, string text)
        {
            foreach (var user in userList)
            {
                CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
                response.push(owner.player.UserId);
                response.push(text);
                user.send(response);
            }
        }

        // 내 케릭 가져다주기.
        public void ResponseGetMyPlayer(CGameUser user)
        {
            CPacket response = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_RES);
            PushPlayerData(user, response);
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

        // 유저 패킷 패키징.
        public static void PushPlayerData(CGameUser user, CPacket response)
        {
            response.push(user.player.UserId);
            response.push(user.player.MoveSpeed);
            response.push(user.player.NearRange);
            response.push(user.player.CurrentPosX);
            response.push(user.player.CurrentPosY);
            response.push(user.player.unitDirection);
            response.push(user.player.playerState);
        }
    }
}