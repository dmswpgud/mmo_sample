using Client.Game.Map;
using GameServer;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 유닛 방향
/// 유닛 포지션
/// 유닛이 속해있는 타일
/// </summary>
public abstract class Unit : MonoBehaviour
{
    public int ID { private set; get; }
    public int X { private set; get; }
    public int Y { private set; get; }
    public UnitDirection Direction  { private set; get; }
    
    GameObject Renderer;

    public void Initialized(int id, int x, int y, UnitDirection direction, GameObject renderer = null)
    {
        ID = id;
        X = x;
        Y = y;
        Direction = direction;
        Renderer = renderer;
        
        //
        SetPosition(x, y);
        SetDirection(Direction);
    }
    
    protected void SetPosition(int x, int y)
    {
        X = x; Y = y;
        var tile = GameManager.Inst.GetTileInfo(x, y);
        transform.position = tile.transform.position;
        GameManager.Inst.SetUnitTile(tile, this);
    }

    protected void SetDirection(UnitDirection dir, float speed = 0f)
    {
        Direction = dir;
        var dirPos = GameUtils.GetUnitFrontTile(X, Y, Direction);
        SetUnitDirectionByPosition(dirPos.X, dirPos.Y, speed);
    }

    protected void SetDirectionByPosition(int destX, int destY, float speed)
    {
        Direction = GameUtils.SetDirectionByPosition(X, Y, destX, destY);
        SetUnitDirectionByPosition(destX, destY, speed);
    }

    private void SetUnitDirectionByPosition(int destX, int destY, float speed)
    {
        Renderer.transform.DOLookAt(new Vector3(destX, 0, destY), speed);
    }
}


