using System.Collections;
using System.Collections.Generic;
using Client.Game.Map;
using GameServer;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public GameObject UnitObj;
    
    public List<Player> players = new List<Player>();
    
    public Player myPlayer;
    
    private void MakeMyPlayer(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerData) res;
        
        myPlayer = CreatePlayer(data);
        
        players.Add(myPlayer);
        
        myPlayer.IsMyPlayer = true;

        Camera.main.transform.parent = myPlayer.transform;
                
        Camera.main.transform.localPosition = new Vector3(1.5f, 12f, 1.5f);

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
        
        var player = CreatePlayer(data);
        
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

        Destroy(player.gameObject);
        
        var index = players.FindIndex(p => p.PlayerData.userId == data.userId);

        players.RemoveAt(index);
    }
    
    private Player CreatePlayer(PlayerData data)
    {
        GameObject ins = Instantiate(UnitObj);

        var player = ins.GetComponent<Player>();

        player.SetPlayer(data);

        return player;
    }

    public Player GetPlayerFromId(int id)
    {
        return players.Find(p => p.PlayerData.userId == id);
    }

    private List<GridPoint> path;
    
    private void SetPath(Player player, GridPoint destPoint)
    {
        DrawWall();

        var start = new GridPoint(player.currentTile.GridPoint.X, player.currentTile.GridPoint.Y);

        var end = destPoint;

        var pathFinder = new PathFinder();
        
        path = pathFinder.FindPath(tileInfos, start, end);

        // 경로가 없다면 리턴.
        if (path.Count <= 0)
            return;
        
        // 서버에 이동할 경로를 보냄.
        CNetworkManager.Inst.RequestPlayerMove(player.PlayerData.userId, path[0].X, path[0].Y, ResponseMovePlayer);
        
        // 목표지점에 도착하면 다음 경로로 이동하는걸 경로가 0이 될때까지 반복.
        player.OnArrivePoint = (p) =>
        {
            if (path.Count > 0)
            {
                path.RemoveAt(0);
            }
            
            if (path.Count != 0)
            {
                CNetworkManager.Inst.RequestPlayerMove(p.PlayerData.userId, path[0].X, path[0].Y, ResponseMovePlayer);
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
            return;
        
        player.MovePlayerNextPosition(data);

        var rangeTiles = GetRangeGridPoint(new GridPoint(data.currentPosX, data.currentPosY), data.NearRange);
        
        //DrawTile(rangeTiles);
    }
}
