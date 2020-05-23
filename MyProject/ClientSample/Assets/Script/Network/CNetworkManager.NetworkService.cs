using System;
using FreeNet;
using GameServer;
using UnityEngine;

public partial class CNetworkManager : MonoBehaviour
{
    public void RequestCreateAccount(string account, string password, string characterName, Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.CREATE_ACCOUNT_REQ);
        msg.push(account);
        msg.push(password);
        msg.push(characterName);
        send(msg);
        OnNetworkCallback = onRes;
    }
    public void RequestEnterGameServer(string account, string password, Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_REQ);
        msg.push(account);
        msg.push(password);
        send(msg);
        OnNetworkCallback = onRes;
    }

    public void RequestReset(Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_RESET_REQ);
        send(msg);
        OnNetworkCallback = onRes;
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

    public void RequestPlayerMove(Int32 x, Int32 y, byte dir, Action<ResponseData, ERROR> onRes = null)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_MOVE_REQ);
        msg.push(x);
        msg.push(y);
        msg.push(dir);
        send(msg);
        OnMovePlayer = onRes;
    }
    
    public void RequestPlayerState(UnitStateData stateData, Int32 receiverUserId, Action<ResponseData, ERROR> onRes)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_STATE_REQ);
        stateData.PushData(msg);
        msg.push(receiverUserId);
        send(msg);
        OnNetworkCallback = onRes;
    }

    public void RequestPickingItem(int serverId, Action<ResponseData, ResponseData, ERROR> onRes)
    {
        CPacket msg = CPacket.create((short)PROTOCOL.PICKING_ITEM_REQ);
        msg.push(serverId);
        send(msg);
        OnNetworkCallback2 = onRes;
    }
    
    public void RequestUseItem(int itemId, Action<ResponseData, ResponseData, ERROR> onRes)
    {
            CPacket msg = CPacket.create((short)PROTOCOL.USE_ITEM_REQ);
            msg.push(itemId);
            send(msg);
            OnNetworkCallback2 = onRes;
    }

    public void RegisterDisconnectedServer(Action onRes)
    {
        OnReceivedDisconnectedServer = onRes;
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
