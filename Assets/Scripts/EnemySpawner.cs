using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    GameObject spawnPrefab;
    [SerializeField]
    float spawnTimer = 10;
    float timeUntilSpawn;
    // Start is called before the first frame update
    void Start()
    {
        timeUntilSpawn = spawnTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilSpawn -= Time.deltaTime;
        if (timeUntilSpawn < 0)
        {
            timeUntilSpawn = spawnTimer;
            if (spawnPrefab)
            {
                Instantiate(spawnPrefab, transform.position, new Quaternion());
            }
        }
    }
}
