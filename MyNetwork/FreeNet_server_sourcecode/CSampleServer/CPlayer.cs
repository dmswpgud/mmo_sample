using System;
using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CPlayer : CUnit
    {
        public CPlayer(CGameUser user) : base(user)
        {
        }

        public override void SetPosition(int x, int y, int dir)
        {
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            UpdateNearUnit();
        }

        public override void UpdateNearUnit()
        {
            var updateNearPlayers = GameUtils.GetNearbyUnit(playerData.playerId, stateData.posX, stateData.posY, playerData.nearRange, Program.gameServer.userList);
            var list1 = listNearbyUser.Where(i => !updateNearPlayers.Contains(i)).ToList(); // 삭제된 플레이어 리스트
            var list2 = updateNearPlayers.Where(i => !listNearbyUser.Contains(i)).ToList(); // 새로 추가된 플레이어 리스트

            // 범위내 케릭터 삭제 요청.
            foreach (var user in list1)
            {
                user.RemoveNearUnit(this);
                ResponseRemoveNearUnit(user, this);
                ResponseRemoveNearUnit(this, user);
            }

            // 범위내 케릭터 추가 요청.
            foreach (var user in list2)
            {
                user.AddNearUnit(this);
                ResponseAddNearUnit(user, this);
                ResponseAddNearUnit(this, user);
            }
           
            // 범위내 케릭터 갱신.
            listNearbyUser.Clear();
            listNearbyUser.AddRange(updateNearPlayers);
        }

        // 범위내 케릭터 추가.
        public override void AddNearUnit(CUnit user)
        {
            listNearbyUser.Add(user);
        }

        // 범위내 케릭터 삭제.
        public override void RemoveNearUnit(CUnit user)
        {
            listNearbyUser.Remove(user);
        }

        // 플레이어가 범위내에 있을 시 알리기.
        public override void ResponseAddNearUnit(CUnit user, CUnit user2)
        {
            if (!user.IsPlayer())
                return;
                
            CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            user2.playerData.PushData(response);
            user2.stateData.PushData(response);
            user2.HpMp.PushData(response);
            user.owner?.send(response);
            
            Program.PrintLog($"[{user.playerData.name}]의 범위내에 [{user2.playerData.name}]가 들어옴.");
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public override void ResponseRemoveNearUnit(CUnit user, CUnit user2)
        {
            if (!user.IsPlayer())
                return;
            
            CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            user2.playerData.PushData(response);
            user.owner?.send(response);
            
            Program.PrintLog($"[{user.playerData.name}]의 범위내에서[{user2.playerData.name}]가 벚어남.");
        }
        
        // 플레이어 이동.
        public override void RequestPlayerMove()
        {
            if (!IsPlayer())
                return;
            
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            stateData.PushData(response);
            owner?.send(response);
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var userData in listNearbyUser)
            {
                stateData.PushData(response);
                userData?.owner?.send(response);
            }
        }
        
        // 플레이어 상태 변경.
        public override void RequestPlayerState(int receiveUserId)
        {
            PlayerStateAttack(receiveUserId);
        }

        private void PlayerStateAttack(int defenderUserId)
        {
            var attacker = this;
            var defender = attacker.listNearbyUser.Find(p => p.playerData.playerId == defenderUserId);
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
            
            if (defender == null)
            {
                attacker.stateData.PushData(response);
                //defender?.player.stateData.PushData(response);
                //defender.player.HpMp.PushData(response);
                CGameServer.ResponsePacketToUsers(attacker.listNearbyUser, response);
            }
            else
            {
                var defenderHp = GameUtils.DamageCalculator(attacker, defender);
                defender.HpMp.Hp = defenderHp;
                defender.stateData.state = defenderHp <= 0 ? (byte)PlayerState.DEATH : (byte)PlayerState.DAMAGE;

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
                }

                { // 공격 > 서버 > 브로드캐스트
                    var attackerDefenderNearUserList = GameUtils.GetTotlNearUserList(attacker.listNearbyUser, defender.listNearbyUser);
                    
                    foreach (var user in attackerDefenderNearUserList)
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

        // 접속 종료.
        public override void DisconnectedPlayer()
        {
            // 접속 종료되었으니 범위내에 다른 플레이어들에게 나를 범위에서 삭제.
            foreach (var user in listNearbyUser)
            {
                user.RemoveNearUnit(this);
            }
        }
    }
}