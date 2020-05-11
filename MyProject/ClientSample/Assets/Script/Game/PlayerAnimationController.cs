using System;
using System.Collections;
using System.Collections.Generic;
using GameServer;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private GameObject render;
    private Animator animator;
    public Action<PlayerState> OnFinishedAnim;
    
    public void Set(GameObject mode, Animator anim)
    {
        render = mode;
        animator = anim;
    }

    public void OnFinishedAttackAnim()
    {
        OnFinishedAnim?.Invoke(PlayerState.ATTACK);
        OnFinishedAnim = null;
    }

    public void SetState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                animator.Play("ShieldWarrior@Idle01");
                break;
            case PlayerState.WARK:
                animator.Play("ShieldWarrior@Walk01");
                break;
            case PlayerState.ATTACK:
                animator.Play("ShieldWarrior@Attack01");
                break;
            case PlayerState.DAMAGE:
                animator.Play("ShieldWarrior@Damage01");
                break;
            case PlayerState.DEATH:
                animator.Play("ShieldWarrior@Death01");
                break;
        }
        
        OnFinishedAnim?.Invoke(state);
        OnFinishedAnim = null;
    }
    
    public void SetDirection(UnitDirection dir)
    {
        switch (dir)
        {
            case UnitDirection.UP:
                render.transform.rotation = Quaternion.identity;
                break;
            case UnitDirection.DOWN:
                render.transform.rotation = Quaternion.Euler(0, 180f, 0);
                break;
            case UnitDirection.LEFT:
                render.transform.rotation = Quaternion.Euler(0, -90f, 0);
                break;
            case UnitDirection.RIGHT:
                render.transform.rotation = Quaternion.Euler(0, 90f, 0);
                break;
            case UnitDirection.UP_LEFT:
                render.transform.rotation = Quaternion.Euler(0, -45f, 0);
                break;
            case UnitDirection.UP_RIGHT:
                render.transform.rotation = Quaternion.Euler(0, 45f, 0);
                break;
            case UnitDirection.DOWN_LEFT:
                render.transform.rotation = Quaternion.Euler(0, 240, 0);
                break;
            case UnitDirection.DOWN_RIGHT:
                render.transform.rotation = Quaternion.Euler(0, 120, 0);
                break;
        }
    }
}
