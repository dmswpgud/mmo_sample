using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public class CItem : CUnit
    {
        public ItemInfo _itemInfo { get; private set; }

        public CItem(PlayerData data, PlayerStateData state, HpMp hpMp) : base(data, state, hpMp)
        {
            SetPosition(X, Y, DIRECTION);
        }

        public void SetItemInfo(ItemInfo info)
        {
            _itemInfo = info;
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

        public override void SetPosition(int x, int y, UnitDirection dir)
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