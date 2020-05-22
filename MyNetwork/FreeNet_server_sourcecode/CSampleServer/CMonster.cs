using System;
using System.Collections.Generic;
using CSampleServer.DefaultNamespace;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CMonster : CUnit
    {
        private MonsterAI MonsterAi;
        private MonsterAI ai;
        private const int resetSec = 3;    // 몬스터가 죽은 후 사라지는 시간(초)

        private CPlayer target;
        private long delayTime;
        private Action OnAfterCall;

        public CMonster(UnitData data, UnitStateData state, HpMp hpMp) : base(data, state, hpMp)
        {
            //prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            SetPosition(X, Y, DIRECTION);

            Program.Tick += Tick;
        }

        public void SetAiInfo(MonsterAiData aiInfo)
        {
            MonsterAi = new MonsterAI(this, aiInfo);
        }

        public void SetState(PlayerState state)
        {
            base.StateData.state = (byte) state;
        }

        void Tick()
        {
            OnAfterCall?.Invoke();

            MonsterAi?.Tick();
        }

        public override void SetPosition(int x, int y, UnitDirection dir)
        {
            prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            
            StateData.posX = (short)x;
            StateData.posY = (short)y;
            StateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }
        
        public override void RequestPlayerMove()
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);

            var nearUnits = GetNearRangeUnit();
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var unit in nearUnits)
            {
                response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
                StateData.PushData(response);
                unit?.Owner?.send(response);
            }
        }

        // 플레이어가 범위내에 있을 시 알리기.
        public override void ResponseAddNearUnit(List<CUnit> units)
        {
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public override void ResponseRemoveNearUnit(List<CUnit> units)
        {
        }

        public override void Dead(CUnit attacker)
        {
            var resetTime = TimeManager.I.UtcTimeStampSeconds + resetSec;

            delayTime = resetTime;

            OnDelayCall(DisconnectedWorld, delayTime);

            ItemManager.I.CreateItem(3, X, Y);
        }

        private void OnDelayCall(Action onCall, long delay)
        {
            long delayCallTime = delay;
            
            OnAfterCall = () =>
            {
                if (TimeManager.I.UtcTimeStampSeconds > delayCallTime)
                {
                    onCall();
                    OnAfterCall = null;
                }
            };
        }

        // 접속 종료.
        public override void DisconnectedWorld()
        {
            base.DisconnectedWorld();

            MonsterManager.I.RemoveMonster(this);
        }
    }
}