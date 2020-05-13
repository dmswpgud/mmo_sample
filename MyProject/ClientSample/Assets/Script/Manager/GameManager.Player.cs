using System.Collections.Generic;
using Client.Game.Map;
using GameServer;
using UnityEngine;

public partial class GameManager
{
    public GameObject PlayerObj;
    public List<Unit> players = new List<Unit>();
    public Player myPlayer;
    private List<GridPoint> path;
    private Unit TargetUnit;

    private void OnSetTargetUnit(Unit target)
    {
        TargetUnit = target;
    }

    private void UpdateGameManagerPlayer()
    {
        RequestPlayerState();
    }
    
    private Player CreatePlayer(PlayerDataPackage data, GameObject model)
    {
        GameObject ins = Instantiate(model);
        var player = ins.GetComponent<Player>();
        player.InitPlayer(data.data, data.state, data.hpMp);
        CreateTargetClicker(player, player.modelHead.transform);
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
        return players.Find(p => p.DATA.playerId == id);
    }
    
    private void SetPath(Player player, GridPoint destPoint)
    {
        DrawWall();

        var start = new GridPoint(player.X, player.Y);

        var end = destPoint;

        var pathFinder = new PathFinder();
        
        path = pathFinder.FindPath(tileInfos, start, end);

        // 경로가 없다면 리턴.
        if (path.Count <= 0)
            return;

        // 방향 설정.
        player.SetDirectionByPosition(path[0].X, path[0].Y);
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
                // 방향 설정.
                //player.SetDirectionByPosition(path[0].X, path[0].Y);
                // 서버에 이동할 경로를 보냄.
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

        #region ATTACK
        if (InputKey.InputAttack)
        {
            var targetTile = GetClickedObject();
            if (targetTile.GridPoint.X == myPlayer.STATE.posX &&
                targetTile.GridPoint.Y == myPlayer.STATE.posY)
                return;
            
            myPlayer.SetDirectionByPosition(targetTile.GridPoint.X, targetTile.GridPoint.Y);
            myPlayer.SetPlayerAnim(PlayerState.ATTACK);
            CNetworkManager.Inst.RequestPlayerState(myPlayer.STATE, receiverUserId: TargetUnit?.ID ?? 0, OnReceivedChangedPlayerState);
            
        }
        #endregion

        #region CHANGE DIRECTION
        if (InputKey.InputChangeDirection)
        {
            var targetTile = GetClickedObject();
            if (targetTile.GridPoint.X == myPlayer.STATE.posX &&
                targetTile.GridPoint.Y == myPlayer.STATE.posY)
                return;

            if (myPlayer.SetDirectionByPosition(targetTile.GridPoint.X, targetTile.GridPoint.Y))
            {
                myPlayer.SetPlayerAnim(PlayerState.CHANGED_DIRECTION);
                CNetworkManager.Inst.RequestPlayerState(myPlayer.STATE, 0, OnReceivedChangedPlayerState);
            }
        }
        #endregion
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
        else
        {
            receiverPlayer?.SetStateData(data.receiverPlayerData);    
        }
    }
}