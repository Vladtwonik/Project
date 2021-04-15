using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ChangeBool : StateMachineBehaviour
{
    public string boolName;
    public bool status;
    public bool resetOnExit;

    // OnStateEnter вызывается в тот момент, когда начинается анимация
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(boolName, status);
    }

    // OnStateUpdate вызывается каждый кадр метода Update между OnStateEnter и OnStateExit 
    // override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //
    // }

    // OnStateExit вызывается в тот момент, когда заканчивается анимация
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(resetOnExit)
        {
            animator.SetBool(boolName, !status);
        }
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

