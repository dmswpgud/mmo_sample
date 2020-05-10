using System;
using System.Collections.Generic;
using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class GridPoint 
    {
        public int X, Y;

        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    
    public class CPlayer
    {
        CGameUser owner;

        public Int32 UserId;
        public Int32 MoveSpeed;
        public Int32 NearRange;
        public Int32 CurrentPosX {get; set;}
        public Int32 CurrentPosY {get; set;}
        public Int32 unitDirection {get; set;}
        public Int32 playerState;

        public List<CGameUser> listNearbyUser = new List<CGameUser>();
        
        public CPlayer(CGameUser user, int userId)
        {
            owner = user;
            UserId = userId;
        }

        public void SetPlayer(CPacket msg)
        {
            UserId = msg.pop_int32();
            MoveSpeed = msg.pop_int32();
            NearRange = msg.pop_int32();
            CurrentPosX = msg.pop_int32();
            CurrentPosY = msg.pop_int32();
            unitDirection = msg.pop_int32();
            playerState = msg.pop_int32();
        }

        public void SetPosition(int x, int y, int dir)
        {
            CurrentPosX = x;
            CurrentPosY = y;
            unitDirection = dir;
            
           var updateNearPlayers = GameUtils.GetNearbyUsers(owner, Program.gameServer.userList);
           var list1 = listNearbyUser.Where(i => !updateNearPlayers.Contains(i)).ToList(); // 삭제된 플레이어 리스트
           var list2 = updateNearPlayers.Where(i => !listNearbyUser.Contains(i)).ToList(); // 새로 추가된 플레이어 리스트

           // 범위내 케릭터 삭제 요청.
           foreach (var user in list1)
           {
               user.player.RemoveNearPlayer(owner);
               Program.gameServer.ResponseRemoveNearPlayer(user, owner);
               Program.gameServer.ResponseRemoveNearPlayer(owner, user);
           }

           // 범위내 케릭터 추가 요청.
           foreach (var user in list2)
           {
               user.player.AddNearPlayer(owner);
               Program.gameServer.ResponseAddNearPlayer(user, owner);
               Program.gameServer.ResponseAddNearPlayer(owner, user);
           }
           
           // 범위내 케릭터 갱신.
           listNearbyUser.Clear();
           listNearbyUser.AddRange(updateNearPlayers);
        }

        // 범위내 케릭터 추가.
        public void AddNearPlayer(CGameUser user)
        {
            listNearbyUser.Add(user);
        }

        // 범위내 케릭터 삭제.
        public void RemoveNearPlayer(CGameUser user)
        {
            listNearbyUser.Remove(user);
        }

        // 접속 종료.
        public void DisconnectedPlayer()
        {
            // 접속 종료되었으니 범위내에 다른 플레이어들에게 나를 삭제해달라고 요청.
            foreach (var user in listNearbyUser)
            {
                user.player.RemoveNearPlayer(owner);
            }
        }

        public CGameUser GetFrontPositionTarget()
        {
            var listTarget = GameUtils.GetNearbyUsers(owner, Program.gameServer.userList);

            var frontPosition = GameUtils.GetFrontPositionUnit((UnitDirection)unitDirection, CurrentPosX, CurrentPosY);

            var target = listTarget.Find(p => p.player.CurrentPosX == frontPosition.X && p.player.CurrentPosY == frontPosition.Y);

            return target;
        }
        

    }
}