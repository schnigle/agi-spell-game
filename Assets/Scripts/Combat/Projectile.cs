using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    float lifeTime = 100;
    float lifeTimeRemaining;
    public GameObject caster;
    public Rigidbody RBody { get; private set; }
    [SerializeField]
    GameObject impactEffect;
    [SerializeField]
    AudioClip impactClip;
    [SerializeField]
    LayerMask shieldMask;

    void Awake()
    {
        RBody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lifeTimeRemaining = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimeRemaining -= Time.deltaTime;
        if (lifeTimeRemaining <= 0)
        {
            Destroy(gameObject);
        }
    }

    // void OnCollisionEnter(Collision other)
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != caster)
        {
            // TODO: damage player, knock back enemies, VFX etc
            if (other.tag == "Actor")
            {
                print("projectile hit enemy");
            }
            else if (other.tag == "Player")
            {
                if (CameraPostProcessing.instance != null)
                {
                    CameraPostProcessing.instance.TriggerHit();
                }
                print("projectile hit player");
            }
            else
            {
                print("projectile hit ground");
            }
            if (impactEffect)
            {
                var effect = Instantiate(impactEffect);
                effect.transform.position = transform.position;
                var particleSystem = effect.GetComponent<ParticleSystem>();
                if(particleSystem != null)
                {
                    Destroy(particleSystem, particleSystem.main.duration);
                }
            }
            if (impactClip)
            {
                AudioSource.PlayClipAtPoint(impactClip, transform.position, 0.2f);
            }
            var pos = transform.position;
            var nearby = Physics.OverlapSphere(pos, 3);
            foreach(var item in nearby)
            {
                Rigidbody rigidbody = null;
                if(item.tag == "Actor")
                {
                    if (!Physics.Raycast(transform.position, Vector3.Normalize(item.bounds.center - transform.position), Vector3.Distance(transform.position, item.bounds.center), shieldMask))
                    {
                        var enemy = item.GetComponent<EnemyAI>();
                        enemy.isRagdolling = true;
                        enemy.Health -= 40;
                    }
                }
                rigidbody = item.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    var direction = (item.bounds.center - pos).normalized;
                    rigidbody.AddForce((5 - Vector3.Distance(item.bounds.center, pos)) * (direction) * 100, ForceMode.Impulse);
                }
            }
            Destroy(gameObject);
        }

    }
}
