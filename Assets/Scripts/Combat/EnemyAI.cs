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
    GameObject player;
    [SerializeField]
    Projectile projectilePrefab;

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

    void Attack()
    {
        if (player && projectilePrefab)
        {
            var projStartPos = transform.position + Vector3.up*2;
            var velocity = 20;
            var distance = Vector3.Distance(projStartPos, player.transform.position);
            var g = Physics.gravity.y;
            var altitude = projStartPos.y-player.transform.position.y;
            var angle = Mathf.Atan((velocity*velocity - Mathf.Sqrt(Mathf.Pow(velocity, 4) - g*(g*distance*distance+2*altitude*velocity*velocity))) / (g*distance)) * Mathf.Rad2Deg;
            // Only fire if AI is within range
            if (!float.IsNaN(angle))
            {
                var proj = Instantiate(projectilePrefab, projStartPos, new Quaternion());
                proj.caster = gameObject;
                proj.transform.LookAt(player.transform);
                proj.transform.eulerAngles = new Vector3(angle,proj.transform.eulerAngles.y,proj.transform.eulerAngles.z);
                proj.RBody.velocity = proj.transform.forward * velocity;
            }
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        isRagdolling = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Quick fix to find the player in a "pluggable" way
        player = GameObject.FindGameObjectWithTag("Player");
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

        // Move towards player and attack them
        if (player)
        {
            if (agent.enabled && agent.remainingDistance < 0.1f)
            {
                // Attack();
                IndirectlyMoveTowards(player.transform.position);
            }
        }
    }

    /// Experimental movement script. Causes the actor to move to a random point on the border of a circle centered on the target.
    public void IndirectlyMoveTowards(Vector3 target)
    {
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 flatTargetPosition = new Vector2(target.x, target.z);
        float distance = Mathf.Max(Vector2.Distance(flatPosition, flatTargetPosition), 10);
        float r = Random.Range(distance * 0.8f, distance);
        var angle = Mathf.Atan2(transform.position.z - target.z, transform.position.x- target.x);
        var maxAngleDelta = Mathf.PI/4;
        float theta = Random.Range(angle - maxAngleDelta, angle + maxAngleDelta);
        target.x += r * Mathf.Cos(theta);
        target.z += r * Mathf.Sin(theta);
        agent.destination = target;
    }

}
