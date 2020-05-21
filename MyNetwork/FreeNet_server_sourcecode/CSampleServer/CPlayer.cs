using System.Collections.Generic;
using CSampleServer.DefaultNamespace;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CPlayer : CUnit
    {
        public CPlayer(CGameUser owner, PlayerDataPackage user) : base(owner, user) { }
        
        private PlayerItemService _itemService = new PlayerItemService();

        public override void SetPosition(int x, int y, int dir)
        {
            prevNearUnits = MapManager.I.GetAllOtherUnit(this);
            
            StateData.posX = (short)x;
            StateData.posY = (short)y;
            StateData.direction = (byte)dir;

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
                unit.UnitData.PushData(response);
                unit.StateData.PushData(response);
                unit.HpMp.PushData(response);
                
                Program.PrintLog($"[{UnitData.name}]의 범위내에 [{unit.UnitData.name}]가 들어옴.");
            }

            Owner?.send(response);
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
                unit.UnitData.PushData(response);
                
                Program.PrintLog($"[{UnitData.name}]의 범위내에서[{unit.UnitData.name}]가 벚어남.");
            }

            Owner?.send(response);
        }
        
        public override void Dead(CUnit attacker)
        {
            
        }
        
        // 플레이어 이동.
        public override void RequestPlayerMove()
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            StateData.PushData(response);
            Owner?.send(response);

            var nearUnits = GetNearRangeUnit();
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var unit in nearUnits)
            {
                response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
                StateData.PushData(response);
                unit?.Owner?.send(response);
            }
        }

        // 플레이어 공격.
        public void PlayerStateAttack(int defenderUserId)
        {
            var attacker = this;
            var defender = GetNearRangeUnit().Find(p => p.UnitData.playerId == defenderUserId);
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
            
            if (defender == null)
            {
                attacker.StateData.PushData(response);
                //defender?.player.stateData.PushData(response);
                //defender.player.HpMp.PushData(response);
                CGameServer.ResponseToUsers(GetNearRangeUnit(), response);
            }
            else
            {
                var defenderHp = GameUtils.DamageCalculator(attacker, defender);
                var defenderState = defenderHp <= 0 ? PlayerState.DEATH : PlayerState.DAMAGE;
                defender.targetUnit = attacker;
                defender.StateData.state = (byte)defenderState;
                defender.HpMp.Hp = defenderHp;

                Program.PrintLog($"[공격자 {UnitData.name}] hm{HpMp.Hp}/{HpMp.Mp}  [피격자 {defender.UnitData.name}] hm{defender.HpMp.Hp}/{defender.HpMp.Mp}");
                
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
                        defender.Dead(defender);
                    }
                }

                { // 공격 > 서버 > 브로드캐스트
                    var broadcastUnits = MapManager.I.GetUnitFromUnits(attacker, defender);
                    foreach (var user in broadcastUnits)
                    {
                        if (attacker.UNIQUE_ID == user.UNIQUE_ID)
                            continue;

                        if (defender.UNIQUE_ID == user.UNIQUE_ID)
                            continue;
                        
                        attacker.StateData.PushData(response);
                        defender.StateData.PushData(response);
                        defender.HpMp.PushData(response);
                        user.Owner?.send(response);
                    }
                }
            }
        }

        // 아이템 줍기.
        public void RequestPickedItem(int uniqueItemId)
        {
            var itemInstance = ItemManager.I.GetItemFromPosition(uniqueItemId, X, Y);
            StateData.direction = (byte)MapManager.I.SetDirectionByPosition(X, Y, itemInstance.X, itemInstance.Y);

            if (itemInstance != null)
            {
                var addedItem = _itemService.AddItem(itemInstance._itemInfo);
                {
                    CPacket response = CPacket.create((short)PROTOCOL.PICKING_ITEM_RES);
                    StateData.state = (byte) PlayerState.PICKED_ITEM;
                    StateData.PushData(response); 
                    addedItem.PushData(response);
                    Owner.send(response);
                    CGameServer.ResponseToUsers(GetNearRangeUnit(), response);
                }
                
                itemInstance.DisconnectedWorld();
            }
        }

        public override void DisconnectedWorld()
        {
            base.DisconnectedWorld();
        }
    }
}