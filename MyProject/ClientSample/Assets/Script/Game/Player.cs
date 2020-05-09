using System;
using GameServer;
using UnityEngine;

public class Player : ObjectBase
{
    public PlayerData PlayerData;
    
    private TileInfo nextTile;
    
    public TextMesh textMesh;

    public Action<Player> OnArrivePoint;

    public bool IsMyPlayer;

    public void SetPlayer(PlayerData data)
    {
        PlayerData = data;

        SetPosition(data.currentPosX, data.currentPosY);

        SetDirection((ObjectDirection)data.direction);

        textMesh.text = data.userId.ToString();
    }

    public void MovePlayerNextPosition(PlayerData playerData = null)
    {
        PlayerData = playerData;

        nextTile = GameManager.Inst.GetTileInfo(PlayerData.currentPosX, PlayerData.currentPosY);
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
            SetPosition(nextTile.GridPoint.X, nextTile.GridPoint.Y);
            
            OnArrivePoint?.Invoke(this);
            
            nextTile = null;
        }
    }
}
