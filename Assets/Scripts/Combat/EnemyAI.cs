using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    public NavMeshAgent Agent { get; private set; }

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// Experimental movement script. Causes the actor to move to a random point within a circle
    /// centered between the actor and the target, resulting less predictable movement.
    public void IndirectlyMoveTowards(Vector3 target)
    {
        float distance = Vector3.Distance(target, transform.position);
        Vector3 targetPoint = transform.position + (target - transform.position) / 2;
        float r = Random.Range(0, distance / 2);
        float theta = Random.Range(0, 2*Mathf.PI);
        targetPoint.x += r * Mathf.Cos(theta);
        targetPoint.z += r * Mathf.Sin(theta);
        Agent.destination = targetPoint;
    }

}
