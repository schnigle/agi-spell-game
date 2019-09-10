using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var colliderToIgnore = GameObject.Find("PlayerObject").GetComponent<Collider>(); // traversing hierarchy like a tree
        Physics.IgnoreCollision(colliderToIgnore, GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log(col.gameObject.name + " _ " + gameObject.name);
    }
}
