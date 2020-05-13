using System;
using FreeNet;
using Newtonsoft.Json;

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
					
					player = new CPlayer(this);
					player.playerData = new PlayerData() {playerId = userId, unitType = 0, moveSpeed = 2, nearRange = 5};
					player.stateData = new PlayerStateData() {playerId = userId, posX = 10, posY = 10, direction = 4};
					player.HpMp = new HpMp() {Hp = 500, Mp = 10};
					Console.WriteLine($"user id {userId}");
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
			Console.WriteLine("The client disconnected.");

			if (player != null)
			{
				Program.PrintLog(JsonConvert.SerializeObject(player.playerData));
				Program.PrintLog(JsonConvert.SerializeObject(player.stateData));
				Program.PrintLog(JsonConvert.SerializeObject(player.HpMp));
				player.DisconnectedPlayer();
			}

			Program.gameServer.DisconnectedUser(player);

			Program.remove_user(this);
		}

		public void send(CPacket msg)
		{
			if (!player.IsPlayer())
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
	}
}
