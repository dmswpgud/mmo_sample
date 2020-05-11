using System;
using FreeNet;

public class ResponseData {}

public class PlayerDataPackage : ResponseData
{
    public PlayerData data;
    public PlayerStateData state;
    public HpMp hpMp;
}

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

public class PlayerData : ResponseData
{
    public Int32 playerId;
    public byte moveSpeed;
    public byte nearRange;
        
    public PlayerData(){}
    public PlayerData(CPacket response)
    {
        playerId = response.pop_int32();
        moveSpeed = response.pop_byte();
        nearRange = response.pop_byte();
    }
        
    public void PushData(CPacket response)
    {
        response.push(playerId);
        response.push(moveSpeed);
        response.push(nearRange);
    }
}

public class PlayerStateData : ResponseData
{
    public Int32 playerId;
    public byte state;
    public byte direction;
    public short posX;
    public short posY;
    
    public PlayerStateData() {}
    public PlayerStateData(CPacket msg)
    {
        playerId = msg.pop_int32();
        state = msg.pop_byte();
        direction = msg.pop_byte();
        posX = msg.pop_int16();
        posY = msg.pop_int16();
    }

    public void PushData(CPacket response)
    {
        response.push(playerId);
        response.push(state);
        response.push(direction);
        response.push(posX);
        response.push(posY);
    }
}

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
