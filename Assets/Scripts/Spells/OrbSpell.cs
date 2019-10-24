using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class OrbSpell : SpellBase
{
	public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float forwardForce = 250.0f;
    private const float waitTime = 10.0f;

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
        wand_source.time = 0.0f;
        wand_source.volume = 0.15f;
        wand_source.Play();

        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        var scrip = tempBull.GetComponent<SpellLogicOrb>();
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

        scrip.setMaxTime(waitTime);
        scrip.setStartTime(Time.time * 1000.0f);
    }

    public void RedoSpell(Vector3 emitPos, float targetMag)
    {
        GameObject tempBull;
        tempBull = Instantiate(bullet, emitPos, playerTrans.rotation) as GameObject;
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        var scrip = tempBull.GetComponent<SpellLogicOrb>();
        scrip.targetMag = Mathf.Min(targetMag * 2.5f, 10.0f);
        Destroy(tempBull, waitTime);

        scrip.setMaxTime(waitTime);
        scrip.setStartTime(Time.time * 1000.0f);
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
        if (Input.GetKeyDown("v"))
        {
            UnleashSpell();
        }
        var orbs = GameObject.FindGameObjectsWithTag("blackorb");

        foreach (var orbA in orbs)
        {
            var scriptA = orbA.GetComponent<SpellLogicOrb>();
            if (scriptA != null && !scriptA.marked && !scriptA.endStarted())
                foreach (var orbB in orbs)
                {
                    var scriptB = orbB.GetComponent<SpellLogicOrb>();
                    if (scriptB != null && !scriptB.marked && orbA.gameObject != orbB.gameObject && !scriptB.endStarted())
                    {
                        var trA = orbA.GetComponent<Transform>();
                        var trB = orbB.GetComponent<Transform>();
                        if (Vector3.Distance(trA.position, trB.position) < 2.0f)
                        {
                            scriptA.marked = true;
                            scriptB.marked = true;
                            scriptA.dieAnim = true;
                            scriptB.dieAnim = true;
                            RedoSpell(orbA.transform.position, Mathf.Max(scriptA.targetMag, scriptB.targetMag));
                        }

                    }
                }
        }
    }
}
