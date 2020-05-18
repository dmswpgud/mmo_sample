using Client.Game.Map;
using DG.Tweening;
using GameServer;
using UnityEngine;

public partial class GameManager
{
    private bool StopAutoAttack = false;
    
    private void UpdateController()
    {
        if (!myPlayer) return;

        if (myPlayer.IsDead) return;

        if (InputKey.InputMove)
        {
            if (TargetUnit)
            {
                if (GetDistance(myPlayer.X, myPlayer.Y, TargetUnit.X, TargetUnit.Y) <= 1)
                {
                    Attack();
                    
                    AutoAttack();
                    return;
                }
            }
            
            var targetTile = GetClickedObject();

            if (targetTile != null && targetTile.isBlock == false)
            {
                targetTile.GetComponent<MeshRenderer>().material.color = Color.green;
                PlayerMove(targetTile.GridPoint);
            }
        }

        if (InputKey.InputCancle)
        {
            StopAutoAttack = true;
        }
    }
    
    public void AutoAttack()
    {
        StopAutoAttack = false;
        
        DOVirtual.DelayedCall(1.5f, () =>
        {
            if (StopAutoAttack == true)
                return;
            
            Attack();
            
            AutoAttack();
        });
    }

    public void Attack()
    {
        myPlayer.SetDirectionByPosition(TargetUnit.X, TargetUnit.Y);
        myPlayer.SetPlayerAnim(PlayerState.ATTACK);
        CNetworkManager.Inst.RequestPlayerState(myPlayer.STATE, receiverUserId: TargetUnit?.ID ?? 0,
            OnReceivedChangedPlayerState);
        myPlayer.OnArrivePoint = null;
    }
    
        private void PlayerMove(GridPoint point)
    {
        DrawWall();
        // 경로탐색 시작.
        var start = new GridPoint(myPlayer.transform.position.x, myPlayer.transform.position.z);
        // 경로탐색 도착.
        var end = point;

        var pathFinder = new PathFinder();
        // A* 로 경로 가져오기.
        path = pathFinder.FindPath(tileInfos, start, end);
        // 경로가 없으면 리턴.
        if (path.Count <= 0)
            return;
        // 일단 상태를 워크로 바꿈
        myPlayer.STATE.state = (byte) PlayerState.WARK;
        // 방향 설정.
        myPlayer.SetDirectionByPosition(path[0].X, path[0].Y);
        // 서버에 이동할 경로를 보냄.
        CNetworkManager.Inst.RequestPlayerMove(path[0].X, path[0].Y, myPlayer.STATE.direction, ResponseMovePlayer);
        // 
        myPlayer.OnArrivePoint = (p) =>
        {
            if (TargetUnit)
            {
                start = new GridPoint(myPlayer.transform.position.x, myPlayer.transform.position.z);
                // 경로탐색 도착.
                end = new GridPoint(TargetUnit.X, TargetUnit.Y);

                pathFinder = new PathFinder();
            }
            
            if (path.Count > 0)
            {
                path.RemoveAt(0);
            }

            if (path.Count <= 0)
                return;
            
            if (TargetUnit)
            {
                if (GetDistance(myPlayer.X, myPlayer.Y, TargetUnit.X, TargetUnit.Y) <= 1)
                {
                    Attack();
                    AutoAttack();
                    myPlayer.OnArrivePoint = null;
                    return;
                }
            }
            
            myPlayer.STATE.state = (byte) PlayerState.WARK;
            // 방향 설정.
            myPlayer.SetDirectionByPosition(path[0].X, path[0].Y);
            // 서버에 이동할 경로를 보냄.
            CNetworkManager.Inst.RequestPlayerMove(path[0].X, path[0].Y, myPlayer.STATE.direction,
                (res, error) =>
                {
                    var data = (PlayerStateData) res;

                    var resPlayer = listUnit.Find(resp => p.DATA.playerId == data.playerId);

                    if (resPlayer?.STATE.posX == data.posX && resPlayer?.STATE.posY == data.posY)
                    {
                        return;
                    }

                    resPlayer?.MovePlayerNextPosition(data);
                });
        };
        
        DrawTile(path);
    }
}
