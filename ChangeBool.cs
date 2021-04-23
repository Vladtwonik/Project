using UnityEngine;
using System.Collections;

public class ChangeBool : StateMachineBehaviour {

    public string boolName;
    public bool status;
    public bool resetOnExit;

	// OnStateEnter вызывается в тот момент, когда начинается анимация
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        animator.SetBool(boolName, status);
	}

	// OnStateExit вызывается в тот момент, когда заканчивается анимация
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        
        if(resetOnExit)
            animator.SetBool(boolName, !status);
	}
}
