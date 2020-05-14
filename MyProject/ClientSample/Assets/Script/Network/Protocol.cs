using System;
using FreeNet;

namespace GameServer
{
    public enum UnitDirection
    {
        UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP_RIGHT
    }

    public enum UnitType
    {
        PLAYER, MONSTER, MAP_OBJECT 
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
        
        CREATE_ACCOUNT_REQ,
        CREATE_ACCOUNT_RES,
        
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
        
        MONSTER_SPONE_RES,

        END
    }

    public enum ERROR : short
    {
        NONE = 0,
        NO_ACCOUNT,
        DUPLICATE_USERS,
    }
}









[Serializable]
public class ResponseData {}

public class StringResponseData : ResponseData
{
    public string str;
    
    public StringResponseData(CPacket response)
    {
        str = response.pop_string();
    }
    
    public void PushData(CPacket response)
    {
        response.push(str);
    }
}

[Serializable]
public class UserDataPackage : ResponseData
{
    public int userId;
    public string account;
    public string password;
    public string name;
    public PlayerData data;
    public PlayerStateData state;
    public HpMp hpMp;
}

public class PlayerDataPackage : ResponseData
{
    public PlayerData data;
    public PlayerStateData state;
    public HpMp hpMp;
}

[Serializable]
public class PlayerStatePackage : ResponseData
{
    public PlayerStateData senderPlayerData;
    public PlayerStateData receiverPlayerData;
    public HpMp receiverPlayerHpMp;
}

public class ChatData : ResponseData
{
    public Int32 userId;
    public string message;
}

public class PlayerIdData : ResponseData
{
    public Int32 playerId;
    public PlayerIdData(CPacket response)
    {
        playerId = response.pop_int32();
    }
}

[Serializable]
public class PlayerData : ResponseData
{
    public Int32 playerId;
    public string name;
    public byte unitType;
    public byte moveSpeed;
    public byte nearRange;
        
    public PlayerData(){}
    public PlayerData(CPacket response)
    {
        playerId = response.pop_int32();
        name = response.pop_string();
        unitType = response.pop_byte();
        moveSpeed = response.pop_byte();
        nearRange = response.pop_byte();
    }
        
    public void PushData(CPacket response)
    {
        response.push(playerId);
        response.push(name);
        response.push(unitType);
        response.push(moveSpeed);
        response.push(nearRange);
    }
}

[Serializable]
public class PlayerStateData : ResponseData
{
    public Int32 playerId;
    public byte unitType;
    public byte state;
    public byte direction;
    public short posX;
    public short posY;
    
    public PlayerStateData() {}
    public PlayerStateData(CPacket msg)
    {
        playerId = msg.pop_int32();
        unitType = msg.pop_byte();
        state = msg.pop_byte();
        direction = msg.pop_byte();
        posX = msg.pop_int16();
        posY = msg.pop_int16();
    }

    public void PushData(CPacket response)
    {
        response.push(playerId);
        response.push(unitType);
        response.push(state);
        response.push(direction);
        response.push(posX);
        response.push(posY);
    }
}

[Serializable]
public class HpMp : ResponseData
{
    public Int32 Hp;
    public Int32 Mp;

    public HpMp() {}
    public HpMp(CPacket msg)
    {
        Hp = msg.pop_int32();
        Mp = msg.pop_int32();
    }
    public void PushData(CPacket response)
    {
        response.push(Hp);
        response.push(Mp);
    }
}
