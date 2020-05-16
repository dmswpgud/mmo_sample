using System.Collections.Generic;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CPlayer : CUnit
    {
        public CPlayer(CGameUser owner, PlayerDataPackage user) : base(owner, user) { }

        public override void SetPosition(int x, int y, int dir)
        {
            prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }

        // 플레이어가 범위내에 있을 시 알리기.
        public override void ResponseAddNearUnit(List<CUnit> units)
        {
            if (units.Count == 0)
                return;
            
            CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            var count = units.Count;
            response.push(count);
            
            foreach (var unit in units)
            {
                unit.playerData.PushData(response);
                unit.stateData.PushData(response);
                unit.HpMp.PushData(response);
                
                Program.PrintLog($"[{playerData.name}]의 범위내에 [{unit.playerData.name}]가 들어옴.");
            }

            owner?.send(response);
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public override void ResponseRemoveNearUnit(List<CUnit> units)
        {
            if (units.Count == 0)
                return;
            
            CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            var count = units.Count;
            response.push(count);
            foreach (var unit in units)
            {
                unit.playerData.PushData(response);
                
                Program.PrintLog($"[{playerData.name}]의 범위내에서[{unit.playerData.name}]가 벚어남.");
            }

            owner?.send(response);
        }
        
        // 플레이어 이동.
        public override void RequestPlayerMove()
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            stateData.PushData(response);
            owner?.send(response);

            var nearUnits = GetNearRangeUnit();
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var unit in nearUnits)
            {
                response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
                stateData.PushData(response);
                unit?.owner?.send(response);
            }
        }
        
        // 플레이어 상태 변경.
        public override void RequestPlayerState(int receiveUserId)
        {
            PlayerStateAttack(receiveUserId);
        }

        public override void Dead(CUnit attacker)
        {
            
        }

        private void PlayerStateAttack(int defenderUserId)
        {
            var attacker = this;
            var defender = GetNearRangeUnit().Find(p => p.playerData.playerId == defenderUserId);
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
            
            if (defender == null)
            {
                attacker.stateData.PushData(response);
                //defender?.player.stateData.PushData(response);
                //defender.player.HpMp.PushData(response);
                CGameServer.ResponseToUsers(GetNearRangeUnit(), response);
            }
            else
            {
                var defenderHp = GameUtils.DamageCalculator(attacker, defender);
                var defenderState = defenderHp <= 0 ? PlayerState.DEATH : PlayerState.DAMAGE;
                defender.stateData.state = (byte)defenderState;
                defender.HpMp.Hp = defenderHp;

                Program.PrintLog($"[공격자 {playerData.name}] hm{HpMp.Hp}/{HpMp.Mp}  [피격자 {defender.playerData.name}] hm{defender.HpMp.Hp}/{defender.HpMp.Mp}");
                
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

        public override void DesconnectedWorld()
        {
            base.DesconnectedWorld();
        }
    }
}