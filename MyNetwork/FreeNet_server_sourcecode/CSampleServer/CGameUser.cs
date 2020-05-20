using System;
using FreeNet;
using Newtonsoft.Json.Linq;
using GameServer;

namespace CSampleServer
{
	/// <summary>
	/// 하나의 session객체를 나타낸다.
	/// </summary>
	public class CGameUser : IPeer
	{
		CUserToken token;
		public CPlayer player {set; get;}
		public UserDataPackage userDataPackage;

		public CGameUser(CUserToken token)
		{
			this.token = token;
			this.token.set_peer(this);
		}

		void IPeer.on_message(Const<byte[]> buffer)
		{
			// ex)
			CPacket msg = new CPacket(buffer.Value, this);
			PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
			Console.WriteLine("------------------------------------------------------");
			Console.WriteLine("protocol id " + protocol);
			switch (protocol)
			{
				case PROTOCOL.CREATE_ACCOUNT_REQ:
				{
					var account = msg.pop_string();
					var password = msg.pop_string();
					var name = msg.pop_string();
					var newUserId = account.GetHashCode();
					var userDataPackage = SystemUtils.CreateAccount(account, password, name, newUserId);

					if (userDataPackage != null)
					{
						CPacket response = CPacket.create((short)PROTOCOL.CREATE_ACCOUNT_RES);
						response.push(name);
						send(response);
						Program.PrintLog($"[{name}] 계정생성");
					}
					else
					{
						CPacket response = CPacket.create((short)PROTOCOL.ERROR);
						var errorCode = (short) ERROR.DUPLICATE_USERS;
						response.push(errorCode);
						Console.WriteLine($"error code {errorCode}");
						Program.PrintLog($"[ERROR] [{name}] 중복 계정 ");
						send(response);
					}
					break;
				}
				// 서버 접속 요청이 옴.
				case PROTOCOL.ENTER_GAME_ROOM_REQ:
				{
					var account = msg.pop_string();
					var password = msg.pop_string();
					var userId = account.GetHashCode();
					var accountData = SystemUtils.GetUserInfo(userId, password);

					// 계정이 없거나, 패스워드가 틀림.
					if (accountData == null)
					{
						CPacket response = CPacket.create((short)PROTOCOL.ERROR);
						var errorCode = (short) ERROR.NO_ACCOUNT;
						response.push(errorCode);
						Console.WriteLine($"error code {errorCode}");
						send(response);
						return;
					}
					// 중복된 계정.
					if (Program.gameServer.userList.Exists(p => p.playerData.playerId == accountData.userId))
					{
						CPacket response = CPacket.create((short)PROTOCOL.ERROR);
                    	var errorCode = (short) ERROR.DUPLICATE_USERS;
                    	response.push(errorCode);
                    	Console.WriteLine($"error code {errorCode}");
                    	send(response);
                    	return;
					}
					// 플레이어 생성.
					userDataPackage = accountData;
					Console.WriteLine($"user id {account}");
					Program.gameServer.UserEntedServer(this);
					break;
				}
				// 채팅 보내달라고 요청이 옴.
				case PROTOCOL.CHAT_MSG_REQ:
				{
					string text = msg.pop_string();
					Console.WriteLine($"text {text}");
					Program.gameServer.SendChatMessage(this, text);
					break;
				}
				// 내 케릭정보를 보내달라고 요청이 옴.
				case PROTOCOL.GET_MY_PLAYER_REQ:
				{
					if (userDataPackage.state.state == (byte) PlayerState.DEATH)
					{
						PlayerManager.I.ResetDeathPlayer(userDataPackage);
					}
					
					Program.gameServer.ResponseGetMyPlayer(this);
					break;
				}
				// 플레이어 정보만 삭제.
				case PROTOCOL.PLAYER_RESET_REQ:
				{
					DiscconectedPlayerFromWorld();
					CPacket response = CPacket.create((short)PROTOCOL.PLAYER_RESET_RES);
					send(response);
					break;
				}
				// 케릭을 이동시키겠다고 요청이 옴.
				case PROTOCOL.PLAYER_MOVE_REQ:
				{
					var x = msg.pop_int32();
					var y = msg.pop_int32();
					var dir = msg.pop_byte();
					
					// 이동할 타일에 유닛이 있거나, 그 유닛이 죽은 상태가 아니라면 원래 포지션 셋팅 해서 패킷 보냄.
					if (MapManager.I.HasUnit(x, y) ||
					    MapManager.I.GetUnits(x, y).Exists(p => p.stateData.state == (byte)PlayerState.DEATH))
					{
						player.RequestPlayerMove();
						return;
					}
					// 뭐가 없으면 이동 허가. (포지션 셋팅 후 패킷 전송)
					player.SetPosition(x, y, dir);
					player.RequestPlayerMove();
					break;
				}
				// 플레이어가 상태를 보내옴.
				case PROTOCOL.PLAYER_STATE_REQ:
				{
					player.stateData = new PlayerStateData(msg);
					var receiveUserId = msg.pop_int32();
					Console.WriteLine($"[{player.playerData.name}] -> [{receiveUserId}] 상태:{(PlayerState)player.stateData.state}");
					player.RequestPlayerState(receiveUserId);
					break;
				}
			}
		}

		// 접속 종료 이벤트.
		void IPeer.on_removed()
		{
			DiscconectedPlayerFromWorld();
			Program.remove_user(this);
		}

		void DiscconectedPlayerFromWorld()
		{
			if (player != null)
			{
				SaveUserData();
				PlayerManager.I.RemovePlayer(player);
				player.DesconnectedWorld();
				player = null;
			}
		}

		public void send(CPacket msg)
		{
			if (player != null && !player.IsPlayer())
				return;
			
			Console.WriteLine((PROTOCOL)msg.protocol_id);
			this.token.send(msg);
		}

		void IPeer.disconnect()
		{
			this.token.socket.Disconnect(false);
		}

		void IPeer.process_user_operation(CPacket msg)
		{
		}

		private void SaveUserData()
		{
			var userPackage = new UserDataPackage();
			userPackage.userId = userDataPackage.userId;
        	userPackage.account = userDataPackage.account;
        	userPackage.password = userDataPackage.password;
        	userPackage.name = userDataPackage.name;
			userPackage.data = player.playerData;
			userPackage.state = player.stateData;
			userPackage.hpMp = player.HpMp;
			var save = JObject.FromObject(userPackage);
			SystemUtils.SaveUserInfo(userPackage.userId.ToString(), save);
		}
	}
}
