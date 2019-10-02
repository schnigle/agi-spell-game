using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour, ISpell
{
    [SerializeField]
    GestureRecognition.Gesture gesture;
	public GestureRecognition.Gesture SpellGesture => gesture;
    [SerializeField]
    Color color = Color.white;
	public Color OrbColor => color;

    // audio
    public AudioClip summon_clip;
    public AudioClip hold_clip;
    public AudioClip cast_clip;
    public AudioSource projectile_source;
    public AudioSource wand_source;

    private float timer = 0.0f;
    private const float waitTime = 10.0f;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;

    public float forwardForce = 15.0f;

    [SerializeField]
    TrajectoryPreview trajectory;

	// Start is called before the first frame update
	void Start()
    {
        //wand_source.clip = cast_clip;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            UnleashSpell();
        }

        timer += Time.deltaTime;
    }

    public void UnleashSpell()
    {
        //wand_source.time = 0.3f;
        //wand_source.Play();
        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        //tempBull.transform.Rotate(Vector3.left * 90);
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        // tempBody.transform.position = (bulletEmitter.transform.forward.normalized * 2+ tempBody.position);
        tempBody.AddForce(bulletEmitter.transform.forward * forwardForce);
        Destroy(tempBull, waitTime);
    }

	public void OnAimStart()
	{
		trajectory?.gameObject.SetActive(true);

    }

	public void OnAimEnd()
	{
		trajectory?.gameObject.SetActive(false);
	}
}
