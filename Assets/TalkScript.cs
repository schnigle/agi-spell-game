using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkScript : MonoBehaviour
{
    public GameObject playerObj;
    public GameObject textObj;

    void Start()
    {
   
    }

    void Update()
    {
        Vector3 v = playerObj.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(playerObj.transform.position - v);
        transform.Rotate(0, 180, 0);

        float playDist = Vector3.Distance(playerObj.transform.position, transform.position);
        if(playDist > 5)
        {
            textObj.gameObject.SetActive(false);
        }
        else
        {
            textObj.gameObject.SetActive(true);
        }
    }


}
