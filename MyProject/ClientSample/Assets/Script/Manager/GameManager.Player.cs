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
        
        var data = (PlayerData) res;
        
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
        
        var data = (PlayerData) res;
        
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

        PrintSystemLog($"{data.userId}님이 서버를 종료했습니다.");
    }
    
    private void DestroyPlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerData) res;
        
        var player = players.Find(p => p.PlayerData.userId == data.userId);

        var index = players.FindIndex(p => p.PlayerData.userId == data.userId);

        RemoveUnitTile(players[index]);
        
        players.RemoveAt(index);
        
        Destroy(player.gameObject);
    }
    
    private Player CreatePlayer(PlayerData data, GameObject model)
    {
        GameObject ins = Instantiate(model);

        var player = ins.GetComponent<Player>();

        player.InitPlayer(data);

        return player;
    }

    public Player GetPlayerFromId(int id)
    {
        return players.Find(p => p.PlayerData.userId == id);
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
        CNetworkManager.Inst.RequestPlayerMove(path[0].X, path[0].Y, (int)player.Direction, ResponseMovePlayer);
        
        // 목표지점에 도착하면 다음 경로로 이동하는걸 경로가 0이 될때까지 반복.
        player.OnArrivePoint = (p) =>
        {
            if (path.Count > 0)
            {
                path.RemoveAt(0);
            }
            
            if (path.Count != 0)
            {
                CNetworkManager.Inst.RequestPlayerMove(path[0].X, path[0].Y, (int)player.Direction, ResponseMovePlayer);
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
        
        var data = (PlayerData) res;
        
        var player = players.Find(p => p.PlayerData.userId == data.userId);
        
        if (player.PlayerData.currentPosX == data.currentPosX && player.PlayerData.currentPosY == data.currentPosY)
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
            if (targetTile.GridPoint.X == myPlayer.PlayerData.currentPosX &&
                targetTile.GridPoint.Y == myPlayer.PlayerData.currentPosY)
            {
                return;
            }
            
            var unit = targetTile.GetTileUnit();
            int targetUserId = unit ? unit.userId : 0;

            myPlayer.SetState(PlayerState.ATTACK);
            myPlayer.ChangeDirectionByTargetPoint(targetTile.GridPoint.X, targetTile.GridPoint.Y);
            CNetworkManager.Inst.RequestPlayerState((int) PlayerState.ATTACK, (int)myPlayer.Direction, targetUserId, OnReceivedChangedPlayerState);
        }
    }

    private void OnReceivedChangedPlayerState(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }

        PlayerStateData data = (PlayerStateData) res;

        switch ((PlayerState)data.playerState)
        {
            case PlayerState.ATTACK:
            {
                // 어택 요청을 보내고 어택 결과를 받는다.
                if (data.ownerUserId == UserId)
                {
                    var defecderPlayer = GetPlayerFromId(data.receiveUserId);
                    defecderPlayer?.SetState(PlayerState.DAMAGE);
                    var str = $"내가 {data.receiveUserId}님에게 {(PlayerState)data.playerState}하고 있습니다.";
                    PrintSystemLog(str);
                }
                // 다른 유저가 싸우는거 브로드캐스트 받음.
                // 공격자, 피격자의 상태 애니메이션 재생 셔켜야댐.
                else
                {
                    var ownerPlayer = GetPlayerFromId(data.ownerUserId);
                    ownerPlayer?.SetDirection((UnitDirection)data.direction);
                    ownerPlayer?.SetState(PlayerState.ATTACK);
                    var defecderPlayer = GetPlayerFromId(data.receiveUserId);
                    defecderPlayer?.SetState(PlayerState.DAMAGE);
                    var str = $"{data.ownerUserId}님이 {data.receiveUserId}님에게 {(PlayerState)data.playerState}하고 있습니다.";
                    PrintSystemLog(str);
                }
                break;
            }
            case PlayerState.DAMAGE:
            {
                var str = $"{data.ownerUserId}님이 나에게 {(PlayerState)data.playerState}했습니다.";
                var attacker = GetPlayerFromId(data.ownerUserId);
                attacker.SetState(PlayerState.ATTACK);
                attacker.SetDirection((UnitDirection)data.direction);
                myPlayer.SetState(PlayerState.DAMAGE);
                break;
            }
            case PlayerState.CHANGED_DIRECTION:
            {
                var ownerPlayer = GetPlayerFromId(data.ownerUserId);
                ownerPlayer.SetDirection((UnitDirection)data.direction);
                var str = $"{data.ownerUserId}님이 {(UnitDirection)data.direction}로 방향을 돌렸습니다.";
                PrintSystemLog(str);
                break;
            }
        }
    }
}