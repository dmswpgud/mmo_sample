using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class MonsterAI
    {
        private CMonster owner;
        private int unitId => owner.playerData.playerId;
        private PlayerStateData state => owner.stateData;
        private MonsterAiData MonsterAiData;
        private List<GridPoint> listPath = new List<GridPoint>();
        
        private int BehaviorIntervalTime;
        private long BehaviorStartTime;

        public MonsterAI(CMonster monster, MonsterAiData aiData)
        {
            owner = monster;
            MonsterAiData = aiData;
        }

        public void Tick()
        {
            BehaviorMachine();
        }
        
        private void BehaviorMachine()
        {
            if (MonsterAiData == null)
                return;
            
            if (!CheckMachineInterval())
                return;

            switch (owner.STATE)
            {
                case PlayerState.IDLE:
                    SearchTarget();
                    break;
                case PlayerState.WARK:
                    Patrol();
                    break;
                case PlayerState.DAMAGE:
                    DashToTarget();
                    break;
                case PlayerState.DASH_TO_TARGET:
                    DashToTarget();
                    break;
                case PlayerState.ATTACK:
                    AttackToTarget();
                    break;
                case PlayerState.CHANGED_DIRECTION:
                    break;
            }
        }

        private bool CheckMachineInterval()
        {
            if (TimeManager.I.UtcTimeStampSeconds < BehaviorStartTime)
                return false;
            
            BehaviorStartTime = TimeManager.I.UtcTimeStampSeconds + BehaviorIntervalTime;
            
            return true;
        }
        
        private void SearchTarget()
        {
            // 범위내에 유닛들을 취득.
            var allUnits = MapManager.I.GetAllUnitByNearRange(state.posX, state.posY, MonsterAiData.searchTargetRange);

            // 취득한 유닛들중 플레이어를 취득.
            var targets = allUnits.FindAll(p => p.playerData.unitType == (byte) UnitType.PLAYER);
            
            // 플레이어를 타겟으로 지정.
            //target = (CPlayer)targets[0];

            if (owner.targetUnit == null && targets.Count <= 0)
            {
                owner.SetState(PlayerState.WARK);
            }
            owner.SetState(PlayerState.WARK);
            BehaviorIntervalTime = 1;
        }
        
        private void Patrol()
        {
            int destX = state.posX;
            int destY = state.posY;
            
            MapManager.I.GetRandomPosition(state.posX, state.posY, MonsterAiData.searchTargetRange, out destX, out destY);

            if (listPath.Count <= 0)
            {
                listPath = MapManager.I.FindPath(state.posX, state.posY, destX, destY);
                owner.SetState(PlayerState.IDLE);
                return;
            }

            var dir = MapManager.I.SetDirectionByPosition(state.posX, state.posY, (short) listPath[0].X, (short) listPath[0].Y);
            
            owner.SetPosition((short)listPath[0].X, (short)listPath[0].Y, (int)dir);
            
            state.state = (byte) PlayerState.WARK;
            
            owner.RequestPlayerMove();

            listPath.RemoveAt(0);
            
            BehaviorIntervalTime = 1;
        }

        private void DashToTarget()
        {
            int destX = owner.targetUnit.stateData.posX;
            int destY = owner.targetUnit.stateData.posY;
            
            listPath = MapManager.I.FindPath(state.posX, state.posY, destX, destY);

            if (listPath.Count == 1)
            {
                state.state = (byte) PlayerState.ATTACK;
                BehaviorIntervalTime = 0;
                return;
            }
            
            if (listPath.Count == 0)
            {
                owner.SetState(PlayerState.IDLE);
                return;
            }

            var dir = MapManager.I.SetDirectionByPosition(state.posX, state.posY, (short) listPath[0].X, (short) listPath[0].Y);
            
            owner.SetPosition((short)listPath[0].X, (short)listPath[0].Y, (int)dir);
            
            state.state = (byte) PlayerState.DASH_TO_TARGET;
            
            owner.RequestPlayerMove();
            
            BehaviorIntervalTime = 1;
        }

        private void AttackToTarget()
        {
            if (owner.targetUnit == null)
            {
                owner.SetState(PlayerState.IDLE);
                return;
            }

            var target = MapManager.I.HasRangeInUnit(owner, owner.targetUnit, 1);
            if (target == false)
            {
                owner.SetState(PlayerState.DASH_TO_TARGET);
                return;
            }
            
            var dir = MapManager.I.SetDirectionByPosition(state.posX, state.posY, owner.targetUnit.stateData.posX, owner.targetUnit.stateData.posY);
            owner.stateData.direction = (byte)dir;
            MonsterStateAttack(owner.targetUnit.playerData.playerId);

            BehaviorIntervalTime = 2;
        }
        
        private void MonsterStateAttack(int defenderUserId)
        {
            var attacker = owner;
            var defender = owner.GetNearRangeUnit().Find(p => p.playerData.playerId == defenderUserId);
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
            
            if (defender == null)
            {
                owner.stateData.PushData(response);
                //defender?.player.stateData.PushData(response);
                //defender.player.HpMp.PushData(response);
                CGameServer.ResponseToUsers(attacker.GetNearRangeUnit(), response);
            }
            else
            {
                var defenderHp = GameUtils.DamageCalculator(attacker, defender);
                var defenderState = defenderHp <= 0 ? PlayerState.DEATH : PlayerState.DAMAGE;
                defender.targetUnit = attacker;
                defender.stateData.state = (byte)defenderState;
                defender.HpMp.Hp = defenderHp;

                Program.PrintLog($"[공격자 {attacker.playerData.name}] hm{attacker.HpMp.Hp}/{attacker.HpMp.Mp}  [피격자 {defender.playerData.name}] hm{defender.HpMp.Hp}/{defender.HpMp.Mp}");
                
                { // 공격 > 서버 > 공격
                    attacker.stateData.PushData(response);
                    defender.stateData.PushData(response);
                    defender.HpMp.PushData(response);
                    attacker.owner?.send(response);
                }

                { // 공격 > 서버 > 피격자
                    attacker.stateData.PushData(response);
                    defender.stateData.PushData(response);
                    defender.HpMp.PushData(response);
                    defender.owner?.send(response);

                    if (defenderState == PlayerState.DEATH)
                    {
                        owner.SetState(PlayerState.IDLE);
                        defender.Dead(defender);
                    }
                }

                { // 공격 > 서버 > 브로드캐스트
                    var broadcastUnits = MapManager.I.GetUnitFromUnits(attacker, defender);
                    foreach (var user in broadcastUnits)
                    {
                        if (attacker.playerData.playerId == user.playerData.playerId)
                            continue;

                        if (defender.playerData.playerId == user.playerData.playerId)
                            continue;
                        
                        attacker.stateData.PushData(response);
                        defender.stateData.PushData(response);
                        defender.HpMp.PushData(response);
                        user.owner?.send(response);
                    }
                }
            }
        }
    }
}