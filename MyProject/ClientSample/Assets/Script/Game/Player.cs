using System;
using GameServer;
using UnityEngine;

public class Player : Unit
{
    public PlayerData PlayerData { private set; get; }
    
    private TileInfo nextTile;
    
    //public TextMesh textMesh;

    public Action<Player> OnArrivePoint;

    public bool IsMyPlayer;
    
    public Animator animator;

    private PlayerState playerState = PlayerState.IDLE;

    [SerializeField]
    private GameObject model;
    
    PlayerAnimationController animController;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animController = gameObject.AddComponent<PlayerAnimationController>();
        animController.Set(model, animator);
    }

    public void InitPlayer(PlayerData data)
    {
        PlayerData = data;

        SetPosition(data.currentPosX, data.currentPosY);

        SetDirection((UnitDirection)data.direction);

        //textMesh.text = data.userId.ToString();

        animController.SetState(PlayerState.IDLE);
    }

    public void MovePlayerNextPosition(PlayerData playerData = null)
    {
        PlayerData = playerData;
        nextTile = GameManager.Inst.GetTileInfo(PlayerData.currentPosX, PlayerData.currentPosY);
        MoveSetDirection(PlayerData.currentPosX, PlayerData.currentPosY);
    }

    private void Update()
    {
        PlayerMoving();
    }

    private void PlayerMoving()
    {
        if (nextTile == null)
        {
            animController.SetState(PlayerState.IDLE);
            return;
        }
        
        animController.SetState(PlayerState.WARK);
        
        transform.position = Vector3.MoveTowards(transform.position, nextTile.transform.position, PlayerData.MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextTile.transform.position) < 0.01f)
        {
            SetPosition(nextTile.GridPoint.X, nextTile.GridPoint.Y);
            
            OnArrivePoint?.Invoke(this);
            
            nextTile = null;
        }
    }

    protected override void ChangedDirection(UnitDirection dir)
    {
        animController.SetDirection(dir);
    }
}
