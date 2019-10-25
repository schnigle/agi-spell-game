using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class AISpellColorSet
{
    public Color attack = Color.white;
    public Color teleport = Color.white;
    public Color shield = Color.white;
}

public class EnemyAI : MonoBehaviour
{
    public enum EnemySpell { attack, teleport, shield }

    public bool isHostile = true;
    // audio
    private AudioClip[] screams;

    public NavMeshAgent agent { get; private set; }
    public Rigidbody body { get; private set; }
    /// True if the actor is currently trying to get up (and return to NavMesh) after being a ragdoll
    public bool isRising { get; private set; }
    Vector3 closestNavMeshRisePoint;
    GameObject player;
    [SerializeField]
    Projectile projectilePrefab;
    const float riseVelocityLimit = 0.6f;
    const float riseAngularVelocityLimit = 5;
    const float minRagdollTime = 3f;
    float ragdollTime = 0;
    float castTimeRemaining = 0;
    [SerializeField]
    float spellPrepTime = 1;
    [SerializeField]
    float spellUnleashTime = 1;
    bool isCasting = false;
    float defaultSpeed;
    bool hasUnleashedSpell = false;
    [SerializeField]
    float projectileVelocity = 20;
    [SerializeField]
    TrailRenderer staffTrail = null;
    [SerializeField]
    Shield shieldPrefab;
    [SerializeField]
    GameObject teleportEffectPrefab;
    [SerializeField]
    StaffOrb staffOrb;
    [SerializeField]
    Color inactiveOrbColor = Color.black;
    [SerializeField]
    Color activeOrbColor = Color.cyan;
    [SerializeField]
    AISpellColorSet spellColorSet;

    EnemySpell preparedSpell;
    Vector3 currentTargetPosition;

    EnemyAnimator animator;

    const float projectileSpawnHeight = 1f;

    [SerializeField]
    float sightRange = 20;
    [SerializeField]
    LayerMask sightObstacleMask;

    float sightCheckTimer = 1;
    bool foundPlayer;

    Queue<float> recentVelocities = new Queue<float>();

    private bool _isRagdolling;
    /// True if the actor is currently in "ragdoll" mode (which means that it acts as a non-kinematic rigidbody while having its movement disabled)
    public bool isRagdolling
    {
        get { return _isRagdolling; }
        set
        {
            if (value)
            {
                ragdollTime = 0;
                _isRagdolling = true;
                agent.enabled = false;
                body.isKinematic = false;
                isRising = false;
                animator.PlayCastAnimation("idle", 1, 1);
                int rand_idx = Random.Range(0, screams.Length);
                this.gameObject.GetComponent<AudioSource>().clip = screams[rand_idx];
                this.gameObject.GetComponent<AudioSource>().Play();
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

    void StartPrepareAnimation(string animation)
    {
            castTimeRemaining = spellUnleashTime + spellPrepTime;
            isCasting = true;
            hasUnleashedSpell = false;
            animator.PlayCastAnimation(animation, spellPrepTime, spellUnleashTime);
    }

    void PrepareTeleport()
    {
        if (!isCasting && !isRagdolling && player != null)
        {
            preparedSpell = EnemySpell.teleport;

            Vector3 targetPosition = transform.position;
            float r = 10;
            float theta = Random.Range(0, Mathf.PI*2);
            targetPosition.x += r * Mathf.Cos(theta);
            targetPosition.z += r * Mathf.Sin(theta);
            // currentTargetPosition = targetPosition;
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 direction = targetPosition - origin;
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, Vector3.Distance(targetPosition, origin), sightObstacleMask))
            {
                currentTargetPosition = hit.point;
            }
            else
            {
                currentTargetPosition = targetPosition;
            }
            StartPrepareAnimation("spell line ud");
        }
    }

    void UnleashTeleport()
    {
        if (!isRagdolling)
        {
            staffOrb.mainColor = spellColorSet.teleport;
            if (teleportEffectPrefab)
			{
				var newObj = Instantiate(teleportEffectPrefab);
				newObj.transform.position = transform.position;
				Destroy(newObj, 5);
			}
            transform.position = currentTargetPosition;
            agent.destination = currentTargetPosition;
            if (teleportEffectPrefab)
			{
                NavMeshHit hit;
                if (NavMesh.SamplePosition(currentTargetPosition, out hit, 5, NavMesh.AllAreas))
                {
                    var newObj = Instantiate(teleportEffectPrefab);
                    newObj.transform.position = hit.position;
                    Destroy(newObj, 5);
                }
			}
        }
    }

    void PrepareShield()
    {
        if (!isCasting && !isRagdolling && player != null && shieldPrefab)
        {
            currentTargetPosition = player.transform.position;
            preparedSpell = EnemySpell.shield;
            StartPrepareAnimation("spell circle cw");
        }
    }

    void UnleashShield()
    {
        if (!isRagdolling && player != null && shieldPrefab)
        {
            staffOrb.mainColor = spellColorSet.shield;
            var shield = Instantiate(shieldPrefab);
            shield.transform.position = transform.position + transform.forward * 3 + Vector3.up * 1;
            shield.transform.rotation = transform.rotation;
            shield.SetTimer(5);
        }
    }

    void PrepareAttack()
    {
        currentTargetPosition = player.transform.position;
        if (!isCasting && !isRagdolling && player != null && projectilePrefab)
        {
            var projStartPos = transform.position + Vector3.up*projectileSpawnHeight;
            var currentFireAngle = GetAttackAngle();
            // Only fire if AI is within range
            if (!float.IsNaN(currentFireAngle))
            {
                preparedSpell = EnemySpell.attack;
                StartPrepareAnimation("spell fish r");
            }
        }
    }

    void UnleashAttack()
    {
        if (!isRagdolling && player != null && projectilePrefab)
        {
            staffOrb.mainColor = spellColorSet.attack;
            var projStartPos = transform.position + Vector3.up*2;
            var currentFireAngle = GetAttackAngle();
            // Only fire if AI is within range
            if (!float.IsNaN(currentFireAngle))
            {
                var proj = Instantiate(projectilePrefab, projStartPos, new Quaternion());
                proj.caster = gameObject;
                proj.transform.LookAt(player.transform);
                proj.transform.eulerAngles = new Vector3(currentFireAngle,proj.transform.eulerAngles.y,proj.transform.eulerAngles.z);
                proj.RBody.velocity = proj.transform.forward * projectileVelocity;
            }
        }
    }

    float GetAttackAngle()
    {
        var projStartPos = transform.position + Vector3.up*projectileSpawnHeight;
        var distance = Vector3.Distance(projStartPos, player.transform.position);
        var g = Physics.gravity.y;
        var altitude = projStartPos.y-player.transform.position.y;
        var currentFireAngle = Mathf.Atan((projectileVelocity*projectileVelocity - Mathf.Sqrt(Mathf.Pow(projectileVelocity, 4) - g*(g*distance*distance+2*altitude*projectileVelocity*projectileVelocity))) / (g*distance)) * Mathf.Rad2Deg;
        return currentFireAngle;
    }

    void Awake()
    {
        animator = GetComponent<EnemyAnimator>();
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        if (staffTrail == null)
        {
            staffTrail = transform.GetComponentInChildren<TrailRenderer>();
        }
        isRagdolling = false;
        defaultSpeed = agent.speed;
    }

    // Start is called before the first frame update
    void Start()
    {
        var others = Transform.FindObjectsOfType<EnemyAI>();
        foreach (var item in others)
        {
            if (item != this)
            {
                agent.destination = (transform.position + item.transform.position) / 2;
            }
        }
        screams = new AudioClip[]{
            Resources.Load<AudioClip>("audio/scream_1"),
            Resources.Load<AudioClip>("audio/scream_2"),
            Resources.Load<AudioClip>("audio/scream_3"),
            Resources.Load<AudioClip>("audio/scream_4"),
            Resources.Load<AudioClip>("audio/scream_5"),
            Resources.Load<AudioClip>("audio/scream_6"),
            Resources.Load<AudioClip>("audio/scream_7"),
            Resources.Load<AudioClip>("audio/scream_8"),
            Resources.Load<AudioClip>("audio/scream_9"),
            Resources.Load<AudioClip>("audio/scream_10"),
            Resources.Load<AudioClip>("audio/scream_11"),
            Resources.Load<AudioClip>("audio/scream_12")
        };
        
        staffOrb.mainColor = Color.black;
        // Quick fix to find the player in a "pluggable" way
        player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            currentTargetPosition = player.transform.position;
        }
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
        if (isRagdolling)
        {
            isCasting = false;
            ragdollTime += Time.deltaTime;
        }
        if (isRagdolling && ragdollTime > minRagdollTime && body.velocity.sqrMagnitude < riseVelocityLimit * riseVelocityLimit && body.angularVelocity.sqrMagnitude < riseAngularVelocityLimit * riseAngularVelocityLimit)
        {
            isRagdolling = false;
        }

        // Animation properties
        if (animator)
        {
            animator.ragdolling = isRagdolling;
            animator.speed = agent.speed;
            animator.walking = agent.velocity.sqrMagnitude > 0.5f * 0.5f;
        }

        // Move towards player and attack them
        if (player && isHostile)
        {
            if (!foundPlayer)
            {
                sightCheckTimer -= Time.deltaTime;
                if (sightCheckTimer <= 0)
                {
                    sightCheckTimer = Random.Range(0.8f, 1.2f);
                    bool sightCheck = CanSeePlayer();
                    if (sightCheck)
                    {
                        foundPlayer = true;
                    }
                }
            }
            if (foundPlayer && agent.enabled && agent.remainingDistance < 0.1f)
            {
                if (!isCasting && CanSeePlayer())
                {
                    int rand = Random.Range(0, 3);
                    if (rand == 0)
                    {
                        PrepareAttack();
                    }
                    else if (rand == 1)
                    {
                        PrepareShield();
                    }
                    else if (rand == 2)
                    {
                        PrepareTeleport();
                    }
                }
                IndirectlyMoveTowards(player.transform.position);
            }
        }

        if (isCasting)
        {
            agent.velocity = Vector3.zero;
            if (player)
            {
                // rotate towards player
                var q = Quaternion.LookRotation(player.transform.position - transform.position);
                var euler_q = q.eulerAngles;
                euler_q.x = 0;
                euler_q.z = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(euler_q), 500 * Time.deltaTime);
            }
            castTimeRemaining -= Time.deltaTime;
            if (castTimeRemaining < spellUnleashTime && !hasUnleashedSpell)
            {
                hasUnleashedSpell = true;
                switch (preparedSpell)
                {
                    case EnemySpell.attack:
                        UnleashAttack();
                        break;
                    case EnemySpell.shield:
                        UnleashShield();
                        break;
                    case EnemySpell.teleport:
                        UnleashTeleport();
                        break;
                    default:
                        break;
                }
            }
            if (castTimeRemaining <= 0)
            {
                isCasting = false;
            }

        }
        else
        {
            agent.speed = defaultSpeed;
            staffOrb.mainColor = inactiveOrbColor;
        }
        CheckMoveProgress();
        UpdateStaffLine();
    }

    void CheckMoveProgress()
    {
        if (!isCasting && !isRagdolling && agent.enabled)
        {
            recentVelocities.Enqueue(agent.velocity.sqrMagnitude * Time.deltaTime);
            if (recentVelocities.Count > 10)
            {
                recentVelocities.Dequeue();
                float velocitySum = 0;
                foreach (var item in recentVelocities)
                {
                    velocitySum += item / recentVelocities.Count;
                }
                if (velocitySum < 0.02f)
                {
                    print("move it");
                    agent.destination = transform.position;
                    recentVelocities.Clear();
                }
            }
        }
        else
        {
            recentVelocities.Clear();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = player.transform.position - origin;
        float distance = Mathf.Min(Vector3.Distance(origin, player.transform.position), sightRange);
        direction.Normalize();
        return !Physics.Raycast(origin, direction, distance, sightObstacleMask);
    }

    void UpdateStaffLine()
    {
        if (staffTrail != null)
        {
            var preparationProgress = 1 - (castTimeRemaining - spellUnleashTime) / spellPrepTime;
            float lowerPrepLimit = 2f/6;
            float upperPrepLimit = 5f/6;
            if (isCasting && preparationProgress > lowerPrepLimit && preparationProgress < upperPrepLimit && preparationProgress < 1)
            {
                staffTrail.enabled = true;
                staffOrb.mainColor = activeOrbColor;
            }
            else
            {
                staffTrail.enabled = false;
                staffTrail.Clear();
            }
        }
    }

    /// Experimental movement script. Causes the actor to move to a random point on the border of a circle centered on the target.
    public void IndirectlyMoveTowards(Vector3 target)
    {
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 flatTargetPosition = new Vector2(target.x, target.z);
        float distance = Mathf.Max(Vector2.Distance(flatPosition, flatTargetPosition), 10);
        float r = Random.Range(distance * 0.6f, distance);
        var angle = Mathf.Atan2(transform.position.z - target.z, transform.position.x- target.x);
        var maxAngleDelta = Mathf.PI/16;
        float theta = Random.Range(angle - maxAngleDelta, angle + maxAngleDelta);
        target.x += r * Mathf.Cos(theta);
        target.z += r * Mathf.Sin(theta);
        agent.destination = target;
    }

}
