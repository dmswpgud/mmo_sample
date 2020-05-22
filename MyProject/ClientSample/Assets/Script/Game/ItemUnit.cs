using System;
using GameServer;
using UnityEngine;

public class ItemUnit : Unit
{
    [SerializeField]
    private GameObject model;
    
    public void InitPlayer(UnitData data, UnitStateData state, HpMp hpMp)
    {
        base.DATA = data;
        base.STATE = state;
        base.SetPosition(state.posX, state.posY);
        base.Initialized(model);
    }
    
    public override void MovePlayerNextPosition(UnitStateData stateData)
    {

    }

    public override void SetStateData(UnitStateData stateData)
    {
    }

    public override void SetHpMp(HpMp hpMp)
    {
    }

    public override void OnFinishedAnim(Action<PlayerState> onFinished)
    {
    }
}
