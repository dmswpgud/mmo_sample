using System.Collections.Generic;
using Client.Game.Map;
using GameServer;
using UnityEngine;

public partial class GameManager
{
    public GameObject PlayerPrefab;
    public GameObject MonsterPrefab;
    public GameObject ObjectPrefab;
    public List<Unit> listUnit = new List<Unit>();
    public Player myPlayer;
    private List<GridPoint> path;
    public Unit TargetUnit;

    private void OnSetTargetUnit(Unit target)
    {
        TargetUnit = target;
    }

    private Player CreatePlayer(PlayerDataPackage data, GameObject model)
    {
        GameObject ins = Instantiate(model);
        var player = ins.GetComponent<Player>();
        player.InitPlayer(data.data, data.state, data.hpMp);
        CreateTargetClicker(player, player.modelHead.transform);
        player.SetHpMp(data.hpMp);
        return player;
    }

    private void DisconnectedPlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerData) res;
        DestroyUnit(data, ERROR.NONE);
        PrintSystemLog($"{data.playerId}님이 서버를 종료했습니다.");
    }

    public Unit GetPlayerByUserId(int id)
    {
        return listUnit.Find(p => p.DATA.playerId == id);
    }
    
    private void ResponseMovePlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        if (myPlayer.IsDead)
            return;
        
        //DrawWall();
        
        var data = (PlayerStateData) res;
        
        var player = listUnit.Find(p => p.DATA.playerId == data.playerId);
        
        if (player?.STATE.posX == data.posX && player?.STATE.posY == data.posY)
        {
            return;
        }
        
        player?.MovePlayerNextPosition(data);    

        // var rangeTiles = GetRangeGridPoint(new GridPoint(data.currentPosX, data.currentPosY), data.NearRange);
        // DrawTile(rangeTiles);
    }


    private void OnReceivedChangedPlayerState(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        PlayerStatePackage data = (PlayerStatePackage) res;

        var senderPlayer = GetPlayerByUserId(data.senderPlayerData.playerId);
        var receiverPlayer = GetPlayerByUserId(data.receiverPlayerData.playerId);

        if (senderPlayer)
        {
            senderPlayer.SetStateData(data.senderPlayerData);
            senderPlayer.OnFinishedAnim((state) =>
            {
                receiverPlayer?.SetStateData(data.receiverPlayerData);
                receiverPlayer?.SetHpMp(data.receiverPlayerHpMp);
            });
        }
        else
        {
            receiverPlayer?.SetStateData(data.receiverPlayerData);    
        }
    }
    
    private void OnResponsePickingItem(ResponseData res1, ResponseData res2, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var stateData = (PlayerStateData) res1;
        var itemInfo = (ItemInfo) res2;
        
        var player = GetPlayerByUserId(stateData.playerId);

        player.SetStateData(stateData);

        AddItem(itemInfo);

        Debug.Log($"{itemInfo.itemName} {itemInfo.count}을 얻었습니다");
    }
}