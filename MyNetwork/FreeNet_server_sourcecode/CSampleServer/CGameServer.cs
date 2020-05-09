using System.Collections.Generic;
using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CGameServer
    {
        public List<CGameUser> userList = new List<CGameUser>();

        public bool ExistsUser(int userId)
        {
            return userList.Exists(user => user.player.UserId == userId);
        }

        public CGameUser GetUserFromUserId(int userId)
        {
            return userList.Find(p => p.player.targetUserId == userId);
        }
        
        // 서버 접속.
        public void UserEntedServer(CGameUser user)
        {
            // 서버에 유저 추가.
            userList.Add(user);

            CPacket response = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_RES);
            response.push(user.player.UserId);
            user.send(response);
        }
        
        // 서버 접속 종료.
        public void DisconnectedUser(CGameUser user)
        {
            userList.Remove(user);
            
            foreach (var otherUser in userList)
            {
                CPacket response = CPacket.create((short)PROTOCOL.DISCONECTED_PLAYER_RES);
                response.push(user.player.UserId);
                otherUser.send(response);
            }
        }

        // 다른 유저에게 채팅 보내기
        public void SendChatMessage(CGameUser owner, string text)
        {
            foreach (var user in userList)
            {
                CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
                response.push(owner.player.UserId);
                response.push(text);
                user.send(response);
            }
        }

        // 내 케릭 가져다주기.
        public void ResponseGetMyPlayer(CGameUser user)
        {
            CPacket response = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_RES);
            PushPlayerData(user, response);
            user.send(response);
        }
        
        // 플레이어가 범위내에 있을 시 알리기.
        public void ResponseAddNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            PushPlayerData(user2, response);
            user.send(response);
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public void ResponseRemoveNearPlayer(CGameUser user, CGameUser user2)
        {
            CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            PushPlayerData(user2, response);
            user.send(response);
        }

        // 플레이어 이동.
        public void RequestPlayerMove(CGameUser user)
        {
            // 이동한 플레이어에게 서버에 이동한거 등록했다고 답장
            CPacket response = CPacket.create((short)PROTOCOL.PLAYER_MOVE_RES);
            PushPlayerData(user, response);
            user.send(response);
            
            // 이동한 녀석의 좌표를 다른 플레이어들에게 보내기
            foreach (var userData in user.player.listNearbyUser)
            {
                PushPlayerData(user, response);
                userData.send(response);
            }
        }
        
        // 플레이어 공격 요청
        public void RequestPlayerState(CGameUser user)
        {
            switch ((PlayerState)user.player.playerState)
            {
                case PlayerState.ATTACK:
                    PlayerStateAttack(user);
                    break;
                case PlayerState.DAMAGE:
                    break;
            }
        }

        private void PlayerStateAttack(CGameUser attacker)
        {
            var defender = attacker.player.GetFrontPositionTarget();
            
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
                
                ResponsePacketToUsers(attacker.player.listNearbyUser, response);
                
                attacker.send(response); // TODO: <= 로그 확인용.. 나한테 보낼 필요는 없기에 나중에 삭제
            }
            // 타겟이 있으면 서버에서 HP계산 후 어태커와 디펜더에게 결과 전송,
            // 다른 플레이어들에겐 상태값과 어태커, 디펜더 아이디를 보낸다.
            else
            {
                // 공격자
                {
                    CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
                    response.push(attacker.player.UserId);
                    response.push((int)PlayerState.ATTACK);
                    response.push(attacker.player.unitDirection);
                    response.push(defender.player.UserId); // 공격받는 자의 아이디도 보냄.
                    response.push(GameUtils.DamageCalculator(attacker.player, defender.player)); // 공격 받는자의 HP보냄
                    attacker.send(response);
                }
                // 공격받는 자.
                {
                    CPacket response = CPacket.create((short)PROTOCOL.PLAYER_STATE_RES);
                    response.push(defender.player.UserId);
                    response.push((int)PlayerState.DAMAGE);
                    response.push(attacker.player.unitDirection);
                    response.push(attacker.player.UserId); // 공격한 자의 아이디도 보냄.
                    response.push(GameUtils.DamageCalculator(attacker.player, defender.player)); // 공격 받는자의 HP보냄
                    defender.send(response);
                }
                
                {
                    List<CGameUser> attackerDefenderNearUserList = new List<CGameUser>();
                    // 어태커에 없는 디펜더의 범위 유저 리스트.
                    var defenderList = attacker.player.listNearbyUser.Where(i => !defender.player.listNearbyUser.Contains(i)).ToList();
                    attackerDefenderNearUserList.AddRange(attacker.player.listNearbyUser);
                    attackerDefenderNearUserList.AddRange(defender.player.listNearbyUser);
                    
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

        // 여러명에게 보내기.
        public void ResponsePacketToUsers(List<CGameUser> listUsers, CPacket response)
        {
            foreach (var user in listUsers)
            {
                user.send(response);
            }
        }

        // 유저 패킷 패키징.
        public void PushPlayerData(CGameUser user, CPacket response)
        {
            response.push(user.player.UserId);
            response.push(user.player.MoveSpeed);
            response.push(user.player.NearRange);
            response.push(user.player.CurrentPosX);
            response.push(user.player.CurrentPosY);
            response.push(user.player.unitDirection);
            response.push(user.player.targetUserId);
            response.push(user.player.playerState);
        }
    }
}