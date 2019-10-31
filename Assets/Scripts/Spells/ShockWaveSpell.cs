using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWaveSpell : SpellBase
{

    // audio
    public AudioClip cast_clip;
    public AudioSource wand_source;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float shockForce = 20.0f;
    public float radius = 10f;
    private const float waitTime = 5.0f;

    public override void UnleashSpell()
    {
        wand_source.volume = 0.25f;
        wand_source.Play();

        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;

        Vector3 pos = bulletEmitter.transform.position;


        Vector3 explosionPos = transform.position;

        Collider[] colliders = Physics.OverlapSphere(pos, radius);
        foreach (Collider hit in colliders)
        {

            if (hit.tag == "Actor")
            {
                var enemy = hit.GetComponent<EnemyAI>();
                enemy.isRagdolling = true;
            }


            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(120*shockForce, pos, radius, 3.0F, ForceMode.Impulse);
                Debug.Log(explosionPos);
                Debug.Log(rb);
            }
        }
        Destroy(tempBull, waitTime);
    }

    public override void OnAimStart()
    {
        //audio
        wand_source.clip = cast_clip;
    }

    public override void OnAimEnd()
    {
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            UnleashSpell();
        }
    }
}
