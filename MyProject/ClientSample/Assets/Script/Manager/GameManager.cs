using System;
using System.Collections.Generic;
using Client.Game.Map;
using GameServer;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public static GameManager Inst;
    
    public int UserId;

    public void Awake()
    {
        Inst = this;

        StartTitle();
    }

    private void StartTitle()
    {
        LoginDialog.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        ChatDialog.gameObject.SetActive(true);
        
        MakeMap();

        CNetworkManager.Inst.RegisterDisconnectedPlayer(DisconnectedPlayer);
        CNetworkManager.Inst.RequsetGetMyPlayer(MakeMyPlayer);
        CNetworkManager.Inst.RegisterAddNearPlayer(MakePlayer);
        CNetworkManager.Inst.RegisterRemoveNearPlayer(DestroyPlayer);
        CNetworkManager.Inst.RegisterOtherPlayerMove(ResponseMovePlayer);
        CNetworkManager.Inst.RegisterChangedOtherPlayerstate(OnReceivedChangedPlayerState);
    }

    private void Update()
    {
        UpdateGameManagerMap();
        UpdateGameManagerPlayer();
    }
    
    private TileInfo GetClickedObject()
    {
        RaycastHit hit;
        
        GameObject target = null; 

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

        if(Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            var x = hit.point.x;
            var y = hit.point.z;

            return GetTargetPanel(x, y);
        }

        return null; 
    }
}
