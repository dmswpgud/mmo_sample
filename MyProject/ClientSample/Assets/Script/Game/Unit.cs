using System;
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
    public PlayerData DATA { protected set; get; }
    public PlayerStateData STATE { protected set; get; }

    [SerializeField] private int InspectorView_unitId;
    public int ID => DATA.playerId;
    public int X => STATE.posX;
    public int Y => STATE.posY;
    public UnitDirection Direction => (UnitDirection)STATE.direction;
    
    public GameObject Renderer { private set; get; }

    public TargetClicker targetClicker;

    public void Initialized(GameObject renderer = null)
    {
        Renderer = renderer;
        
        InspectorView_unitId = ID;
        //
        SetPosition(X, Y);
        SetDirection(Direction);
    }

    void OnDestroy()
    {
        Destroy(targetClicker.gameObject);
    }

    public abstract void MovePlayerNextPosition(PlayerStateData stateData);
    public abstract void SetStateData(PlayerStateData stateData);
    public abstract void OnFinishedAnim(Action<PlayerState> onFinished);

    protected void SetPosition(int x, int y)
    {
        STATE.posX = (short)x;
        STATE.posY = (short)y;
        var tile = GameManager.Inst.GetTileInfo(x, y);
        transform.position = tile.transform.position;
        GameManager.Inst.SetUnitTile(tile, this);
    }

    protected void SetDirection(UnitDirection dir, float speed = 0f)
    {
        STATE.direction = (byte) dir;
        var dirPos = GameUtils.GetUnitFrontTile(X, Y, Direction);
        SetUnitDirectionByPosition(dirPos.X, dirPos.Y, speed);
    }

    protected bool SetDirectionByPosition(int destX, int destY, float speed)
    {
        var dir = GameUtils.SetDirectionByPosition(X, Y, destX, destY);
        STATE.direction = (byte) dir;
        SetUnitDirectionByPosition(destX, destY, speed);
        
        return true;
    }

    private void SetUnitDirectionByPosition(int destX, int destY, float speed)
    {
        Renderer.transform.DOLookAt(new Vector3(destX, 0, destY), speed);
    }
}


