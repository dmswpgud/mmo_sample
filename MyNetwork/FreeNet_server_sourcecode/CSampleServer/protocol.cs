namespace GameServer
{
    public enum UnitDirection
    {
        UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP_RIGHT
    }

    public enum UnitType
    {
        PLAYER, ENEMY, MAP_OBJECT 
    }

    public enum PlayerState
    {
        IDLE, WARK, ATTACK, DAMAGE, CHANGED_DIRECTION, DEATH,
    }
    
    public enum PROTOCOL : short
    {
        ERROR = -1,
		
        BEGIN = 0,

        CHAT_MSG_REQ,
        CHAT_MSG_ACK,
        
        ENTER_GAME_ROOM_REQ,
        ENTER_GAME_ROOM_RES,
        
        GET_MY_PLAYER_REQ,
        GET_MY_PLAYER_RES,
        
        DISCONECTED_PLAYER_RES,
        
        PLAYER_MOVE_REQ,
        PLAYER_MOVE_RES,
        
        ADD_NEAR_PLAYER_RES,
        REMOVE_NEAR_PLAYER_RES,
        
        PLAYER_STATE_REQ,
        PLAYER_STATE_RES,

        END
    }

    public enum ERROR : short
    {
        NONE = 0,
        DUPLICATE_USERS = 1,
    }
}