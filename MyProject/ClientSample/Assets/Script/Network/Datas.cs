using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponseData {}

public class ChatData : ResponseData
{
    public Int32 userId;
    public string message;
}

public class UserData : ResponseData
{
    public Int32 userId;
}

public class PlayersData : ResponseData
{
    public List<PlayerData> players = new List<PlayerData>();
}

public class PlayerData : ResponseData
{
    public Int32 userId;
    public Int32 MoveSpeed;
    public Int32 NearRange;
    public Int32 currentPosX;
    public Int32 currentPosY;
    public Int32 direction;
    public Int32 playerState;
}

public class PlayerStateData : ResponseData
{
    public Int32 ownerUserId;
    public Int32 playerState;
    public Int32 direction;
    public Int32 receiveUserId;
    public Int32 resultData;
}