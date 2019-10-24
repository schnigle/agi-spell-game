using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class HelperAI : MonoBehaviour
{
    public enum EnemySpell { attack, teleport, shield }

    // EnemyAI baseAI;
    NavMeshAgent agent;
    public GameObject flagposts;


    Vector3 closestNavMeshRisePoint;
    GameObject player;
    List<GameObject> flags = new List<GameObject>();
    EnemyAnimator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<EnemyAnimator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Quick fix to find the player in a "pluggable" way
        player = GameObject.FindGameObjectWithTag("Player");

        foreach (Transform flag in flagposts.transform)
        {
            flags.Add(flag.gameObject);

        }
    }

    float playerFlagDist;
    public GameObject getFlagClosestToPlayer()
    {
        playerFlagDist = float.MaxValue;
        GameObject chosenFlag = null;
        foreach (var f in flags)
        {
            float flagDist = Vector3.Distance(player.transform.position, f.transform.position);
            if (flagDist < playerFlagDist)
            {
                chosenFlag = f;
                playerFlagDist = flagDist;
            }
        }
        return chosenFlag;
    }

    void Update()
    {

        if (player)
        {
            var flag = getFlagClosestToPlayer();
            float helperFlagDist = Vector3.Distance(transform.position, flag.transform.position);

            if (playerFlagDist < 10 && helperFlagDist > 1.4)
            {
                // agent.enabled = true;
                // agent.isStopped = false;
                MoveExactlyTowards(flag.transform.position);
            }
            else
            {
                // agent.enabled = false;
                // agent.isStopped = true;
                var q = Quaternion.LookRotation(player.transform.position - transform.position);
                var euler_q = q.eulerAngles;
                euler_q.x = 0;
                euler_q.z = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(euler_q), 500 * Time.deltaTime);
            }
            // Debug.Log("TEST: " + playerFlagDist + " _ " + flag.name);
        }
        // Animation properties
        if (animator)
        {
            animator.speed = agent.speed;
            animator.walking = agent.velocity.sqrMagnitude > 0.5f * 0.5f;
        }
    }

    public void MoveExactlyTowards(Vector3 target)
    {
        //Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        // Vector2 flatTargetPosition = new Vector2(target.x, target.z);
        // float distance = Mathf.Max(Vector2.Distance(flatPosition, flatTargetPosition), 10);

        target.y = transform.position.y;
        if(Vector3.Distance(player.transform.position,target) < 2)
        {
            target.z += 2;
            target.y += 1;
        }

        agent.destination = target;
    }
}
