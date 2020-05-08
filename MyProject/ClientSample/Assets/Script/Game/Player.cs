using System;
using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerData PlayerData;

    public TileInfo currentTile;
    
    private TileInfo nextTile;
    
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

    public void MovePlayerNextPosition(PlayerData playerData = null)
    {
        PlayerData = playerData;

        nextTile = GameManager.Inst.GetTileInfo(PlayerData.currentPosX, PlayerData.currentPosY);
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
        if (nextTile == null)
        {
            return;
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
