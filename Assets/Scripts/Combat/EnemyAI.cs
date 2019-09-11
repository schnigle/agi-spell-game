using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    public NavMeshAgent agent { get; private set; }
    public Rigidbody body { get; private set; }
    /// True if the actor is currently trying to get up (and return to NavMesh) after being a ragdoll
    public bool isRising { get; private set; }
    Vector3 closestNavMeshRisePoint;
    private bool _isRagdolling;
    /// True if the actor is currently in "ragdoll" mode (which means that it acts as a non-kinematic rigidbody while having its movement disabled)
    public bool isRagdolling
    {
        get { return _isRagdolling; }
        set
        {
            if (value)
            {
                _isRagdolling = true;
                agent.enabled = false;
                body.isKinematic = false;
                isRising = false;
            }
            else
            {
                _isRagdolling = false;
                body.isKinematic = true;
                isRising = true;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 100, NavMesh.AllAreas))
                {
                    closestNavMeshRisePoint = hit.position;
                }
            }
        }
    }


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isRagdolling = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Before resuming NavMeshAgent, return to the default angle and get back to the NavMesh.
        if (isRising)
        {
            var angleRate = Time.deltaTime * 300;
            var rotX = Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.x, 0, angleRate);
            var rotZ = Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.z, 0, angleRate);
            transform.eulerAngles = new Vector3(rotX, transform.eulerAngles.y, rotZ);
            transform.position = Vector3.MoveTowards(transform.position, closestNavMeshRisePoint, Time.deltaTime * 15);
            if (transform.rotation.eulerAngles.x == 0 && transform.rotation.eulerAngles.z == 0 && Vector3.SqrMagnitude(transform.position - closestNavMeshRisePoint) == 0)
            {
                isRising = false;
                agent.enabled = true;
            }
        }
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
        agent.destination = targetPoint;
    }

}
