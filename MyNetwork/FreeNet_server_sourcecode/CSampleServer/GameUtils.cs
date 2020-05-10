using System;
using System.Collections.Generic;
using System.Linq;
using GameServer;

namespace CSampleServer
{
    public class GridPoint 
    {
        public int X, Y;
        public GridPoint(int x, int y) { X = x; Y = y; }
    }
    
    public class GameUtils
    {
        public static List<CGameUser> GetNearbyUsers(CGameUser targetPlayer, List<CGameUser> listUSer)
        {
            int range = targetPlayer.player.playerData.nearRange;
            List<CGameUser> listNearUsers = new List<CGameUser>();
            
            for (int i = 0; i < listUSer.Count; ++i)
            {
                if (targetPlayer.player.playerData.playerId == listUSer[i].player.playerData.playerId)
                    continue;
                
                if ((targetPlayer.player.stateData.posX + range >= listUSer[i].player.stateData.posX &&
                     targetPlayer.player.stateData.posX - range <= listUSer[i].player.stateData.posX) &&
                    (targetPlayer.player.stateData.posY + range >= listUSer[i].player.stateData.posY &&
                     targetPlayer.player.stateData.posY - range <= listUSer[i].player.stateData.posY))
                {
                    listNearUsers.Add(listUSer[i]);
                }
            }

            return listNearUsers;
        }
        
        public static List<CGameUser> GetNearUserFromPosition(int x, int y, List<CGameUser> listUSer)
        {
            List<CGameUser> listNearUsers = listUSer.FindAll(p =>
                p.player.stateData.posX == x &&
                p.player.stateData.posY == y);

            return listNearUsers;
        }
        
        public static GridPoint GetFrontPositionUnit(UnitDirection dir, int x, int y)
        {
            switch (dir)
            {
                case UnitDirection.UP:
                    return new GridPoint(x, y + 1);
                case UnitDirection.UP_RIGHT:
                    return new GridPoint(x + 1, y + 1);
                case UnitDirection.RIGHT:
                    return new GridPoint(x + 1, y);
                case UnitDirection.DOWN_RIGHT:
                    return new GridPoint(x + 1, y - 1);
                case UnitDirection.DOWN:
                    return new GridPoint(x, y - 1);
                case UnitDirection.DOWN_LEFT:
                    return new GridPoint(x - 1, y - 1);
                case UnitDirection.LEFT:
                    return new GridPoint(x - 1, y);
                case UnitDirection.UP_LEFT:
                    return new GridPoint(x - 1, y + 1);
            }
        
            return null;
        }

        public static Int32 DamageCalculator(CPlayer attacker, CPlayer deffender)
        {
            return deffender.HpMp.Hp -= 10;
        }
        
        public CGameUser GetFrontPositionTarget(CGameUser user, List<CGameUser> nearUsers)
        {
            var frontPosition = GetFrontPositionUnit((UnitDirection)user.player.stateData.direction, user.player.stateData.posX, user.player.stateData.posY);

            var target = nearUsers.Find(p => p.player.stateData.posX == frontPosition.X && p.player.stateData.posY == frontPosition.Y);

            return target;
        }
        
        public CGameUser GetTargetUserFromUserId(int targetUserId, List<CGameUser> userList)
        {
            return userList.Find(p => p.player.playerData.playerId == targetUserId);
        }

        public static List<CGameUser> GetTotlNearUserList(List<CGameUser> listA, List<CGameUser> listB)
        {
            List<CGameUser> listResult = new List<CGameUser>();
            // A에 없는 B의 유저 리스트.
            var defenderList = listB.Where(i => !listA.Contains(i)).ToList();
            listResult.AddRange(listA);
            listResult.AddRange(defenderList);

            return listResult;
        }
    }
}