using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class OrbSpell : MonoBehaviour, ISpell
{
    [SerializeField]
    GestureRecognition.Gesture gesture;
	public GestureRecognition.Gesture SpellGesture => gesture;
    [SerializeField]
    Color color = Color.white;
	public Color OrbColor => color;


	public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float forwardForce = 250.0f;
    private const float waitTime = 10.0f;

    [SerializeField]
    TrajectoryPreview trajectory;

    public void UnleashSpell()
    {
        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        var scrip = tempBull.GetComponent<SpellLogicOrb>();
        tempBody.AddForce(bulletEmitter.transform.forward * forwardForce);
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
        scrip.targetMag = Mathf.Min(targetMag*2.5f, 10.0f);
        Destroy(tempBull, waitTime);

        scrip.setMaxTime(waitTime);
        scrip.setStartTime(Time.time * 1000.0f);
    }

    public void OnAimStart()
    {
        trajectory?.gameObject.SetActive(true);
    }

    public void OnAimEnd()
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
        var orbs =  GameObject.FindGameObjectsWithTag("blackorb");

        foreach(var orbA in orbs)
        {
            var scriptA = orbA.GetComponent<SpellLogicOrb>();
            if(scriptA != null && !scriptA.marked)
                foreach (var orbB in orbs)
                {
                    var scriptB = orbB.GetComponent<SpellLogicOrb>();
                    if (scriptB != null && !scriptB.marked && orbA.gameObject != orbB.gameObject)
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
