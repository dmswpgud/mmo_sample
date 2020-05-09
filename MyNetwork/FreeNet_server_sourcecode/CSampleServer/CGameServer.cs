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
            PushPlayerrData(user, response);
            user.send(response);
        }
        
        // 플레이어가 범위내에 있을 시 알리기.
        public void ResponseAddNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            PushPlayerrData(user2, response);
            user.send(response);
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public void ResponseRemoveNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            PushPlayerrData(user2, response);
            user.send(response);
        }

        // 플레이어 이동.
        public void RequestPlayerMove(CGameUser user)
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            PushPlayerrData(user, response);
            user.send(response);
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var userData in user.player.listNearbyUser)
            {
                PushPlayerrData(user, response);
                userData.send(response);
            }
        }

        // 여러명에게 보내기.
        public void ResponsePacketUsers(List<CGameUser> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user.send(response);
            }
        }

        // 유저 패킷 패키징.
        public void PushPlayerrData(CGameUser user, CPacket response)
        {
            response.push(user.player.UserId);
            response.push(user.player.MoveSpeed);
            response.push(user.player.NearRange);
            response.push(user.player.CurrentPosX);
            response.push(user.player.CurrentPosY);
            response.push(user.player.Direction);
        }
    }
}