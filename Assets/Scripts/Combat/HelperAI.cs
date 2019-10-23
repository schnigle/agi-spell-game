using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class HelperAI : MonoBehaviour
{
    public enum EnemySpell { attack, teleport, shield }

    public NavMeshAgent agent { get; private set; }
    public Rigidbody body { get; private set; }

    public GameObject flagposts;


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

    const float projectileSpawnHeight = 1f;

    List<GameObject> flags = new List<GameObject>();

    void StartPrepareAnimation(string animation)
    {
        castTimeRemaining = spellUnleashTime + spellPrepTime;
        isCasting = true;
        hasUnleashedSpell = false;
        GetComponent<EnemyAnimator>().PlayCastAnimation(animation, spellPrepTime, spellUnleashTime);
    }

    void PrepareTeleport()
    {
        if (!isCasting && player != null)
        {
            preparedSpell = EnemySpell.teleport;

            Vector3 targetPosition = transform.position;
            float r = 10;
            float theta = Random.Range(0, Mathf.PI * 2);
            targetPosition.x += r * Mathf.Cos(theta);
            targetPosition.z += r * Mathf.Sin(theta);
            currentTargetPosition = targetPosition;
            StartPrepareAnimation("spell line ud");
        }
    }

    void UnleashTeleport()
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


    void PrepareAttack()
    {
        currentTargetPosition = player.transform.position;
        if (!isCasting && player != null && projectilePrefab)
        {
            var projStartPos = transform.position + Vector3.up * projectileSpawnHeight;
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
        if (player != null && projectilePrefab)
        {
            staffOrb.mainColor = spellColorSet.attack;
            var projStartPos = transform.position + Vector3.up * 2;
            var currentFireAngle = GetAttackAngle();
            // Only fire if AI is within range
            if (!float.IsNaN(currentFireAngle))
            {
                var proj = Instantiate(projectilePrefab, projStartPos, new Quaternion());
                proj.caster = gameObject;
                proj.transform.LookAt(player.transform);
                proj.transform.eulerAngles = new Vector3(currentFireAngle, proj.transform.eulerAngles.y, proj.transform.eulerAngles.z);
                proj.RBody.velocity = proj.transform.forward * projectileVelocity;
            }
        }
    }

    float GetAttackAngle()
    {
        var projStartPos = transform.position + Vector3.up * projectileSpawnHeight;
        var distance = Vector3.Distance(projStartPos, player.transform.position);
        var g = Physics.gravity.y;
        var altitude = projStartPos.y - player.transform.position.y;
        var currentFireAngle = Mathf.Atan((projectileVelocity * projectileVelocity - Mathf.Sqrt(Mathf.Pow(projectileVelocity, 4) - g * (g * distance * distance + 2 * altitude * projectileVelocity * projectileVelocity))) / (g * distance)) * Mathf.Rad2Deg;
        return currentFireAngle;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        if (staffTrail == null)
        {
            staffTrail = transform.GetComponentInChildren<TrailRenderer>();
        }
        defaultSpeed = agent.speed;
    }

    // Start is called before the first frame update
    void Start()
    {
        staffOrb.mainColor = Color.black;
        // Quick fix to find the player in a "pluggable" way
        player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            currentTargetPosition = player.transform.position;
        }

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

            if (playerFlagDist < 5 && helperFlagDist > 1.4)
            {
                agent.enabled = true;
                MoveExactlyTowards(flag.transform.position);
            }
            else
            {
                agent.enabled = false;
                var q = Quaternion.LookRotation(player.transform.position - transform.position);
                var euler_q = q.eulerAngles;
                euler_q.x = 0;
                euler_q.z = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(euler_q), 500 * Time.deltaTime);
            }
            // Debug.Log("TEST: " + playerFlagDist + " _ " + flag.name);
        }

        if (isCasting)
        {
            agent.speed = 0;
            if (player)
            {
                // rotate towards player
                var q = Quaternion.LookRotation(currentTargetPosition - transform.position);
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

        UpdateStaffLine();
    }

    void UpdateStaffLine()
    {
        if (staffTrail != null)
        {
            var preparationProgress = 1 - (castTimeRemaining - spellUnleashTime) / spellPrepTime;
            float lowerPrepLimit = 2f / 6;
            float upperPrepLimit = 5f / 6;
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

    /// Experimental movement script. Causes the actor to move to a random point on the border of a circle centered on the target.
    public void IndirectlyMoveTowards(Vector3 target)
    {
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 flatTargetPosition = new Vector2(target.x, target.z);
        float distance = Mathf.Max(Vector2.Distance(flatPosition, flatTargetPosition), 10);
        float r = Random.Range(distance * 0.6f, distance);
        var angle = Mathf.Atan2(transform.position.z - target.z, transform.position.x - target.x);
        var maxAngleDelta = Mathf.PI / 16;
        float theta = Random.Range(angle - maxAngleDelta, angle + maxAngleDelta);
        target.x += r * Mathf.Cos(theta);
        target.z += r * Mathf.Sin(theta);
        agent.destination = target;
    }
}
