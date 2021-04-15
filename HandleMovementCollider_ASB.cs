using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HandleMovementCollider_ASB : StateMachineBehaviour
{
    StateManager states;

    public int index;

    // OnStateEnter вызывается в тот момент, когда начинается движение и state machine анализирует состояние
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (states == null)
            states = animator.transform.GetComponentInParent<StateManager>();

        states.CloseMovementCollider(index);
    }

    // OnStateUpdate вызывается каждый кадр метода Update между OnStateEnter и OnStateExit (?)
    // override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //
    // }

    // OnStateExit вызывается в тот момент, когда заканчивается анимация
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (states == null)
        {
            states = animator.transform.GetComponentInParent<StateManager>();
        }

        states.OpenMovementCollider(index);
    }

    // OnStateMove вызывается после Animator.OnAnimatorMove(). 
    //Код, который вызывает и влияет на процесс поворота должен быть реализован здесь
    // override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //
    // }

    // OnStateIX вызывается после Animator.OnAnimatorIX(). 
    //Данная часть кода, которая устанавливает анимацию IX (инверсную кинематику), должна быть здесь  
    // override public void OnStateIX(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //
    // }
}
