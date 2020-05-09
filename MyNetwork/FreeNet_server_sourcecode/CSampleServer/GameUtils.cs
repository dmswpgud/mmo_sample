using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public class GameUtils
    {
        public static List<CGameUser> GetNearbyUsers(CGameUser targetPlayer, List<CGameUser> listUSer)
        {
            int range = targetPlayer.player.NearRange;
            List<CGameUser> listNearUsers = new List<CGameUser>();
            
            for (int i = 0; i < listUSer.Count; ++i)
            {
                if (targetPlayer.player.UserId == listUSer[i].player.UserId)
                    continue;
                
                if ((targetPlayer.player.CurrentPosX + range >= listUSer[i].player.CurrentPosX &&
                     targetPlayer.player.CurrentPosX - range <= listUSer[i].player.CurrentPosX) &&
                    (targetPlayer.player.CurrentPosY + range >= listUSer[i].player.CurrentPosY &&
                     targetPlayer.player.CurrentPosY - range <= listUSer[i].player.CurrentPosY))
                {
                    listNearUsers.Add(listUSer[i]);
                }
            }

            return listNearUsers;
        }
        
        public static List<CGameUser> GetNearUserFromPosition(int x, int y, List<CGameUser> listUSer)
        {
            List<CGameUser> listNearUsers = listUSer.FindAll(p =>
                p.player.CurrentPosX == x &&
                p.player.CurrentPosY == y);

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

        public static int DamageCalculator(CPlayer attacker, CPlayer deffender)
        {
            return 1;
        }
    }
}