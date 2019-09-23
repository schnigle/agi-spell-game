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
        Vector3 dir = (gameObject.transform.position - playerPos).normalized;
        dir.y = 0.01f;
        dir.Normalize();
        gameObject.GetComponent<Rigidbody>().AddForce(dir * 8);

    }

        void OnCollisionEnter(Collision col)
    {

        ContactPoint contact = col.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

    }
}
