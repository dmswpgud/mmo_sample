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

    private Player CreatePlayer(UnitDataPackage data, GameObject model)
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
        
        var data = (UnitData) res;
        DestroyUnit(data, ERROR.NONE);
        PrintSystemLog($"{data.UniqueId}님이 서버를 종료했습니다.");
    }

    public Unit GetPlayerByUserId(int id)
    {
        return listUnit.Find(p => p.DATA.UniqueId == id);
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
        
        var data = (UnitStateData) res;
        
        var player = listUnit.Find(p => p.DATA.UniqueId == data.UniqueId);
        
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
        
        UnitStatePackage data = (UnitStatePackage) res;

        var senderPlayer = GetPlayerByUserId(data.senderUnitData.UniqueId);
        var receiverPlayer = GetPlayerByUserId(data.receiverUnitData.UniqueId);

        if (senderPlayer)
        {
            senderPlayer.SetStateData(data.senderUnitData);
            senderPlayer.OnFinishedAnim((state) =>
            {
                receiverPlayer?.SetStateData(data.receiverUnitData);
                receiverPlayer?.SetHpMp(data.receiverPlayerHpMp);
            });
        }
        else
        {
            receiverPlayer?.SetStateData(data.receiverUnitData);    
        }
    }
    
    private void OnResponsePickingItem(ResponseData res1, ResponseData res2, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var stateData = (UnitStateData) res1;
        var itemInfo = (ItemInfo) res2;
        
        var player = GetPlayerByUserId(stateData.UniqueId);

        player.SetStateData(stateData);

        AddItem(itemInfo);

        Debug.Log($"{itemInfo.itemName} {itemInfo.count}을 얻었습니다");
    }
}