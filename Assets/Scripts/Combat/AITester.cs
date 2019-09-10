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
        if (Input.GetMouseButtonDown(1))
        {
            var mousePosition = Input.mousePosition;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 targetPosition = hit.point;
                // enemy.IndirectlyMoveTowards(targetPosition);
                enemy.Agent.destination = targetPosition;
            }
        }

        if (Input.GetKeyDown("p"))
        {
            Instantiate(projectile, enemy.transform.position + Vector3.up * 10, new Quaternion());
        }
    }
}
