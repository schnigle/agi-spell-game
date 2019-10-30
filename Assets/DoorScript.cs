using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    public Transform playerTrans;

    void Start()
    {
        
    }

    readonly int maxCount = 3;
    int counter = 0;


    void Update()
    {

        counter = (counter + 1) % maxCount;

        if(counter == 1)
        {
            float playDist = Vector3.Distance(transform.position, playerTrans.position);
            if (playDist < 3.0f)
            {
                SceneManager.LoadScene("TerrainSandbox", LoadSceneMode.Single);
            }
        }
        
    }

}
