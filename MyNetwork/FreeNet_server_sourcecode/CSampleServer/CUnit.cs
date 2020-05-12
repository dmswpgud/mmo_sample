using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public abstract class CUnit
    {
        public CGameUser owner;
        public PlayerData playerData = new PlayerData();
        public PlayerStateData stateData = new PlayerStateData();
        public HpMp HpMp = new HpMp();
        public List<CUnit> listNearbyUser = new List<CUnit>();

        public CUnit(CGameUser user)
        {
            owner = user;
        }

        public bool IsMonster()
        {
            return playerData.unitType == (byte) UnitType.MONSTER;
        }
        
        public CUnit() {}

        public abstract void UpdateNearUnit();
        public abstract void RemoveNearUnit(CUnit unit);
        public abstract void AddNearUnit(CUnit unit);
        public abstract void ResponseRemoveNearUnit(CUnit user, CUnit user2);
        public abstract void RequestPlayerMove();
        public abstract void ResponseAddNearUnit(CUnit user, CUnit user2);
        public abstract void SetPosition(int x, int y, int dir);
        public abstract void RequestPlayerState(int receiveUserId);
        public abstract void DisconnectedPlayer();
    }
}