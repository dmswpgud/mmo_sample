using System;
using System.Collections.Generic;
using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CPlayer
    {
        CGameUser owner;
        public List<CGameUser> listNearbyUser = new List<CGameUser>();
        public PlayerData playerData = new PlayerData();
        public PlayerStateData stateData = new PlayerStateData();
        public HpMp HpMp = new HpMp();
        
        public CPlayer(CGameUser user)
        {
            owner = user;
        }

        public void SetPosition(int x, int y, int dir)
        {
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;
            
           var updateNearPlayers = GameUtils.GetNearbyUsers(owner, Program.gameServer.userList);
           var list1 = listNearbyUser.Where(i => !updateNearPlayers.Contains(i)).ToList(); // 삭제된 플레이어 리스트
           var list2 = updateNearPlayers.Where(i => !listNearbyUser.Contains(i)).ToList(); // 새로 추가된 플레이어 리스트

           // 범위내 케릭터 삭제 요청.
           foreach (var user in list1)
           {
               user.player.RemoveNearPlayer(owner);
               ResponseRemoveNearPlayer(user, owner);
               ResponseRemoveNearPlayer(owner, user);
           }

           // 범위내 케릭터 추가 요청.
           foreach (var user in list2)
           {
               user.player.AddNearPlayer(owner);
               ResponseAddNearPlayer(user, owner);
               ResponseAddNearPlayer(owner, user);
           }
           
           // 범위내 케릭터 갱신.
           listNearbyUser.Clear();
           listNearbyUser.AddRange(updateNearPlayers);
        }

        // 범위내 케릭터 추가.
        private void AddNearPlayer(CGameUser user)
        {
            listNearbyUser.Add(user);
        }

        // 범위내 케릭터 삭제.
        private void RemoveNearPlayer(CGameUser user)
        {
            listNearbyUser.Remove(user);
        }
        
        // 플레이어가 범위내에 있을 시 알리기.
        public void ResponseAddNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            user2.player.playerData.PushData(response);
            user2.player.stateData.PushData(response);
            user2.player.HpMp.PushData(response);
            user.send(response);
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public void ResponseRemoveNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            user2.player.playerData.PushData(response);
            user.send(response);
        }
        
        // 플레이어 이동.
        public void RequestPlayerMove(CGameUser user)
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            user.player.stateData.PushData(response);
            user.send(response);
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var userData in user.player.listNearbyUser)
            {
                user.player.stateData.PushData(response);
                userData.send(response);
            }
        }
        
        // 플레이어 상태 변경.
        public void RequestPlayerState(CGameUser user, int receiveUserId)
        {
            PlayerStateAttack(user, receiveUserId);
            
            // switch ((PlayerState)user.player.stateData.state)
            // {
            //     case PlayerState.ATTACK:
            //         while (stateData.state == (byte)PlayerState.ATTACK)
            //         {
            //             PlayerStateAttack(user, receiveUserId);
            //             //Console.Write(".");
            //             System.Threading.Thread.Sleep(2000);
            //         }
            //         break;
            // }
        }

        private void PlayerStateAttack(CGameUser attacker, int defenderUserId)
        {
            var defender = attacker.player.listNearbyUser.Find(p => p.player.playerData.playerId == defenderUserId);
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
            
            if (defender == null)
            {
                attacker.player.stateData.PushData(response);
                //defender?.player.stateData.PushData(response);
                //defender.player.HpMp.PushData(response);
                CGameServer.ResponsePacketToUsers(attacker.player.listNearbyUser, response);
            }
            else
            {
                var defenderHp = GameUtils.DamageCalculator(attacker.player, defender.player);
                defender.player.HpMp.Hp = defenderHp;
                defender.player.stateData.state = defenderHp <= 0 ? (byte)PlayerState.DEATH : (byte)PlayerState.DAMAGE;

                { // 공격 > 서버 > 공격
                    attacker.player.stateData.PushData(response);
                    defender.player.stateData.PushData(response);
                    defender.player.HpMp.PushData(response);
                    attacker.send(response);
                }

                { // 공격 > 서버 > 피격자
                    attacker.player.stateData.PushData(response);
                    defender.player.stateData.PushData(response);
                    defender.player.HpMp.PushData(response);
                    defender.send(response);
                }

                { // 공격 > 서버 > 브로드캐스트
                    var attackerDefenderNearUserList = GameUtils.GetTotlNearUserList(attacker.player.listNearbyUser, defender.player.listNearbyUser);
                    
                    foreach (var user in attackerDefenderNearUserList)
                    {
                        if (attacker.player.playerData.playerId == user.player.playerData.playerId)
                            continue;

                        if (defender.player.playerData.playerId == user.player.playerData.playerId)
                            continue;
                        
                        attacker.player.stateData.PushData(response);
                        defender.player.stateData.PushData(response);
                        defender.player.HpMp.PushData(response);
                        user.send(response);
                    }
                }
            }
        }

        // 접속 종료.
        public void DisconnectedPlayer()
        {
            // 접속 종료되었으니 범위내에 다른 플레이어들에게 나를 범위에서 삭제.
            foreach (var user in listNearbyUser)
            {
                user.player.RemoveNearPlayer(owner);
            }
        }
    }
}