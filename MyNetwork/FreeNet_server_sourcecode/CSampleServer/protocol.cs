using System;
using System.Collections.Generic;
using FreeNet;

namespace GameServer
{
    public enum UnitDirection
    {
        UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP_RIGHT
    }

    public enum UnitType
    {
        PLAYER, MONSTER, ITEM 
    }

    public enum PlayerState
    {
        IDLE, WARK, ATTACK, DAMAGE, CHANGED_DIRECTION, DEATH, PICKED_ITEM,
        
        // 몬스터 상태값.
        DASH_TO_TARGET,        
    }

    public enum ItemType
    {
        WEAPONE, HELMET, INNER, ARMOR, CLOAK, GLOBE, SHIELD, BOOTS, RING1, RING2, NECKLACE, BELT, POTION
    }

    public enum MonsterState
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
        
        PLAYER_RESET_REQ,
        PLAYER_RESET_RES,

        PICKING_ITEM_REQ,
        PICKING_ITEM_RES,
        
        USE_ITEM_REQ,
        USE_ITEM_RES,

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
    public UnitData data;
    public UnitStateData state;
    public HpMp hpMp;
}

public class UnitDataPackage : ResponseData
{
    public UnitData data;
    public UnitStateData state;
    public HpMp hpMp;
    public UnitDataPackage() {}
    public UnitDataPackage(UnitData d, UnitStateData s, HpMp h)
    {
        data = d;
        state = s;
        hpMp = h;
    }  
}

[Serializable]
public class UnitStatePackage : ResponseData
{
    public UnitStateData senderUnitData;
    public UnitStateData receiverUnitData;
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
public class UnitData : ResponseData
{
    public int tableId;
    public Int32 UniqueId;
    public string name;
    public byte unitType;
    public byte moveSpeed;
        
    public UnitData(){}
    public UnitData(CPacket response)
    {
        UniqueId = response.pop_int32();
        name = response.pop_string();
        unitType = response.pop_byte();
        moveSpeed = response.pop_byte();
    }
        
    public void PushData(CPacket response)
    {
        response.push(UniqueId);
        response.push(name);
        response.push(unitType);
        response.push(moveSpeed);
    }
}

[Serializable]
public class UnitStateData : ResponseData
{
    public Int32 UniqueId;
    public byte unitType;
    public byte state;
    public byte direction;
    public short posX;
    public short posY;
    
    public UnitStateData() {}
    public UnitStateData(CPacket msg)
    {
        UniqueId = msg.pop_int32();
        unitType = msg.pop_byte();
        state = msg.pop_byte();
        direction = msg.pop_byte();
        posX = msg.pop_int16();
        posY = msg.pop_int16();
    }

    public void PushData(CPacket response)
    {
        response.push(UniqueId);
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
    public Int32 MaxHp;
    public Int32 MaxMp;
    public Int32 Hp;
    public Int32 Mp;
    public short HpRecoveryTime;
    public short MpRecoveryTime;

    public HpMp() {}
    public HpMp(CPacket msg)
    {
        MaxHp = msg.pop_int32();
        MaxMp = msg.pop_int32();
        Hp = msg.pop_int32();
        Mp = msg.pop_int32();
        HpRecoveryTime = msg.pop_int16();
        MpRecoveryTime = msg.pop_int16();
    }
    public void PushData(CPacket response)
    {
        response.push(MaxHp);
        response.push(MaxMp);
        response.push(Hp);
        response.push(Mp);
        response.push(HpRecoveryTime);
        response.push(MpRecoveryTime);
    }
}

public class UnitInfosPackage : ResponseData
{
    public List<UnitDataPackage> datas = new List<UnitDataPackage>();
}

public class MonsterSpawnDatas : ResponseData
{
    public List<MonsterSawnData> datas = new List<MonsterSawnData>();
}

public class UnitsDataPackate : ResponseData
{
    public List<UnitDataPackage> datas = new List<UnitDataPackage>();
}

public class PlayerDataPackages : ResponseData
{
    public List<UnitData> datas = new List<UnitData>();
}

[Serializable]
public class MonsterSawnData : ResponseData
{
    public int SpawnId;
    public int MonsterId = 1;
    public short SpawnZonePosX = 15;
    public short SpawnZonePosY = 15;
    public int currentSpawnCount;
    public int SpawnZoneRange;
    public int SpawnMaxCount;
    public int SpawnRemainSec;
    public long LastSpawnTime;
    public long NextSpawnTime;
}

public class MonsterAiDatas : ResponseData
{
    public List<MonsterAiData> datas = new List<MonsterAiData>();
}

public class MonsterAiData : ResponseData
{
    public int dataId;
    public int behaviorPatternSpeed;
    public int attackFirst;
    public int searchTargetRange;
}

[Serializable]
public class ItemInfoPackage : ResponseData
{
    public List<ItemInfo> datas = new List<ItemInfo>();
}

[Serializable]
public class ItemInfo : ResponseData
{
    public int uniqueId;
    public int tableId;
    public string itemName;
    public byte itemType;
    public int price;
    public int sellPrice;
    public byte material;
    public int damage;
    public int hitmodifier;
    public int ac;
    public byte useX;
    public byte safenchant;
    public byte consume_type; // 소비형태
    public int count;
    public byte stackable; // 0 중첩 불가 / 1 중첩 가능
    
    public ItemInfo() {}
    
    public ItemInfo(CPacket msg)
    {
        uniqueId = msg.pop_int32();
        tableId = msg.pop_int32();
        itemName = msg.pop_string();
        itemType = msg.pop_byte();
        price = msg.pop_int32();
        sellPrice = msg.pop_int32();
        material = msg.pop_byte();
        damage = msg.pop_int32();
        hitmodifier = msg.pop_int32();
        ac = msg.pop_int32();
        useX = msg.pop_byte();
        safenchant = msg.pop_byte();
        consume_type = msg.pop_byte();
        count = msg.pop_int16();
        stackable = msg.pop_byte();
    }
    public void PushData(CPacket response)
    {
        response.push(uniqueId);
        response.push(tableId);
        response.push(itemName);
        response.push(itemType);
        response.push(price);
        response.push(sellPrice);
        response.push(material);
        response.push(damage);
        response.push(hitmodifier);
        response.push(ac);
        response.push(useX);
        response.push(safenchant);
        response.push(consume_type);
        response.push(count);
        response.push(stackable);
    }
}