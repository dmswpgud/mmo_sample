using System;
using GameServer;
using UnityEngine;

public class ItemUnit : Unit
{
    [SerializeField]
    private GameObject model;
    
    public void InitPlayer(PlayerData data, PlayerStateData state, HpMp hpMp)
    {
        base.DATA = data;
        base.STATE = state;
        base.SetPosition(state.posX, state.posY);
        base.Initialized(model);
    }
    
    public override void MovePlayerNextPosition(PlayerStateData stateData)
    {

    }

    public override void SetStateData(PlayerStateData stateData)
    {
    }

    public override void SetHpMp(HpMp hpMp)
    {
    }

    public override void OnFinishedAnim(Action<PlayerState> onFinished)
    {
    }
}
