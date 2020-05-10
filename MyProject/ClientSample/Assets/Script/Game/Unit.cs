using GameServer;
using UnityEngine;

/// <summary>
/// 유닛 방향
/// 유닛 포지션
/// 유닛이 속해있는 타일
/// </summary>
public abstract class Unit : MonoBehaviour
{
    public int userId;
    public UnitDirection Direction;
    private TileInfo currentTile;
    public TileInfo GetCurrentTile => currentTile;

    protected void SetPosition(int x, int y)
    {
        var tile = GameManager.Inst.GetTileInfo(x, y);
        currentTile = tile;
        transform.position = tile.transform.position;
        GameManager.Inst.SetUnitTile(tile, this);
    }
    
    public void SetDirection(UnitDirection dir)
    {
        Direction = dir;
        ChangedDirection(dir);
    }

    protected abstract void ChangedDirection(UnitDirection dir);

    // 이동할 곳에 대한 오브트의 방향을 셋팅.
    public void ChangeDirectionByTargetPoint(int destX, int destY)
    {
        var pos = currentTile.GridPoint;

        if (pos.X > destX && pos.Y > destY)
        {
            Direction = UnitDirection.DOWN_LEFT; //
        }
        else if (pos.X > destX && pos.Y == destY)
        {
            Direction = UnitDirection.LEFT; //
        }
        else if (pos.X > destX && pos.Y < destY)
        {
            Direction = UnitDirection.UP_LEFT; //
        }
        else if (pos.X == destX && pos.Y < destY)
        {
            Direction = UnitDirection.UP; //
        }
        else if (pos.X < destX && pos.Y < destY)
        {
            Direction = UnitDirection.UP_RIGHT; //
        }
        else if (pos.X < destX && pos.Y == destY)
        {
            Direction = UnitDirection.RIGHT; //
        }
        else if (pos.X < destX && pos.Y > destY)
        {
            Direction = UnitDirection.DOWN_LEFT;
        }
        else if (pos.X == destX && pos.Y > destY)
        {
            Direction = UnitDirection.DOWN; //
        }

        SetDirection(Direction);
    }

    // 오브젝트가 바라보는 전방의 타일을 반환.
    public TileInfo GetUnitFrontTile()
    {
        var pos = currentTile.GridPoint;
        
        switch (Direction)
        {
            case UnitDirection.UP:
                return GameManager.Inst.GetTileInfo(pos.X - 1, pos.Y - 1);
            case UnitDirection.UP_RIGHT:
                return GameManager.Inst.GetTileInfo(pos.X - 1, pos.Y);
            case UnitDirection.RIGHT:
                return GameManager.Inst.GetTileInfo(pos.X - 1, pos.Y + 1);
            case UnitDirection.DOWN_RIGHT:
                return GameManager.Inst.GetTileInfo(pos.X, pos.Y + 1);
            case UnitDirection.DOWN:
                return GameManager.Inst.GetTileInfo(pos.X + 1, pos.Y + 1);
            case UnitDirection.DOWN_LEFT:
                return GameManager.Inst.GetTileInfo(pos.X + 1, pos.Y);
            case UnitDirection.LEFT:
                return GameManager.Inst.GetTileInfo(pos.X + 1, pos.Y - 1);
            case UnitDirection.UP_LEFT:
                return GameManager.Inst.GetTileInfo(pos.X, pos.Y - 1);
        }
        
        return null;
    }
}


