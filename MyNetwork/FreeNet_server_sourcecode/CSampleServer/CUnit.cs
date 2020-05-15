using System;
using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    [Serializable]
    public abstract class CUnit
    {
        public CGameUser owner;
        public PlayerData playerData = new PlayerData();
        public PlayerStateData stateData = new PlayerStateData();
        public HpMp HpMp = new HpMp();
        public List<CUnit> prevNearUnits = new List<CUnit>();
        public int NearRange = 5;
        
        public CUnit() {}

        public CUnit(CGameUser user, PlayerDataPackage userPack)
        {
            owner = user;
            playerData = userPack.data;
            stateData = userPack.state;
            HpMp = userPack.hpMp;
        }

        public CUnit(PlayerDataPackage userPack)
        {
            playerData = userPack.data;
            stateData = userPack.state;
            HpMp = userPack.hpMp;
        }

        public bool IsPlayer() => playerData.unitType == (byte) UnitType.PLAYER;
        
        public List<CUnit> GetNearRangeUnit()
        {
            var list = MapManager.I.GetAllUnitByNearRange(stateData.posX, stateData.posY, NearRange);

            if (list.Contains(this))
            {
                var index = list.FindIndex(p => p == this);
                list.RemoveAt(index);
            }
            return list; 
        }

        public abstract void ResponseRemoveNearUnit(List<CUnit> units);
        public abstract void RequestPlayerMove();
        public abstract void ResponseAddNearUnit(List<CUnit> units);
        public abstract void SetPosition(int x, int y, int dir);
        public abstract void RequestPlayerState(int receiveUserId);
        public abstract void DisconnectedPlayer();
    }
}