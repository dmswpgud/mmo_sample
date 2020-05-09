using GameServer;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public ObjectDirection Direction;

    private TileInfo currentTile;

    public TileInfo GetCurrentTile => currentTile;

    protected void SetPosition(int x, int y)
    {
        var tile = GameManager.Inst.GetTileInfo(x, y);
        currentTile = tile;
        transform.position = tile.transform.position;
        GameManager.Inst.SetObjectBase(tile, this);
    }
    
    public void SetDirection(ObjectDirection dir)
    {
        Direction = dir;
    }

    // 이동할 곳에 대한 오브트의 방향을 셋팅.
    public void MoveSetDirection(int destX, int destY)
    {
        var pos = currentTile.GridPoint;

        if (pos.X > destX && pos.Y > destY)
        {
            Direction = ObjectDirection.UP;
        }
        else if (pos.X > destX && pos.Y == destY)
        {
            Direction = ObjectDirection.UP_RIGHT;
        }
        else if (pos.X > destX && pos.Y < destY)
        {
            Direction = ObjectDirection.RIGHT;
        }
        else if (pos.X == destX && pos.Y < destY)
        {
            Direction = ObjectDirection.DOWN_RIGHT;
        }
        else if (pos.X < destX && pos.Y < destY)
        {
            Direction = ObjectDirection.DOWN;
        }
        else if (pos.X < destX && pos.Y == destY)
        {
            Direction = ObjectDirection.DOWN_LEFT;
        }
        else if (pos.X < destX && pos.Y > destY)
        {
            Direction = ObjectDirection.LEFT;
        }
        else if (pos.X == destX && pos.Y > destY)
        {
            Direction = ObjectDirection.UP_LEFT;
        }
    }

    // 오브젝트가 바라보는 전방의 타일을 반환.
    public TileInfo GetObjectFrontTile()
    {
        var pos = currentTile.GridPoint;
        
        switch (Direction)
        {
            case ObjectDirection.UP:
                return GameManager.Inst.GetTileInfo(pos.X - 1, pos.Y - 1);
            case ObjectDirection.UP_RIGHT:
                return GameManager.Inst.GetTileInfo(pos.X - 1, pos.Y);
            case ObjectDirection.RIGHT:
                return GameManager.Inst.GetTileInfo(pos.X - 1, pos.Y + 1);
            case ObjectDirection.DOWN_RIGHT:
                return GameManager.Inst.GetTileInfo(pos.X, pos.Y + 1);
            case ObjectDirection.DOWN:
                return GameManager.Inst.GetTileInfo(pos.X + 1, pos.Y + 1);
            case ObjectDirection.DOWN_LEFT:
                return GameManager.Inst.GetTileInfo(pos.X + 1, pos.Y);
            case ObjectDirection.LEFT:
                return GameManager.Inst.GetTileInfo(pos.X + 1, pos.Y - 1);
            case ObjectDirection.UP_LEFT:
                return GameManager.Inst.GetTileInfo(pos.X, pos.Y - 1);
        }
        
        return null;
    }
}


