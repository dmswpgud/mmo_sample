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
                    // TODO: 어택할때 연타하게되면 플레이어 유닛이 순간이동하는 현상이 있음. 원인은 어택을 하고 서버에서 상태를 받아올때 좌표를 받아서 셋팅하기 때문..
                    // 해결책은 버튼 연타를 막으면 됨..
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
    
    public void Attack()
    {
        myPlayer.SetDirectionByPosition(TargetUnit.X, TargetUnit.Y);
        myPlayer.SetPlayerAnim(PlayerState.ATTACK);
        CNetworkManager.Inst.RequestPlayerState(myPlayer.STATE, receiverUserId: TargetUnit?.ID ?? 0,
            OnReceivedChangedPlayerState);
        myPlayer.OnArrivePoint = null;
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
        // 목적지 한칸 도착하면 날아오는 콜백
        myPlayer.OnArrivePoint = (p) =>
        {
            if (path.Count > 0) // 패스가 0 이상이면 0인덱스 지워줌
            {
                path.RemoveAt(0);
            }

            if (path.Count <= 0) // 패스가 0이면 리턴
                return;
            
            if (TargetUnit) // 타겟이 있다면
            {
                // 타겟이 한칸내에 있다면 공격 하고 이 콜백을 끝냄.
                if (GetDistance(myPlayer.X, myPlayer.Y, TargetUnit.X, TargetUnit.Y) <= 1)
                {
                    Attack();
                    AutoAttack();
                    myPlayer.OnArrivePoint = null;
                    return;
                }
            }
            // 상태를 워크로 변경
            myPlayer.STATE.state = (byte) PlayerState.WARK;
            // 방향 설정.
            myPlayer.SetDirectionByPosition(path[0].X, path[0].Y);
            // 서버에 이동할 경로를 보냄. (경로가 없을때까지 무한반복.)
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
