using System;
using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    [Serializable]
    public abstract class CUnit
    {
        public CGameUser Owner { get; }
        public PlayerData UnitData { get; set; }
        public PlayerStateData StateData { set; get; }
        public HpMp HpMp { get; set; }
        public int NearRange => 5;
        public int UNIQUE_ID => UnitData.playerId;
        public PlayerState STATE => (PlayerState)StateData.state;
        public UnitType TYPE => (UnitType) UnitData.unitType;
        public int X => StateData.posX;
        public int Y => StateData.posY;
        
        public List<CUnit> prevNearUnits = new List<CUnit>();
        public CUnit targetUnit { get; set; }
        
        public CUnit() {}

        public CUnit(CGameUser user, PlayerDataPackage userPack)
        {
            Initialized();
            Owner = user;
            UnitData = userPack.data;
            StateData = userPack.state;
            HpMp = userPack.hpMp;
        }

        public CUnit(PlayerDataPackage userPack)
        {
            Initialized();
            UnitData = userPack.data;
            StateData = userPack.state;
            HpMp = userPack.hpMp;
        }

        protected void Initialized()
        {
            if (Program.gameServer._listUnit.Contains(this))
                return;
            
            Program.gameServer._listUnit.Add(this);
        }

        public bool IsPlayer() => UnitData.unitType == (byte) UnitType.PLAYER;
        
        public List<CUnit> GetNearRangeUnit()
        {
            var list = MapManager.I.GetAllUnitByNearRange(StateData.posX, StateData.posY, NearRange);

            if (list.Contains(this))
            {
                var index = list.FindIndex(p => p == this);
                list.RemoveAt(index);
            }
            return list; 
        }

        public virtual void DisconnectedWorld()
        {
            MapManager.I.RemoveUnitTile(this);    // 맵에서 유닛 삭제.
            Program.gameServer._listUnit.Remove(this);
            Program.gameServer.DisconnectedUnit(this); //
        }

        public abstract void ResponseRemoveNearUnit(List<CUnit> units);
        public abstract void RequestPlayerMove();
        public abstract void ResponseAddNearUnit(List<CUnit> units);
        public abstract void SetPosition(int x, int y, int dir);
        public abstract void Dead(CUnit attacker);
    }
}