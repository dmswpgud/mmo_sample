using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public class CItem : CUnit
    {
        private ItemInfo _itemInfo;
        
        public CItem(ItemInfo itemInfo, int x, int y)
        {
            _itemInfo = itemInfo;
            
            var pack = new PlayerDataPackage();
            var data = new PlayerData();
            var state = new PlayerStateData();
            var hpmp = new HpMp();

            data.playerId = itemInfo.dataId;
            data.name = itemInfo.itemName;
            data.unitType = (byte) UnitType.MAP_OBJECT;
            state.playerId = itemInfo.dataId;
            state.unitType = (byte) UnitType.MAP_OBJECT;
            state.posX = (short)x;
            state.posY = (short)y;
            pack.data = data;
            pack.state = state;
            pack.hpMp = hpmp;
            
            playerData = data;
            stateData = state;
            HpMp = new HpMp();
            
            SetPosition(stateData.posX, stateData.posY, stateData.direction);
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
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }

        public override void RequestPlayerState(int receiveUserId)
        {
        }

        public override void Dead(CUnit attacker)
        {
        }
    }
}