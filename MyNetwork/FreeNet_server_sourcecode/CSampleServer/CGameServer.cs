using System;
using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CUnit> userList = new List<CUnit>();

        public void Tick()
        {
            
        }

        public bool ExistsUser(int userId)
        {
            return userList.Exists(user => user.playerData.playerId == userId);
        }

        // 서버 접속.
        public void UserEntedServer(CUnit user)
        {
            //userList.Add(new CMonster(user.playerData.playerId + 13));
            
            // 서버에 유저 추가.
            userList.Add(user);

            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.playerData.playerId);
            user.owner?.send(response);
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
            CPacket response = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_RES);
            user.player.playerData.PushData(response);
            user.player.stateData.PushData(response);
            user.player.HpMp.PushData(response);
            user.send(response);
            
            Program.PrintLog($"{user.player.playerData.playerId} 케릭 생성.");
        }
        
        // 여러명에게 보내기.
        public static void ResponsePacketToUsers(List<CUnit> listUsers, CPacket response)
        {
            Console.WriteLine((PROTOCOL)response.protocol_id);
            foreach (var user in listUsers)
            {
                user?.owner?.send(response);
            }
        }
    }
}