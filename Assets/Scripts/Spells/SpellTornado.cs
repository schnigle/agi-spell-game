using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTornado : SpellBase
{
    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float forwardForce = 30.0f;
    private const float waitTime = 15.0f;

    // audio
    public AudioClip cast_clip;
    public AudioClip projectile_clip;
    private AudioSource projectile_source;
    public AudioSource wand_source;

    [SerializeField]
    TrajectoryPreview trajectory;

    public override void UnleashSpell()
    {
        // cast audio 
        //wand_source.time = 0.0f;
        wand_source.volume = 0.15f;
        wand_source.Play();

        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        tempBody.AddForce(bulletEmitter.transform.forward * forwardForce);

        // projectile audio
        projectile_source = tempBull.AddComponent<AudioSource>();
        projectile_source.clip = projectile_clip;
        projectile_source.loop = true;
        projectile_source.spatialize = true;
        projectile_source.spatialBlend = 1.0f;
        projectile_source.volume = 1.0f;
        projectile_source.Play();

        Destroy(tempBull, waitTime);
    }

    public override void OnAimStart()
    {
        trajectory?.gameObject.SetActive(true);

        // audio
        wand_source.clip = cast_clip;
    }

    public override void OnAimEnd()
    {
        trajectory?.gameObject.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            UnleashSpell();
        }


    }
}
