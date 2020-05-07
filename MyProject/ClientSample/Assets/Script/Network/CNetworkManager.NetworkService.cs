﻿using System;
using FreeNet;
using GameServer;
using UnityEngine;

public partial class CNetworkManager : MonoBehaviour
{
    public void RequestEnterGameServer(Int32 userId, Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_REQ);
        msg.push(userId);
        send(msg);
        OnReceiveConnectedOtherUser = onRes;
    }
    
    public void RequestChatMessage(string message, Action<ResponseData, ERROR> onRes)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.CHAT_MSG_REQ);
        msg.push(message);
        send(msg);
        OnReceiveChatInfoCallback = onRes;
    }

    public void RequsetGetMyPlayer(Action<ResponseData, ERROR> onRes)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.GET_MY_PLAYER_REQ);
        send(msg);
        OnNetworkCallback = onRes;
    }

    public void RequestPlayerMove(Int32 playerId, Int32 x, Int32 y, Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_MOVE_REQ);
        msg.push(playerId);
        msg.push(x);
        msg.push(y);
        send(msg);
        OnMovePlayer = onRes;
    }
    
    public void RegisterChatEvent(Action<ResponseData, ERROR> onRes)
    {
        OnReceiveChatInfoCallback = onRes;
    }

    public void RegisterOtherPlayerMove(Action<ResponseData, ERROR> onRes)
    {
        OnReceiveMoveOtherPlayer = onRes;
    }
    
    public void RegisterDisconnectedPlayer(Action<ResponseData, ERROR> onRes)
    {
        OnDisconnectedPlayer = onRes;
    }
    
    public void RegisterAddNearPlayer(Action<ResponseData, ERROR> onRes)
    {
        OnReceivedAddNearPlayer = onRes;
    }
    
    public void RegisterRemoveNearPlayer(Action<ResponseData, ERROR> onRes)
    {
        OnReceivedRemoveNearPlayer = onRes;
    }
}
