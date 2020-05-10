using System;
using FreeNet;

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
				// 서버 접속 요청이 옴.
				case PROTOCOL.ENTER_GAME_ROOM_REQ:
				{
					var userId = msg.pop_int32();
					
					// 유저 중복 체크
					if (Program.gameServer.ExistsUser(userId))
					{
						CPacket response = CPacket.create((short)PROTOCOL.ERROR);
						var errorCode = (short) ERROR.DUPLICATE_USERS;
						response.push(errorCode);
						Console.WriteLine($"error code {errorCode}");
						send(response);
						return;
					}
					
					player = new CPlayer(this, userId);

					Console.WriteLine($"user id {userId}");
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
					player.MoveSpeed = 2;
					player.NearRange = 5;
					player.CurrentPosX = 10;
					player.CurrentPosY = 10;
					player.unitDirection = 4; // DOWN
					Program.gameServer.ResponseGetMyPlayer(this);
					player.SetPosition(player.CurrentPosX, player.CurrentPosY, player.unitDirection);
					break;
				}
				// 케릭을 이동시키겠다고 요청이 옴.
				case PROTOCOL.PLAYER_MOVE_REQ:
				{
					var x = msg.pop_int32();
					var y = msg.pop_int32();
					var dir = msg.pop_int32();
					
					// 이동할 좌표에 뭐가 있는지 체크.
					var nearObjects = GameUtils.GetNearUserFromPosition(x, y, player.listNearbyUser);
					
					// 뭐가 있으면 이동 불허. (포지션 셋팅 안하고 그냥 패킷 보냄)
					if (nearObjects.Count != 0)
					{
						player.RequestPlayerMove(this);
						return;
					}

					// 뭐가 없으면 이동 허가. (포지션 셋팅 후 패킷 전송)
					player.SetPosition(x, y, dir);
					player.RequestPlayerMove(this);
					break;
				}
				// 플레이어가 상태를 보내옴.
				case PROTOCOL.PLAYER_STATE_REQ:
				{
					player.playerState = msg.pop_int32();
					player.unitDirection = msg.pop_int32();
					var receiveUserId = msg.pop_int32();
					player.RequestPlayerState(this, receiveUserId);
					break;
				}
			}
		}

		// 접속 종료 이벤트.
		void IPeer.on_removed()
		{
			Console.WriteLine("The client disconnected.");

			if (player != null)
			{
				player.DisconnectedPlayer();
			}

			Program.gameServer.DisconnectedUser(this);

			Program.remove_user(this);
		}

		public void send(CPacket msg)
		{
			this.token.send(msg);
		}

		void IPeer.disconnect()
		{
			this.token.socket.Disconnect(false);
		}

		void IPeer.process_user_operation(CPacket msg)
		{
		}
	}
}
