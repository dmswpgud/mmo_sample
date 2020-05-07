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

				case PROTOCOL.CHAT_MSG_REQ:
				{
					string text = msg.pop_string();
					Console.WriteLine($"text {text}");
					Program.gameServer.SendChatMessage(this, text);
					break;
				}

				case PROTOCOL.PLAYERS_REQ:
				{
					Program.gameServer.ResponsePlayers(this);
					break;
				}

				case PROTOCOL.PLAYER_MOVE_REQ:
				{
					var userId = msg.pop_int32();
					player.CurrentPosX = msg.pop_int32();
					player.CurrentPosY = msg.pop_int32();
					Program.gameServer.RequestPlayerMove(this);
					break;
				}
			}
		}

		void IPeer.on_removed()
		{
			Console.WriteLine("The client disconnected.");

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
