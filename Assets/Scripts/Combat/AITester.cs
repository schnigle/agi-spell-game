using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITester : MonoBehaviour
{

    [SerializeField]
    EnemyAI enemy;
    Camera camera;
    [SerializeField]
    GameObject projectile;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Order AI to move to position
        if (Input.GetMouseButtonDown(1) && enemy.agent.enabled)
        {
            Vector3 groundHit;
            if (CursorToGroundHit(out groundHit))
            {
                enemy.agent.destination = groundHit;
                // enemy.IndirectlyMoveTowards(groundHit);
            }
        }
        // Toggle ragdoll
        if (Input.GetKeyDown("r"))
        {
            enemy.isRagdolling = !enemy.isRagdolling;
            // enemy.body.AddForce(new Vector3(Random.Range(-5,5),15,Random.Range(-5,5)), ForceMode.VelocityChange);
            // enemy.body.AddTorque(new Vector3(Random.Range(-50,50), Random.Range(-50,50), Random.Range(-50,50)), ForceMode.VelocityChange);
        }
        // Spawn a projectile that falls down on the enemy
        if (Input.GetKeyDown("p"))
        {
            Instantiate(projectile, enemy.transform.position + Vector3.up * 10, new Quaternion());
        }
    }

    bool CursorToGroundHit(out Vector3 groundHit)
    {
        var mousePosition = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            groundHit = hit.point;
            return true;
        }
        groundHit = new Vector3();
        return false;
    }
}
