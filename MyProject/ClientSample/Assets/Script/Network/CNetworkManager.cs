using UnityEngine;
using System;
using System.Collections.Generic;
using FreeNet;
using FreeNetUnity;
using GameServer;

public enum USER_STATE
{
    NOT_CONNECTED,
    CONNECTED,
}

public partial class CNetworkManager : MonoBehaviour {

    public static CNetworkManager Inst;
    CFreeNetUnityService gameserver;
    string received_msg;

    public MonoBehaviour message_receiver;

    private Action<ResponseData, ERROR> OnNetworkCallback;
    private Action<ResponseData, ERROR> OnDisconnectedPlayer;
    private Action<ResponseData, ERROR> OnMovePlayer;
    
    private Action<ResponseData, ERROR> OnReceiveConnectedOtherUser;
    private Action<ResponseData, ERROR> OnReceiveChatInfoCallback;
    private Action<ResponseData, ERROR> OnReceiveMoveOtherPlayer;
    private Action<ResponseData, ERROR> OnReceivedAddNearPlayer;
    private Action<ResponseData, ERROR> OnReceivedRemoveNearPlayer;
    private Action<ResponseData, ERROR> OnReceivedOtherPlayerChangedState;
    public USER_STATE user_state = USER_STATE.NOT_CONNECTED;

    void Awake()
    {
        Inst = this;
        
        this.received_msg = "";

        this.gameserver = gameObject.AddComponent<CFreeNetUnityService>();

        this.gameserver.appcallback_on_status_changed += on_status_changed;

        this.gameserver.appcallback_on_message += on_message;

        connect();
    }

    public void connect()
    {
        this.gameserver.connect("127.0.0.1", 7979);
    }

    public bool is_connected()
    {
        return this.gameserver.is_connected();
    }

    void on_status_changed(NETWORK_EVENT status)
    {
        switch (status)
        {
            case NETWORK_EVENT.connected:
            {
                CLogManager.log("on connected");
                this.received_msg += "on connected\n";
                user_state = USER_STATE.CONNECTED;
                break;
            }
            case NETWORK_EVENT.disconnected:
            {
                CLogManager.log("disconnected");
                this.received_msg += "disconnected\n";
                user_state = USER_STATE.NOT_CONNECTED;
                break;
            }
        }
    }

    void on_message(CPacket msg)
    {
        PROTOCOL protocol_id = (PROTOCOL)msg.pop_protocol_id();
        
        Debug.Log($"Response : {protocol_id}");
        
        switch (protocol_id)
        {
            case PROTOCOL.ERROR: // 요고 어떻게 쓸지..
            {
                var error = msg.pop_int32();
                var errorCode = (ERROR)error;
                OnNetworkCallback(null, errorCode);
                break;
            }
            
            case PROTOCOL.ENTER_GAME_ROOM_RES: // 게임 접속 요청 후 접속 됬다고 알려옴.
            {
                var userId = msg.pop_int32();
                var userData = new UserData();
                userData.userId = userId;
                OnReceiveConnectedOtherUser?.Invoke(userData, ERROR.NONE);
                break;
            }
            case PROTOCOL.CHAT_MSG_ACK: // 채팅 정보 받음.
            {
                var res = new ChatData();
                res.userId = msg.pop_int32();;
                res.message =  msg.pop_string();
                OnReceiveChatInfoCallback(res, ERROR.NONE);
                break;
            }
            case PROTOCOL.GET_MY_PLAYER_RES: // 내 케릭을 달라고 요청하고 정보를 알려옴.
            {
                PlayerData data = CreatePlayerData(msg);
                OnNetworkCallback(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.DISCONECTED_PLAYER_RES: // 다른 유저가 접속을 끊었다고 알려옴.
            {
                PlayerData data = new PlayerData();
                data.userId = msg.pop_int32();
                OnDisconnectedPlayer?.Invoke(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.PLAYER_MOVE_RES: // 유닛의 이동 요청 후 이동 횄다고 알려옴.
            {
                PlayerData data = CreatePlayerData(msg);

                if (GameManager.Inst.UserId == data.userId)
                    OnMovePlayer?.Invoke(data, ERROR.NONE);
                else
                {
                    OnReceiveMoveOtherPlayer?.Invoke(data, ERROR.NONE);    
                }
                break;
            }
            case PROTOCOL.ADD_NEAR_PLAYER_RES: // 범위 밖에 유저가 범위 안으로 들어왔다고 알려옴.
            {
                PlayerData data = CreatePlayerData(msg);
                OnReceivedAddNearPlayer?.Invoke(data, ERROR.NONE);
                //GameManager.Inst.PrintSystemLog($"{data.userId}님이 범위내에 들어왔습니다.");
                break;
            }
            case PROTOCOL.REMOVE_NEAR_PLAYER_RES: // 범위안에 있던 유닛이 범위 밖으로 나갔다고 알려옴.
            {
                PlayerData data = CreatePlayerData(msg);
                OnReceivedRemoveNearPlayer?.Invoke(data, ERROR.NONE);
                //GameManager.Inst.PrintSystemLog($"{data.userId}님이 범위내에서 사라졌습니다.");
                break;
            }
            case PROTOCOL.PLAYER_STATE_RES: // 플레이어 상태값을 보내옴.
            {
                PlayerStateData stateData = CreatePlayerStateData(msg);
                OnReceivedOtherPlayerChangedState?.Invoke(stateData, ERROR.NONE);
                break;
            }
        }
    }

    private void send(CPacket msg)
    {
        if (user_state == USER_STATE.NOT_CONNECTED)
        {
            Debug.Log("is not connected");
            return;
        }
        
        this.gameserver.send(msg);
    }

    private PlayerData CreatePlayerData(CPacket msg)
    {
        PlayerData data = new PlayerData();
        data.userId = msg.pop_int32();
        data.MoveSpeed = msg.pop_int32();
        data.NearRange = msg.pop_int32();
        data.currentPosX = msg.pop_int32();
        data.currentPosY = msg.pop_int32();
        data.direction = msg.pop_int32();
        data.playerState = msg.pop_int32();
        
        return data;
    }
    
    public PlayerStateData CreatePlayerStateData(CPacket msg)
    {
        PlayerStateData data = new PlayerStateData();
        data.ownerUserId = msg.pop_int32();
        data.playerState = msg.pop_int32();
        data.direction = msg.pop_int32();
        data.receiveUserId = msg.pop_int32();
        data.resultData = msg.pop_int32();
        
        return data;
    }
    
    // 유저 패킷 패키징.
    public void PushPlayerData(Unit unit, CPacket msg)
    {
        var player = (Player) unit;
        msg.push(player.PlayerData.userId);
        msg.push(player.PlayerData.MoveSpeed);
        msg.push(player.PlayerData.NearRange);
        msg.push(player.PlayerData.currentPosX);
        msg.push(player.PlayerData.currentPosY);
        msg.push(player.PlayerData.direction);
        msg.push(player.PlayerData.playerState);
    }

    public void PushPlayerStateData(PlayerStateData stateData, CPacket msg)
    {
        msg.push(stateData.ownerUserId);
        msg.push(stateData.playerState);
        msg.push(stateData.direction);
        msg.push(stateData.receiveUserId);
        msg.push(stateData.resultData);
    }
}