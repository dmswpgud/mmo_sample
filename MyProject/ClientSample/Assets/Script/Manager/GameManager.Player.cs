using System.Collections.Generic;
using Client.Game.Map;
using GameServer;
using UnityEngine;

public partial class GameManager
{
    public GameObject PlayerObj;
    public GameObject OtherPlayerObj;
    public List<Player> players = new List<Player>();
    public Player myPlayer;
    private List<GridPoint> path;

    private void UpdateGameManagerPlayer()
    {
        RequestPlayerState();
    }
    
    private void MakeMyPlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerDataPackage) res;
        
        myPlayer = CreatePlayer(data, PlayerObj);
        
        players.Add(myPlayer);
        
        myPlayer.IsMyPlayer = true;

        Camera.main.transform.parent = myPlayer.transform;
                
        Camera.main.transform.localPosition = new Vector3(0f, 8f, -6.7f);

        mapCollider.transform.parent = myPlayer.transform;
                
        mapCollider.transform.position = Vector3.zero;
                
        mapCollider.SetActive(true);
    }
    
    private void MakePlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerDataPackage) res;
        
        var player = CreatePlayer(data, OtherPlayerObj);
        
        players.Add(player);
    }

    private void DisconnectedPlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerData) res;

        DestroyPlayer(data, ERROR.NONE);

        PrintSystemLog($"{data.playerId}님이 서버를 종료했습니다.");
    }
    
    private void DestroyPlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerData) res;
        
        var player = players.Find(p => p.DATA.playerId == data.playerId);

        var index = players.FindIndex(p => p.DATA.playerId == data.playerId);

        RemoveUnitTile(players[index]);
        
        players.RemoveAt(index);
        
        Destroy(player.gameObject);
    }
    
    private Player CreatePlayer(PlayerDataPackage data, GameObject model)
    {
        GameObject ins = Instantiate(model);

        var player = ins.GetComponent<Player>();

        player.InitPlayer(data.data, data.state, data.hpMp);

        return player;
    }

    public Player GetPlayerByUserId(int id)
    {
        return players.Find(p => p.DATA.playerId == id);
    }
    
    private void SetPath(Player player, GridPoint destPoint)
    {
        DrawWall();

        var start = new GridPoint(player.GetCurrentTile.GridPoint.X, player.GetCurrentTile.GridPoint.Y);

        var end = destPoint;

        var pathFinder = new PathFinder();
        
        path = pathFinder.FindPath(tileInfos, start, end);

        // 경로가 없다면 리턴.
        if (path.Count <= 0)
            return;
        
        // 방향 설정.
        player.ChangeDirectionByTargetPoint(path[0].X, path[0].Y);
        
        // 서버에 이동할 경로를 보냄.
        CNetworkManager.Inst.RequestPlayerMove(path[0].X, path[0].Y, player.STATE.direction, ResponseMovePlayer);
        
        // 목표지점에 도착하면 다음 경로로 이동하는걸 경로가 0이 될때까지 반복.
        player.OnArrivePoint = (p) =>
        {
            if (path.Count > 0)
            {
                path.RemoveAt(0);
            }
            
            if (path.Count != 0)
            {
                CNetworkManager.Inst.RequestPlayerMove(path[0].X, path[0].Y, player.STATE.direction, ResponseMovePlayer);
            }
        };

        DrawTile(path);
    }

    private void ResponseMovePlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        //DrawWall();
        
        var data = (PlayerStateData) res;
        
        var player = players.Find(p => p.DATA.playerId == data.playerId);
        
        if (player.STATE.posX == data.posX && player.STATE.posY == data.posY)
        {
            return;
        }
        
        player.MovePlayerNextPosition(data);    

        // var rangeTiles = GetRangeGridPoint(new GridPoint(data.currentPosX, data.currentPosY), data.NearRange);
        // DrawTile(rangeTiles);
    }

    private void RequestPlayerState()
    {
        if (myPlayer == null)
            return;

        if (InputKey.InputAttack)
        {
            var targetTile = GetClickedObject();

            // 내가 나를 클릭하면 리턴.
            if (targetTile.GridPoint.X == myPlayer.STATE.posX &&
                targetTile.GridPoint.Y == myPlayer.STATE.posY)
            {
                return;
            }
            
            var unit = targetTile.GetTileUnit();
            int targetUserId = unit ? unit.DATA.playerId : 0;

            if (unit == null)
            {
                myPlayer.SetPlayerState(PlayerState.ATTACK);
            }

            myPlayer.SetPlayerState(PlayerState.ATTACK, false);
            myPlayer.ChangeDirectionByTargetPoint(targetTile.GridPoint.X, targetTile.GridPoint.Y);
            CNetworkManager.Inst.RequestPlayerState(myPlayer.STATE, targetUserId, OnReceivedChangedPlayerState);
        }
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
            });
        }
        
        receiverPlayer?.SetStateData(data.receiverPlayerData);
    }
}