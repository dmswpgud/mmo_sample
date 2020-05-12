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
        public static List<CUnit> GetNearbyUnit(int id, int x, int y, int range, List<CUnit> listUSer)
        {
            List<CUnit> listNearUsers = new List<CUnit>();
            
            for (int i = 0; i < listUSer.Count; ++i)
            {
                if (id == listUSer[i].playerData.playerId)
                    continue;
                
                if ((x + range >= listUSer[i].stateData.posX &&
                     x - range <= listUSer[i].stateData.posX) &&
                    (y + range >= listUSer[i].stateData.posY &&
                     y - range <= listUSer[i].stateData.posY))
                {
                    listNearUsers.Add(listUSer[i]);
                }
            }

            return listNearUsers;
        }

        public static List<CUnit> GetNearUserFromPosition(int x, int y, List<CUnit> listUSer)
        {
            List<CUnit> listNearUsers = listUSer.FindAll(p =>
                p.stateData.posX == x &&
                p.stateData.posY == y);

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

        public static Int32 DamageCalculator(CUnit attacker, CUnit deffender)
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

        public static List<CUnit> GetTotlNearUserList(List<CUnit> listA, List<CUnit> listB)
        {
            List<CUnit> listResult = new List<CUnit>();
            // A에 없는 B의 유저 리스트.
            var defenderList = listB.Where(i => !listA.Contains(i)).ToList();
            listResult.AddRange(listA);
            listResult.AddRange(defenderList);

            return listResult;
        }
    }
}