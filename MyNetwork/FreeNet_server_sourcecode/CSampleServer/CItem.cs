using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public class CItem : CUnit
    {
        public ItemInfo _itemInfo { get; }
        
        public CItem(ItemInfo itemInfo, int x, int y)
        {
            _itemInfo = itemInfo;
            
            var pack = new PlayerDataPackage();
            var data = new PlayerData();
            var state = new PlayerStateData();
            var hpmp = new HpMp();

            data.playerId = itemInfo.uniqueId;
            data.name = itemInfo.itemName;
            data.unitType = (byte) UnitType.ITEM;
            state.playerId = itemInfo.uniqueId;
            state.unitType = (byte) UnitType.ITEM;
            state.posX = (short)x;
            state.posY = (short)y;
            pack.data = data;
            pack.state = state;
            pack.hpMp = hpmp;
            
            UnitData = data;
            StateData = state;
            HpMp = new HpMp();
            
            SetPosition(StateData.posX, StateData.posY, StateData.direction);
        }
        
        public override void ResponseRemoveNearUnit(List<CUnit> units)
        {
        }

        public override void RequestPlayerMove()
        {
        }

        public override void ResponseAddNearUnit(List<CUnit> units)
        {
        }

        public override void SetPosition(int x, int y, int dir)
        {
            StateData.posX = (short)x;
            StateData.posY = (short)y;
            StateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }

        public override void Dead(CUnit attacker)
        {
        }
        
        // 접속 종료.
        public override void DisconnectedWorld()
        {
            base.DisconnectedWorld();
        }
    }
}