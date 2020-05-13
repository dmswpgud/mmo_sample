using System;
using FreeNet;
using Newtonsoft.Json.Linq;

namespace CSampleServer
{
	using GameServer;

	/// <summary>
	/// 하나의 session객체를 나타낸다.
	/// </summary>
	public class CGameUser : IPeer
	{
		CUserToken token;
		
		public CPlayer player {private set; get;}
		private UserDataPackage userDataPackage;

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
						return;
					}
					else
					{
						CPacket response = CPacket.create((short)PROTOCOL.ERROR);
						var errorCode = (short) ERROR.DUPLICATE_USERS;
						response.push(errorCode);
						Console.WriteLine($"error code {errorCode}");
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
					
					if (Program.gameServer.userList.Exists(p => p.playerData.playerId == accountData.userId))
					{
							CPacket response = CPacket.create((short)PROTOCOL.ERROR);
                    		var errorCode = (short) ERROR.DUPLICATE_USERS;
                    		response.push(errorCode);
                    		Console.WriteLine($"error code {errorCode}");
                    		send(response);
                    		return;
					}
					
					userDataPackage = accountData;
					player = new CPlayer(this);
					player.playerData = accountData.data;
					player.stateData = accountData.state;
					player.HpMp = accountData.hpMp;
					
					Console.WriteLine($"user id {account}");
					Program.gameServer.UserEntedServer(player);
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
					Program.gameServer.ResponseGetMyPlayer(this);
					player.SetPosition(player.stateData.posX, player.stateData.posY, player.stateData.direction);
					break;
				}
				// 케릭을 이동시키겠다고 요청이 옴.
				case PROTOCOL.PLAYER_MOVE_REQ:
				{
					var x = msg.pop_int32();
					var y = msg.pop_int32();
					var dir = msg.pop_byte();
					
					// 이동할 좌표에 뭐가 있는지 체크.
					var nearObjects = GameUtils.GetNearUserFromPosition(x, y, player.listNearbyUser);
					
					// 뭐가 있으면 이동 불허. (포지션 셋팅 안하고 그냥 패킷 보냄)
					if (nearObjects.Count != 0)
					{
						player.RequestPlayerMove();
						return;
					}
					Console.WriteLine($"아이디 {player.stateData.playerId} 이동 이전좌표 {player.stateData.posX} {player.stateData.posY} {(UnitDirection)player.stateData.direction}  좌표 :{x} {y} {(UnitDirection)dir}");
					
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
					Console.WriteLine($"공격자 {player.stateData.playerId} 타겟 :{receiveUserId} 상태:{(PlayerState)player.stateData.state}");
					player.RequestPlayerState(receiveUserId);
					break;
				}
			}
		}

		// 접속 종료 이벤트.
		void IPeer.on_removed()
		{
			if (player != null)
			{
				SaveUserData();
				
				player.DisconnectedPlayer();
			}

			Program.gameServer.DisconnectedUser(player);

			Program.remove_user(this);
		}

		public void send(CPacket msg)
		{
			if (player != null && !player.IsPlayer())
				return;
			
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
