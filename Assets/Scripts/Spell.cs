using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    private float timer = 0.0f;
    private const float waitTime = 10.0f;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;

    public float forwardForce = 5.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            GameObject tempBull;
            tempBull = Instantiate(bullet, bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
            //tempBull.transform.Rotate(Vector3.left * 90);
            Rigidbody tempBody;
            tempBody = tempBull.GetComponent<Rigidbody>();
            tempBody.AddForce(transform.forward * forwardForce);
            Destroy(tempBull, waitTime);
        }

        timer += Time.deltaTime;
    }

}
