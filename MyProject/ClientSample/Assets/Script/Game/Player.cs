using System;
using GameServer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Unit
{
    public HpMp HPMP  { private set; get; }
    private TileInfo nextTile;
    public Action<Player> OnArrivePoint;
    public bool IsMyPlayer;
    public Animator animator;
    
    [SerializeField]
    private GameObject model;
    [SerializeField]
    public GameObject modelHead;
    PlayerAnimationController animController;
    public bool IsDead => STATE == null || STATE?.state == (byte) PlayerState.DEATH;
    public PlayerState UnitState => (PlayerState)STATE.state; 

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
        this.HPMP = hpMp;
        base.Initialized(model);
        SetPlayerAnim(UnitState);
    }
    
    public override void SetStateData(PlayerStateData state)
    {
        SetPlayerAnim((PlayerState)state.state);
        base.SetDirection((UnitDirection) state.direction, 0.2f);
        base.SetPosition(state.posX, state.posY);
    }
    
    public override void SetHpMp(HpMp hpMp)
    {
        HPMP = hpMp;
        targetClicker.SetHp(HPMP.MaxHp, HPMP.Hp);
        Dead();
    }

    public override void MovePlayerNextPosition(PlayerStateData playerData = null)
    {
        STATE = playerData;
        nextTile = GameManager.Inst.GetTileInfo(STATE.posX, STATE.posY);
        SetDirection((UnitDirection)STATE.direction, DATA.moveSpeed / 4f);
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
            base.SetPosition(nextTile.GridPoint.X, nextTile.GridPoint.Y);
            
            OnArrivePoint?.Invoke(this);
            
            nextTile = null;
        }
    }

    public bool SetDirectionByPosition(int destX, int destY)
    {
        return base.SetDirectionByPosition(destX, destY, 0.3f);
    }

    public void SetPlayerAnim(PlayerState state, bool playAnim = true)
    {
        this.STATE.state = (byte)state;

        if (playAnim)
        {
            animController.SetState(state);
        }
    }

    public override void OnFinishedAnim(Action<PlayerState> onFinished)
    {
        animController.OnFinishedAnim = onFinished;
    }

    public void Dead()
    {
        if (!IsDead)
            return;

        if (IsMyPlayer)
        {
            GameManager.Inst.AnnounceDialog.Show("디졌습니다.\n다시시작하세요.", () =>
            {
                CNetworkManager.Inst.RequestReset((res, error) =>
                {
                    SceneManager.LoadScene(0);
                });
            });
        }
    }
}
