using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        List<CGameUser> userList = new List<CGameUser>();

        public bool ExistsUser(int userId)
        {
            return userList.Exists(user => user.player.UserId == userId);
        }
        
        public void UserEntedServer(CGameUser user)
        {
            // 대기 리스트에 중복 추가 되지 않도록 체크.
            if (this.userList.Contains(user))
            {
                return;
            }

            // 서버에 유저 추가.
            userList.Add(user);

            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.player.UserId);
            user.send(response);

            AddNewPlayerRes(user);
        }

        public void AddNewPlayerRes(CGameUser newUser)
        {
            foreach (var user in userList)
            {
                if (user.player.UserId == newUser.player.UserId)
                    continue;
                
                CPacket response = CPacket.create((short)PROTOCOL.ADD_NEW_PLAYER_RES);
                response.push(newUser.player.UserId);
                response.push(newUser.player.CurrentPosX);
                response.push(newUser.player.CurrentPosY);
                user.send(response);
            }
        }

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

        public void ResponsePlayers(CGameUser user)
        {
            CPacket response = CPacket.create((short)PROTOCOL.PLAYERS_RES);
            response.push(userList.Count);

            for (int i = 0; i < userList.Count; ++i)
            {
                response.push(userList[i].player.UserId);
                response.push(userList[i].player.CurrentPosX);
                response.push(userList[i].player.CurrentPosY);
            }
            
            user.send(response);
        }

        public void RequestPlayerMove(CGameUser user)
        {
            var nearUsers = userList;
            
            foreach (var userData in nearUsers)
            {
                CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
                response.push(user.player.UserId);
                response.push(user.player.CurrentPosX);
                response.push(user.player.CurrentPosY);
                userData.send(response);
            }
        }
        
        // public void RequestPlayerMove(CGameUser user)
        // {
        //     CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
        //     
        //     var nearUsers = GameUtils.GetNearbyUsers(user, userList);
        //     
        //     response.push(nearUsers.Count);
        //     
        //     foreach (var userData in nearUsers)
        //     {
        //         response.push(userData.player.UserId);
        //         response.push(userData.player.CurrentPosX);
        //         response.push(userData.player.CurrentPosY);
        //     }
        //
        //     ResponsePacketUsers(nearUsers, response);
        // }

        public void ResponsePacketUsers(List<CGameUser> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user.send(response);
            }
        }
    }
}