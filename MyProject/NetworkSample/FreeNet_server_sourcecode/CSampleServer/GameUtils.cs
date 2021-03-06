using System.Collections.Generic;

namespace CSampleServer
{
    public class GameUtils
    {
        public static List<CGameUser> GetNearbyUsers(CGameUser targetPlayer, List<CGameUser> listUSer)
        {
            int range = 5;
            List<CGameUser> listNearUsers = new List<CGameUser>();
            
            for (int i = 0; i < listUSer.Count; ++i)
            {
                if ((targetPlayer.player.CurrentPosX + range > listUSer[i].player.CurrentPosX &&
                    targetPlayer.player.CurrentPosX - range < listUSer[i].player.CurrentPosX) &&
                    (targetPlayer.player.CurrentPosY + range > listUSer[i].player.CurrentPosY &&
                    targetPlayer.player.CurrentPosY - range < listUSer[i].player.CurrentPosY))
                {
                    listNearUsers.Add(listUSer[i]);
                }
            }

            return listNearUsers;
        }
    }
}