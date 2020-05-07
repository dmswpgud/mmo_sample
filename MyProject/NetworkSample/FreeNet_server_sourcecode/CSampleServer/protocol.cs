namespace GameServer
{
	public enum PROTOCOL : short
	{
		ERROR = -1,
		
		BEGIN = 0,

		CHAT_MSG_REQ = 1,
		CHAT_MSG_ACK = 2,
		ENTER_GAME_ROOM_REQ = 3,
		ENTER_GAME_ROOM_RES = 4,
		PLAYERS_REQ = 5,
		PLAYERS_RES = 6,
		ADD_NEW_PLAYER_RES = 7,
		DISCONECTED_PLAYER_RES = 8,
		PLAYER_MOVE_REQ = 9,
		PLAYER_MOVE_RES = 10,

		END
	}

	public enum ERROR : short
	{
		NONE = 0,
		DUPLICATE_USERS = 1,
	}
}
