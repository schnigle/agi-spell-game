using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    public Transform playerTrans;
    public Animator animator;

    void Start()
    {
        
    }

    readonly int maxCount = 3;
    int counter = 0;

    string chosenlvl = null;

    public void fadeToLevel(string lvlname)
    {
        chosenlvl = lvlname;
        animator.SetTrigger("FadeOut");
    }


    void Update()
    {

        counter = (counter + 1) % maxCount;

        if(counter == 1)
        {
            float playDist = Vector3.Distance(transform.position, playerTrans.position);
            if (playDist < 3.0f)
            {
                fadeToLevel("TerrainSandbox");
                //SceneManager.LoadScene("TerrainSandbox", LoadSceneMode.Single);
            }
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("faderani3"))
        {
            if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.4f){

                onFadeComplete();
            }
        }
        
    }

    public void onFadeComplete()
    {
        SceneManager.LoadScene(chosenlvl, LoadSceneMode.Single);
    }

}
