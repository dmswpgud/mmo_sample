using System;
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

    public void RequestPlayerMove(Int32 x, Int32 y, Int32 dir, Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_MOVE_REQ);
        msg.push(x);
        msg.push(y);
        msg.push(dir);
        send(msg);
        OnMovePlayer = onRes;
    }
    
    public void RequestPlayerState(Int32 PlayerState,  Int32 unitDirection, Int32 targetUserId, Action<ResponseData, ERROR> onRes)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_STATE_REQ);
        msg.push(PlayerState);
        msg.push(unitDirection);
        msg.push(targetUserId);
        send(msg);
        OnNetworkCallback = onRes;
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
    
    public void RegisterChangedOtherPlayerstate(Action<ResponseData, ERROR> onRes)
    {
        OnReceivedOtherPlayerChangedState = onRes;
    }
}
