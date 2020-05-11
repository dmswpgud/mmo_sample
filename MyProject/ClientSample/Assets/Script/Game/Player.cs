using System;
using GameServer;
using UnityEngine;

public class Player : Unit
{
    
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
        this.DATA = data;
        this.STATE = state;
        HPMP = hpMp;
        SetStateData(this.STATE);
    }

    public void MovePlayerNextPosition(PlayerStateData playerData = null)
    {
        STATE = playerData;
        nextTile = GameManager.Inst.GetTileInfo(STATE.posX, STATE.posY);
        ChangeDirectionByTargetPoint(STATE.posX, STATE.posY);
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
        
        SetAnim(PlayerState.WARK);
        
        transform.position = Vector3.MoveTowards(transform.position, nextTile.transform.position, DATA.moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextTile.transform.position) < 0.01f)
        {
            SetPosition(nextTile.GridPoint.X, nextTile.GridPoint.Y);
            
            OnArrivePoint?.Invoke(this);
            
            nextTile = null;
        }
    }
    
    public void SetStateData(PlayerStateData state)
    {
        SetAnim((PlayerState)state.state);
        SetDirection((UnitDirection) state.direction);
        SetPosition(state.posX, state.posY);
    }

    protected override void ChangedDirection(UnitDirection dir)
    {
        STATE.direction = (byte) dir;
        animController.SetDirection(dir);
    }

    protected override void ChangedPosition(int x, int y)
    {
        STATE.posX = (short) x;
        STATE.posY = (short) y;
    }

    public void SetAnim(PlayerState state)
    {
        this.STATE.state = (byte)state;
        animController.SetState(state);
    }
}
