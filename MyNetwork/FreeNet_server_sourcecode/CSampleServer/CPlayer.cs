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
        public Int32 UserId;
        public Int32 MoveSpeed;
        public Int32 NearRange;
        public Int32 CurrentPosX {get; set;}
        public Int32 CurrentPosY {get; set;}
        public Int32 unitDirection {get; set;}
        public Int32 playerState;
        public List<CGameUser> listNearbyUser = new List<CGameUser>();
        
        public CPlayer(CGameUser user, int userId)
        {
            owner = user;
            UserId = userId;
        }

        public void SetPosition(int x, int y, int dir)
        {
            CurrentPosX = x;
            CurrentPosY = y;
            unitDirection = dir;
            
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
            CGameServer.PushPlayerData(user2, response);
            user.send(response);
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public void ResponseRemoveNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            CGameServer.PushPlayerData(user2, response);
            user.send(response);
        }
        
        // 플레이어 이동.
        public void RequestPlayerMove(CGameUser user)
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            CGameServer.PushPlayerData(user, response);
            user.send(response);
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var userData in user.player.listNearbyUser)
            {
                CGameServer.PushPlayerData(user, response);
                userData.send(response);
            }
        }
        
        // 플레이어 상태 변경.
        public void RequestPlayerState(CGameUser user, int receiveUserId)
        {
            switch ((PlayerState)user.player.playerState)
            {
                case PlayerState.ATTACK:
                    PlayerStateAttack(user, receiveUserId);
                    break;
            }
        }
        
        private void PlayerStateAttack(CGameUser attacker, int defenderUserId)
        {
            var defender = attacker.player.listNearbyUser.Find(p => p.player.UserId == defenderUserId);
            
            // 패킷 구성.
            // 상태를 요청한 유저 아이디.
            // 요청 유저의 상태 값.
            // 요청 유저의 방향.
            // 요청 유저의 타겟 (타겟이 없으면 0)
            // 요청 유저의 결과 값.
            
            // 타겟이 없으면 플레이어 상태만 전 플레이어에게 보낸다.
            if (defender == null)
            {
                CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
                response.push(attacker.player.UserId);
                response.push(attacker.player.playerState);
                response.push(attacker.player.unitDirection);
                response.push(0); // 타겟이 없기에 0 보냄.
                response.push(0); // 타겟이 없기에 0 보냄. // 공격 받는자의 HP보냄 // 사실 보낼 필욘 없음;
                
                CGameServer.ResponsePacketToUsers(attacker.player.listNearbyUser, response);
                
                attacker.send(response); // TODO: <= 로그 확인용.. 나한테 보낼 필요는 없기에 나중에 삭제
            }
            // 타겟이 있으면 서버에서 HP계산 후 어태커와 디펜더에게 결과 전송,
            // 다른 플레이어들에겐 상태값과 어태커, 디펜더 아이디를 보낸다.
            else
            {
                // 공격자.
                {
                    CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
                    response.push(attacker.player.UserId);
                    response.push((int)PlayerState.ATTACK);
                    response.push(attacker.player.unitDirection);
                    response.push(defender.player.UserId); // 공격받는 자의 아이디도 보냄.
                    response.push(GameUtils.DamageCalculator(attacker.player, defender.player)); // 공격 받는자의 HP보냄
                    attacker.send(response);
                }
                // 피격자.
                {
                    CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
                    response.push(attacker.player.UserId);
                    response.push((int)PlayerState.DAMAGE);
                    response.push(attacker.player.unitDirection);
                    response.push(defender.player.UserId); // 공격한 자의 아이디도 보냄.
                    response.push(GameUtils.DamageCalculator(attacker.player, defender.player)); // 공격 받는자의 HP보냄
                    defender.send(response);
                }
                // 방관자.
                {
                    List<CGameUser> attackerDefenderNearUserList = new List<CGameUser>();
                    // 어태커에 없는 디펜더의 범위 유저 리스트.
                    var defenderList = defender.player.listNearbyUser.Where(i => !attacker.player.listNearbyUser.Contains(i)).ToList();
                    attackerDefenderNearUserList.AddRange(attacker.player.listNearbyUser);
                    attackerDefenderNearUserList.AddRange(defenderList);
                    
                    CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
                    foreach (var user in attackerDefenderNearUserList)
                    {
                        if (attacker.player.UserId == user.player.UserId)
                            continue;

                        if (defender.player.UserId == user.player.UserId)
                            continue;
                        
                        response.push(attacker.player.UserId);
                        response.push(attacker.player.playerState);
                        response.push(attacker.player.unitDirection);
                        response.push(defender.player.UserId); // 타겟이 없기에 0 보냄.
                        response.push(GameUtils.DamageCalculator(attacker.player, defender.player)); // 공격 받는자의 HP보냄
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