using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToPlayer : StateMachineBehaviour
{
    // Cuando se activa la animación, la velocidad del enemigo es 0
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<NavMeshAgent>().speed = 0f;
    }
    // Cuando sale de la animación, aplica el valor 2 a la velocidad del enemigo
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<NavMeshAgent>().speed = 2f;
    }

}
