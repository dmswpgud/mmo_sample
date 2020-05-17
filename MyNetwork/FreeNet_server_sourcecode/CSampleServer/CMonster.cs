using System;
using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CMonster : CUnit
    {
        private MonsterAiData MonsterAiData;
        private const int resetSec = 3;    // 몬스터가 죽은 후 사라지는 시간(초)

        private CPlayer target;
        private long delayTime;
        private Action OnAfterCall;

        public CMonster(PlayerDataPackage userPack)  : base(userPack)
        {
            //prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            SetPosition(stateData.posX, stateData.posY, stateData.direction);

            Program.Tick += Tick;
        }

        public void SetAiInfo(MonsterAiData aiInfo)
        {
            MonsterAiData = aiInfo;
        }

        private void SetState(PlayerState state)
        {
            base.stateData.state = (byte) state;
        }

        void Tick()
        {
            OnAfterCall?.Invoke();
            AiTick();
        }

        public override void SetPosition(int x, int y, int dir)
        {
            prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }
        
        private void AiTick()
        {
            if (MonsterAiData == null)
                return;

            switch (STATE)
            {
                case PlayerState.IDLE:
                    SearchTarget();
                    break;
                case PlayerState.WARK:
                    Wark();
                    break;
                case PlayerState.ATTACK:
                    break;
                case PlayerState.DAMAGE:
                    break;
                case PlayerState.CHANGED_DIRECTION:
                    break;
            }
        }

        private void SearchTarget()
        {
            // 범위내에 유닛들을 취득.
            var allUnits = MapManager.I.GetAllUnitByNearRange(stateData.posX, stateData.posY, MonsterAiData.searchTargetRange);

            // 취득한 유닛들중 플레이어를 취득.
            var targets = allUnits.FindAll(p => p.playerData.unitType == (byte) UnitType.PLAYER);
            
            // 플레이어를 타겟으로 지정.
            //target = (CPlayer)targets[0];

            if (target == null && targets.Count <= 0)
            {
                SetState(PlayerState.WARK);
            }
        }

        private void Wark()
        {
            int destX = stateData.posX;
            int destY = stateData.posY;
            
            MapManager.I.GetRandomPosition(stateData.posX, stateData.posY, MonsterAiData.searchTargetRange, out destX, out destY);

            var path = MapManager.I.FindPath(stateData.posX, stateData.posY, destX, destY);

            if (path.Count <= 0)
                return;
            
            var dir = MapManager.I.SetDirectionByPosition(stateData.posX, stateData.posY, destX, destY);
            
            SetPosition((short)path[0].X, (short)path[0].Y, (int)dir);
            
            stateData.state = (byte) PlayerState.WARK;
            
            RequestPlayerMove();
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
                stateData.PushData(response);
                unit?.owner?.send(response);
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
                    onCall();
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