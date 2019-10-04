using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    Animator animator;
    EnemyAI actor;
    // Start is called before the first frame update
    void Start()
    {
        animator = transform.GetComponentInChildren<Animator>();
        actor = GetComponent<EnemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("walking", actor.agent.velocity.magnitude > 0.5f);
        animator.SetBool("ragdolling", actor.isRagdolling);
        animator.SetFloat("walk speed", actor.agent.speed);
    }

    public void PlayCastAnimation(string animation, float prepTime, float unleashTime)
    {
        animator.SetFloat("prep speed", 1/prepTime);
        animator.SetFloat("cast speed", 1/unleashTime);
        animator.CrossFadeInFixedTime(animation, 0.1f);
    }
}
