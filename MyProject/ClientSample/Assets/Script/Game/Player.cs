using System;
using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerData PlayerData;

    public TileInfo currentTile;
    
    public TileInfo nextTile;
    
    private List<GridPoint> listPath = new List<GridPoint>();

    public TextMesh textMesh;

    public Action<Player> OnArrivePoint;

    public bool IsMyPlayer;

    public void SetPlayer(PlayerData data)
    {
        PlayerData = data;

        currentTile = GameManager.Inst.GetTileInfo(data.currentPosX, data.currentPosY);
        
        transform.position = currentTile.transform.position;

        textMesh.text = data.userId.ToString();
    }

    public void SetPath(List<GridPoint> path)
    {
        OnArrivePoint = null;
        
        var newPath = new List<GridPoint>();

        if (nextTile != null)
        {
            newPath.Add(nextTile.GridPoint);
        }
        
        newPath.AddRange(path);
        
        listPath = newPath;
    }

    public void MovePlayerNextPosition(PlayerData playerData = null)
    {
        PlayerData = playerData;
        
        if (listPath.Count > 0)
        {
            listPath.RemoveAt(0);
        }

        if (!IsMyPlayer)
        {
            var nextGridPoint = new GridPoint(playerData.currentPosX, playerData.currentPosY);
            listPath.Add(nextGridPoint);
        }
    }

    public void SetPosition(GridPoint position)
    {
        var tile = GameManager.Inst.GetTileInfo(position.X, position.Y);

        transform.position = tile.transform.position;
    }

    private void Update()
    {
        PlayerMoving();
    }

    private void PlayerMoving()
    {
        if (listPath == null || listPath.Count == 0)
            return;

        if (nextTile == null)
        {
            nextTile = GameManager.Inst.GetTileInfo(listPath[0].X, listPath[0].Y);
        }
        
        transform.position = Vector3.MoveTowards(transform.position, nextTile.transform.position, PlayerData.MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextTile.transform.position) < 0.01f)
        {
            currentTile = nextTile;
            
            OnArrivePoint?.Invoke(this);
            
            nextTile = null;
        }
    }
}
