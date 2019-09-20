using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{

    [SerializeField] private HealthBar healthBar;

    public GameObject playerObject;
    PlayerScript playerScript;
   
    void Start()
    {
        if(playerObject != null)
            playerScript = playerObject.GetComponent<PlayerScript>();
    }


    void Update()
    {
        if(playerScript == null)
        {
            playerScript = playerObject.GetComponent<PlayerScript>();
            return;
        }
        PlayerData playdata = playerScript.GetPlayerData();
        float lifePerc = playdata.hp /(float)playdata.totalHP;
        Vector3 lTemp = transform.localScale;
        lTemp.x = lifePerc;
        transform.localScale = lTemp;
        healthBar.transform.localScale = lTemp;
    }
}
