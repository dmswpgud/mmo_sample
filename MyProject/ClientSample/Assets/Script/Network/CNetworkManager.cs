using UnityEngine;
using System;
using System.Collections.Generic;
using FreeNet;
using FreeNetUnity;
using GameServer;

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
    private USER_STATE user_state = USER_STATE.NOT_CONNECTED;
    
    enum USER_STATE
    {
        NOT_CONNECTED,
        CONNECTED,
    }

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
            case PROTOCOL.ERROR:
            {
                var error = msg.pop_int32();
                var errorCode = (ERROR)error;
                OnNetworkCallback(null, errorCode);
                break;
            }
            
            case PROTOCOL.ENTER_GAME_ROOM_RES:
            {
                var userId = msg.pop_int32();
                var userData = new UserData();
                userData.userId = userId;
                OnReceiveConnectedOtherUser?.Invoke(userData, ERROR.NONE);
                break;
            }
            case PROTOCOL.CHAT_MSG_ACK:
            {
                int userId = msg.pop_int32();
                string text = msg.pop_string();
                var res = new ChatData();
                res.userId = userId;
                res.message = text;
                OnReceiveChatInfoCallback(res, ERROR.NONE);
                break;
            }
            case PROTOCOL.GET_MY_PLAYER_RES:
            {
                PlayerData data = CreatePlayerData(msg);
                
                OnNetworkCallback(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.DISCONECTED_PLAYER_RES:
            {
                PlayerData data = new PlayerData();
                data.userId = msg.pop_int32();

                OnDisconnectedPlayer(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.PLAYER_MOVE_RES:
            {
                PlayerData data = CreatePlayerData(msg);

                if (GameManager.Inst.UserId == data.userId)
                    OnMovePlayer(data, ERROR.NONE);
                else
                {
                    OnReceiveMoveOtherPlayer?.Invoke(data, ERROR.NONE);    
                }
                break;
            }
            case PROTOCOL.ADD_NEAR_PLAYER_RES:
            {
                PlayerData data = CreatePlayerData(msg);
                
                OnReceivedAddNearPlayer?.Invoke(data, ERROR.NONE);
                
                GameManager.Inst.PrintSystemLog($"{data.userId}님이 범위내에 들어왔습니다.");
                break;
            }
            case PROTOCOL.REMOVE_NEAR_PLAYER_RES:
            {
                PlayerData data = CreatePlayerData(msg);
                
                OnReceivedRemoveNearPlayer?.Invoke(data, ERROR.NONE);
                
                GameManager.Inst.PrintSystemLog($"{data.userId}님이 범위내에서 사라졌습니다.");
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
        
        return data;
    }
}