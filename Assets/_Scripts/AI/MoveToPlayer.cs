using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToPlayer : StateMachineBehaviour
{
    // Cuando se activa la animaci�n, la velocidad del enemigo es 0
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<NavMeshAgent>().speed = 0f;
        animator.gameObject.GetComponent<SphereCollider>().enabled = false;
        animator.SetLayerWeight(1, 0f);
    }
    // Cuando sale de la animaci�n, aplica el valor 2 a la velocidad del enemigo
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<ZM_AI>().ZM_Aggro(animator.gameObject.GetComponent<ZM_AI>().zm_aggro_level);
        animator.gameObject.GetComponent<SphereCollider>().enabled = true;
        animator.SetLayerWeight(1, 1f);

    }

}
