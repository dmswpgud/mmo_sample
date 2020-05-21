using System;
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
            case PlayerState.PICKED_ITEM:
                animator.Play("ShieldWarrior@Block01");
                break;
            case PlayerState.ATTACK:
                animator.Play("ShieldWarrior@Attack01");
                return;
            case PlayerState.DAMAGE:
                animator.Play("ShieldWarrior@Damage01");
                break;
            case PlayerState.DEATH:
                animator.Play("ShieldWarrior@Death01");
                break;
        }
        OnFinishedAnim = null;
    }
}
