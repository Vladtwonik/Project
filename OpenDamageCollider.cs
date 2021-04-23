using UnityEngine;
using System.Collections;

public class OpenDamageCollider : StateMachineBehaviour {

    StateManager states;
    public HandleDamageColliders.DamageType damageType;
    public HandleDamageColliders.DCtype dcType;
    public float delay;

	// OnStateEnter вызывается в тот момент, когда начинается анимация
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    {
        if (states == null)
            states = animator.transform.GetComponentInParent<StateManager>();

        states.handleDC.OpenCollider(dcType, delay, damageType);
	}

	// OnStateExit вызывается в тот момент, когда заканчивается анимация
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	    if (states == null)
            states = animator.transform.GetComponentInParent<StateManager>();

        states.handleDC.CloseColliders();
	}

}
