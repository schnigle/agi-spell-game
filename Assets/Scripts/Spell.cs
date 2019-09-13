using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    private float timer = 0.0f;
    private const float waitTime = 10.0f;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;

    public float forwardForce = 15.0f;


    // Start is called before the first frame update
    void Start()
    {

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
        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        //tempBull.transform.Rotate(Vector3.left * 90);
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        // tempBody.transform.position = (bulletEmitter.transform.forward.normalized * 2+ tempBody.position);
        tempBody.AddForce(bulletEmitter.transform.forward * forwardForce);
        Destroy(tempBull, waitTime);
    }

}
