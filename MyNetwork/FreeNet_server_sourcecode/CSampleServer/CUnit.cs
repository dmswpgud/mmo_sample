using System;
using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    [Serializable]
    public abstract class CUnit
    {
        public CGameUser Owner { get; set; }
        public UnitData UnitData { get; set; }
        public UnitStateData StateData { get; set; }
        public HpMp HpMp { get; set; }
        public int NearRange => 5;
        public int UNIQUE_ID => UnitData.UniqueId;
        public PlayerState STATE => (PlayerState)StateData.state;
        public UnitDirection DIRECTION => (UnitDirection) StateData.direction;
        public UnitType TYPE => (UnitType) UnitData.unitType;
        public int X => StateData.posX;
        public int Y => StateData.posY;
        
        public List<CUnit> prevNearUnits = new List<CUnit>();
        public CUnit targetUnit { get; set; }
        
        public CUnit() {}

        public CUnit(UnitData data, UnitStateData state, HpMp hpMp)
        {
            Initialized();
            UnitData = data;
            StateData = state;
            HpMp = hpMp;
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
        public abstract void SetPosition(int x, int y, UnitDirection dir);
        public abstract void RecoveryHp(int recovery);
        public abstract void Dead(CUnit attacker);
    }
}