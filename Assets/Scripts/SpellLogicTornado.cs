using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogicTornado : MonoBehaviour
{
    public GameObject muzzlePrefab, hitPrefab;
    public int force = 100;
    bool hit = false;
    private GameObject latesthitObject;

    private Vector3 playerPos;


    void Start()
    {
  
        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward;

            var psMuzzle = muzzleVFX.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
            {
                Destroy(muzzleVFX, psMuzzle.main.duration);
            }
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }

        }

        playerPos = GameObject.Find("PlayerObject").GetComponent<Transform>().position;
        var colliderToIgnore = GameObject.Find("PlayerObject").GetComponent<Collider>(); // traversing hierarchy like a tree
        Physics.IgnoreCollision(colliderToIgnore, GetComponent<Collider>());

    }

    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    void Update()
    {
        gameObject.transform.eulerAngles = new Vector3(0, 0, 0);

        Vector3 dir = (gameObject.transform.position - playerPos);
        dir.y = 0.1f;
        dir.Normalize();

        var rigidBod = gameObject.GetComponent<Rigidbody>();
        rigidBod.AddRelativeForce(dir * 10);

        Vector3 pos = gameObject.transform.position;

        /*var nearby = Physics.OverlapSphere(pos, 25);
        foreach (var item in nearby)
        {
            Rigidbody rigidbody = null;
            if (item.tag == "Actor")
            {
                var enemy = item.GetComponent<EnemyAI>();
                enemy.isRagdolling = true;
            }
            rigidbody = item.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                var direction = (rigidbody.transform.position - pos).normalized;
                rigidbody.AddForce((5 - Vector3.Distance(rigidbody.position, pos)) * (direction) * 200, ForceMode.Impulse);
            }
        }*/


    }

        void OnCollisionEnter(Collision col)
    {

        ContactPoint contact = col.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

    }
}
