using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterAnimRandom : MonoBehaviour
{

    public string idleStateName = "Idle";
    public float minSpeed = 0.8f;
    public float maxSpeed = 1.2f;


    void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null) return;

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        animator.speed = randomSpeed;

        float randomStartTime = Random.Range(0f, 1f); 
        animator.Play(idleStateName, 0, randomStartTime);
    }

}
