using System;
using System.Collections.Generic;

namespace CSampleServer
{
    public class CMonster : CUnit
    {
        private const int resetSec = 3;    // 몬스터가 죽은 후 사라지는 시간(초)
        
        private long delayTime;
        private Action OnAfterCall;

        public CMonster(PlayerDataPackage userPack)  : base(userPack)
        {
            //prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            SetPosition(stateData.posX, stateData.posY, stateData.direction);

            Program.Tick += Tick;
        }

        void Tick()
        {
            OnAfterCall?.Invoke();
        }

        public override void SetPosition(int x, int y, int dir)
        {
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }

        // 플레이어가 범위내에 있을 시 알리기.
        public override void ResponseAddNearUnit(List<CUnit> units)
        {
            // if (base.IsPlayer() == false)
            //     return;
            //
            // CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            // user.playerData.PushData(response);
            // user.stateData.PushData(response);
            // user.HpMp.PushData(response);
            // owner?.send(response);
            //
            // Program.PrintLog($"[{playerData.name}]의 범위내에 [{user.playerData.name}]가 들어옴.");
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public override void ResponseRemoveNearUnit(List<CUnit> units)
        {
            // if (base.IsPlayer() == false)
            //     return;
            //
            // CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            // user.playerData.PushData(response);
            // owner?.send(response);
            //
            // Program.PrintLog($"[{playerData.name}]의 범위내에서[{user.playerData.name}]가 벚어남.");
        }

        public override void RequestPlayerMove()
        {
        }

        public override void RequestPlayerState(int receiveUserId)
        {
        }

        public override void Dead(CUnit attacker)
        {
            var resetTime = TimeManager.I.UtcTimeStampSeconds + resetSec;

            delayTime = resetTime;

            OnDelayCall(DesconnectedWorld, delayTime);
        }

        private void OnDelayCall(Action onCall, long delay)
        {
            long delayCallTime = delay;
            
            OnAfterCall = () =>
            {
                if (TimeManager.I.UtcTimeStampSeconds > delayCallTime)
                {
                    DesconnectedWorld();
                    OnAfterCall = null;
                }
            };
        }

        // 접속 종료.
        public override void DesconnectedWorld()
        {
            base.DesconnectedWorld();

            MonsterManager.I.RemoveMonster(this);
        }
    }
}