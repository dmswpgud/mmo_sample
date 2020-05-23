using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class MonsterAI
    {
        private CMonster owner;
        private int unitId => owner.UnitData.UniqueId;
        private UnitStateData state => owner.StateData;
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
            var targets = allUnits.FindAll(p => p.UnitData.unitType == (byte) UnitType.PLAYER);
            
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
            listPath = MapManager.I.FindPath(state.posX, state.posY, destX, destY);
            
            if (listPath.Count <= 0)
            {
                owner.SetState(PlayerState.IDLE);
                return;
            }

            var dir = MapManager.I.SetDirectionByPosition(state.posX, state.posY, (short) listPath[0].X, (short) listPath[0].Y);
            
            owner.SetPosition((short)listPath[0].X, (short)listPath[0].Y, dir);
            
            state.state = (byte) PlayerState.WARK;
            
            owner.RequestPlayerMove();

            listPath.RemoveAt(0);
            
            BehaviorIntervalTime = 1;
        }

        private void DashToTarget()
        {
            int destX = owner.targetUnit.StateData.posX;
            int destY = owner.targetUnit.StateData.posY;
            
            listPath = MapManager.I.FindPath(state.posX, state.posY, destX, destY);

            if (listPath.Count == 1)
            {
                state.state = (byte) PlayerState.ATTACK;
                BehaviorIntervalTime = 0;
                return;
            }
            
            if (listPath.Count == 0)
            {
                //owner.targetUnit = null;
                owner.SetState(PlayerState.DASH_TO_TARGET);
                return;
            }
            
            // 이동할 타일에 다른 유닛이 있으면 딜레이 후 다시 데쉬.. (유닛이 안겹침)
            var units = MapManager.I.GetUnits(listPath[0].X, listPath[0].Y);
            if (units.Exists(p => p.UnitData.unitType == (byte) UnitType.MONSTER ||
                                  p.StateData.state != (byte) PlayerState.DEATH))
            {
                owner.SetState(PlayerState.DASH_TO_TARGET);
                BehaviorIntervalTime = 1;
                return;
            }

            var dir = MapManager.I.SetDirectionByPosition(state.posX, state.posY, (short) listPath[0].X, (short) listPath[0].Y);
            
            owner.SetPosition((short)listPath[0].X, (short)listPath[0].Y, dir);
            
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

            var dir = MapManager.I.SetDirectionByPosition(state.posX, state.posY, owner.targetUnit.StateData.posX, owner.targetUnit.StateData.posY);
            owner.StateData.direction = (byte)dir;
            MonsterStateAttack(owner.targetUnit.UnitData.UniqueId);

            BehaviorIntervalTime = 2;
        }
        
        private void MonsterStateAttack(int defenderUserId)
        {
            var attacker = owner;
            var defender = owner.GetNearRangeUnit().Find(p => p.UnitData.UniqueId == defenderUserId);
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
            
            if (defender == null)
            {
                owner.StateData.PushData(response);
                //defender?.player.stateData.PushData(response);
                //defender.player.HpMp.PushData(response);
                CGameServer.ResponseToUsers(attacker.GetNearRangeUnit(), response);
            }
            else
            {
                var defenderHp = GameUtils.DamageCalculator(attacker, defender);
                var defenderState = defenderHp <= 0 ? PlayerState.DEATH : PlayerState.DAMAGE;
                defender.targetUnit = attacker;
                defender.StateData.state = (byte)defenderState;
                defender.HpMp.Hp = defenderHp;

                Program.PrintLog($"[공격자 {attacker.UnitData.name}] hm{attacker.HpMp.Hp}/{attacker.HpMp.Mp}  [피격자 {defender.UnitData.name}] hm{defender.HpMp.Hp}/{defender.HpMp.Mp}");
                
                { // 공격 > 서버 > 공격
                    attacker.StateData.PushData(response);
                    defender.StateData.PushData(response);
                    defender.HpMp.PushData(response);
                    attacker.Owner?.send(response);
                }

                { // 공격 > 서버 > 피격자
                    attacker.StateData.PushData(response);
                    defender.StateData.PushData(response);
                    defender.HpMp.PushData(response);
                    defender.Owner?.send(response);

                    if (defenderState == PlayerState.DEATH)
                    {
                        owner.SetState(PlayerState.IDLE);
                        owner.targetUnit = null;
                        defender.Dead(defender);
                    }
                }

                { // 공격 > 서버 > 브로드캐스트
                    var broadcastUnits = MapManager.I.GetUnitFromUnits(attacker, defender);
                    foreach (var user in broadcastUnits)
                    {
                        if (attacker.UnitData.UniqueId == user.UnitData.UniqueId)
                            continue;

                        if (defender.UnitData.UniqueId == user.UnitData.UniqueId)
                            continue;
                        
                        attacker.StateData.PushData(response);
                        defender.StateData.PushData(response);
                        defender.HpMp.PushData(response);
                        user.Owner?.send(response);
                    }
                }
            }
        }
    }
}