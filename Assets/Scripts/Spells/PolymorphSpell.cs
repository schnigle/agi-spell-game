using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolymorphSpell : SpellBase
{
    // audio
    public AudioClip cast_clip;
    private AudioSource projectile_source;
    public AudioSource wand_source;

    private float timer = 0.0f;
    private const float waitTime = 10.0f;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;

    public float forwardForce = 10.0f;

    [SerializeField]
    TrajectoryPreview trajectory;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("h"))
        {
            UnleashSpell();
        }

        timer += Time.deltaTime;
    }

    public override void UnleashSpell()
    {
        // cast audio  
        wand_source.time = 0.0f;
        wand_source.volume = 0.15f;
        wand_source.Play();

        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        //tempBull.transform.Rotate(Vector3.left * 90);
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        // tempBody.transform.position = (bulletEmitter.transform.forward.normalized * 2+ tempBody.position);
        tempBody.AddForce(bulletEmitter.transform.forward * forwardForce);
        Destroy(tempBull, waitTime);
    }

    public override void OnAimStart()
    {
        //audio
        wand_source.clip = cast_clip;

        trajectory?.gameObject.SetActive(true);
    }

    public override void OnAimEnd()
    {
        trajectory?.gameObject.SetActive(false);
    }
}
