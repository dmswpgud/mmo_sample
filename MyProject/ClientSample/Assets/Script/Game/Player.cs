using System;
using GameServer;
using UnityEngine;

public class Player : Unit
{
    public PlayerData DATA { protected set; get; }
    public PlayerStateData STATE { private set; get; }
    public HpMp HPMP  { private set; get; }
    private TileInfo nextTile;
    public Action<Player> OnArrivePoint;
    public bool IsMyPlayer;
    public Animator animator;
    
    [SerializeField]
    private GameObject model;
    PlayerAnimationController animController;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animController = gameObject.AddComponent<PlayerAnimationController>();
        animController.Set(model, animator);
    }

    public void InitPlayer(PlayerData data, PlayerStateData state, HpMp hpMp)
    {
        base.Initialized(data.playerId, state.posX, state.posY, (UnitDirection) state.direction, model);
        this.DATA = data;
        this.STATE = state;
        this.HPMP = hpMp;
    }
    
    public void SetStateData(PlayerStateData state)
    {
        SetPlayerAnim((PlayerState)state.state);
        SetDirection((UnitDirection) state.direction);
        SetPosition(state.posX, state.posY);
    }

    public void MovePlayerNextPosition(PlayerStateData playerData = null)
    {
        STATE = playerData;
        nextTile = GameManager.Inst.GetTileInfo(STATE.posX, STATE.posY);
        SetDirectionByPosition(STATE.posX, STATE.posY, DATA.moveSpeed / 4f);
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
        
        SetPlayerAnim(PlayerState.WARK);
        
        transform.position = Vector3.MoveTowards(transform.position, nextTile.transform.position, DATA.moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextTile.transform.position) < 0.01f)
        {
            SetPosition(nextTile.GridPoint.X, nextTile.GridPoint.Y);
            
            OnArrivePoint?.Invoke(this);
            
            nextTile = null;
        }
    }

    public void SetDirection(UnitDirection dir)
    {
        if (STATE.direction == (byte)dir)
            return;
        
        STATE.direction = (byte) dir;
        base.SetDirection(dir, 0.2f);
    }

    public bool SetDirectionByPosition(int destX, int destY)
    {
        var dir = GameUtils.SetDirectionByPosition(X, Y, destX, destY);
        if (STATE.direction != (byte) dir)
        {
            STATE.direction = (byte) dir;
            base.SetDirectionByPosition(destX, destY, 0.3f);
            return true;
        }

        return false;
    }

    public void SetPlayerAnim(PlayerState state, bool playAnim = true)
    {
        this.STATE.state = (byte)state;

        if (playAnim)
        {
            animController.SetState(state);
        }
    }

    public void OnFinishedAnim(Action<PlayerState> onFinished)
    {
        animController.OnFinishedAnim = onFinished;
    }
}
