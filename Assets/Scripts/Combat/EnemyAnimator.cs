using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    Animator animator;
    public bool ragdolling;
    public float speed;
    public bool walking;
    // Start is called before the first frame update
    void Start()
    {
        animator = transform.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("walking", walking);
        animator.SetBool("ragdolling", ragdolling);
        animator.SetFloat("walk speed", speed);
    }

    public void PlayCastAnimation(string animation, float prepTime, float unleashTime)
    {
        animator.SetFloat("prep speed", 1/prepTime);
        animator.SetFloat("cast speed", 1/unleashTime);
        animator.CrossFadeInFixedTime(animation, 0.1f);
    }
}
