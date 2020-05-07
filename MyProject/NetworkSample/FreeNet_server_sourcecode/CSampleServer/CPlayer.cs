using System;
using System.Collections.Generic;

namespace CSampleServer
{
    public class CPlayer
    {
        CGameUser owner;

        public Int32 UserId;

        public Int32 CurrentPosX;

        public Int32 CurrentPosY;

        public List<CGameUser> listNearbyUser;
        
        public CPlayer(CGameUser user, int userId)
        {
            this.owner = user;
            
            UserId = userId;
            
            Random random = new Random();

            CurrentPosX = 0;

            CurrentPosY = 0;
        }
    }
}