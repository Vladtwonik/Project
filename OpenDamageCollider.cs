using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDamageCollider : StateMachineBehaviour
{
    StateManager states;
    public HandleDamageColliders.DamageType damageType;
    public HandleDamageColliders.DCtype DCtype;
    public float delay;

    // OnStateEnter вызывается в тот момент, когда начинается движение и state machine анализирует состояние
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (states == null)
            states = animator.transform.GetComponentInParent<StateManager>();

        states.handleDC.OpenCollider(DCtype, delay, damageType);
    }

    // OnStateExit вызывается в тот момент, когда заканчивается анимация
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (states == null)
        {
            states = animator.transform.GetComponentInParent<StateManager>();
        }

        states.handleDC.CloseColliders();
    }

}
