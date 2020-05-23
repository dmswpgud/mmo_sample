using UnityEngine;
using System;
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
    public string IpAdress;
    public bool ShowNetworkLog = false;
    CFreeNetUnityService gameserver;
    string received_msg;

    public MonoBehaviour message_receiver;

    private Action<ResponseData, ERROR> OnNetworkCallback;
    private Action<ResponseData, ResponseData, ERROR> OnNetworkCallback2;
    private Action<ResponseData, ResponseData, ResponseData, ERROR> OnNetworkCallback3;
    private Action<ResponseData, ERROR> OnDisconnectedPlayer;
    private Action<ResponseData, ERROR> OnMovePlayer;

    public Action OnReceivedDisconnectedServer;
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
        this.gameserver.connect(IpAdress, 7979);
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
                OnReceivedDisconnectedServer?.Invoke();
                break;
            }
        }
    }

    void on_message(CPacket msg)
    {
        PROTOCOL protocol_id = (PROTOCOL)msg.pop_protocol_id();
        
        if (ShowNetworkLog) {Debug.Log($"Response : {protocol_id}");}
        
        switch (protocol_id)
        {
            case PROTOCOL.ERROR: // 요고 어떻게 쓸지..
            {
                var error = msg.pop_int32();
                var errorCode = (ERROR)error;
                OnNetworkCallback?.Invoke(null, errorCode);
                break;
            }
            case PROTOCOL.CREATE_ACCOUNT_RES:
            {
                var data = new StringResponseData(msg);
                OnNetworkCallback?.Invoke(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.ENTER_GAME_ROOM_RES: // 게임 접속 요청 후 접속 됬다고 알려옴.
            {
                var data = new PlayerIdData(msg);
                OnNetworkCallback?.Invoke(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.PLAYER_RESET_RES:
            {
                OnNetworkCallback?.Invoke(null, ERROR.NONE);
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
                var count = msg.pop_int32();
                var unitPack = new UnitsDataPackate();
                for (int i = 0; i < count; ++i)
                {
                    var data = new UnitDataPackage();
                    data.data = new UnitData(msg);
                    data.state = new UnitStateData(msg);
                    data.hpMp = new HpMp(msg);
                    unitPack.datas.Add(data);
                }
             
                OnNetworkCallback(unitPack, ERROR.NONE);
                break;
            }
            case PROTOCOL.DISCONECTED_PLAYER_RES: // 다른 유저가 접속을 끊었다고 알려옴.
            {
                UnitData data = new UnitData(msg);
                OnDisconnectedPlayer?.Invoke(data, ERROR.NONE);
                break;
            }
            case PROTOCOL.PLAYER_MOVE_RES: // 유닛의 이동 요청 후 이동 횄다고 알려옴.
            {
                var data = new UnitStateData(msg);

                if (GameManager.Inst.UserId == data.UniqueId)
                {
                    OnMovePlayer?.Invoke(data, ERROR.NONE);
                }
                else
                {
                    OnReceiveMoveOtherPlayer?.Invoke(data, ERROR.NONE);    
                }
                break;
            }
            case PROTOCOL.ADD_NEAR_PLAYER_RES: // 범위 밖에 유저가 범위 안으로 들어왔다고 알려옴.
            {
                var count = msg.pop_int32();
                var unitPack = new UnitsDataPackate();
                for (int i = 0; i < count; ++i)
                {
                    var data = new UnitDataPackage();
                    data.data = new UnitData(msg);
                    data.state = new UnitStateData(msg);
                    data.hpMp = new HpMp(msg);
                    unitPack.datas.Add(data);
                }
                OnReceivedAddNearPlayer?.Invoke(unitPack, ERROR.NONE);
                //GameManager.Inst.PrintSystemLog($"{data.userId}님이 범위내에 들어왔습니다.");
                break;
            }
            case PROTOCOL.REMOVE_NEAR_PLAYER_RES: // 범위안에 있던 유닛이 범위 밖으로 나갔다고 알려옴.
            {
                var count = msg.pop_int32();
                var unitPack = new PlayerDataPackages();
                for (int i = 0; i < count; ++i)
                {
                    var data = new UnitData(msg);
                    unitPack.datas.Add(data);
                }
                OnReceivedRemoveNearPlayer?.Invoke(unitPack, ERROR.NONE);
                //GameManager.Inst.PrintSystemLog($"{data.userId}님이 범위내에서 사라졌습니다.");
                break;
            }
            case PROTOCOL.PLAYER_STATE_RES: // 플레이어 상태값을 보내옴.
            {
                var statePackage = new UnitStatePackage();
                statePackage.senderUnitData = new UnitStateData(msg);
                statePackage.receiverUnitData = new UnitStateData(msg);
                statePackage.receiverPlayerHpMp = new HpMp(msg);

                OnReceivedOtherPlayerChangedState?.Invoke(statePackage, ERROR.NONE);
                break;
            }
            case PROTOCOL.PICKING_ITEM_RES:
            {
                var stateData = new UnitStateData(msg);
                var itemInfo = new ItemInfo(msg);
                OnNetworkCallback2?.Invoke(stateData, itemInfo, ERROR.NONE);
                break;
            }
            case PROTOCOL.USE_ITEM_RES:
            {
                var itemInfo = new ItemInfo(msg);
                var pack = new UnitDataPackage();
                pack.data = new UnitData(msg);
                pack.state = new UnitStateData(msg);
                pack.hpMp = new HpMp(msg);
                OnNetworkCallback2?.Invoke(itemInfo, pack, ERROR.NONE);
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
}