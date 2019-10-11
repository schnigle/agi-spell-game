using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWaveSpell : MonoBehaviour
{
    [SerializeField]
    GestureRecognition.Gesture gesture;
    public GestureRecognition.Gesture SpellGesture => gesture;
    [SerializeField]
    //Color color = Color.white;
    //public Color OrbColor => color;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float shockForce = 5.0f;
    public float radius = 10f;
    private const float waitTime = 5.0f;

    [SerializeField]
    TrajectoryPreview trajectory;

    public void UnleashSpell()
    {
        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;

        Vector3 pos = bulletEmitter .transform.position;


        Vector3 explosionPos = transform.position;

        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders){
          Rigidbody rb = hit.GetComponent<Rigidbody>();
          if (rb != null)
          rb.AddExplosionForce(shockForce, explosionPos, radius, 3.0F);
                }


/*        var nearby = Physics.OverlapSphere(pos, 8);
        foreach(var item in nearby)
        {
            Rigidbody rigidbody = null;
            if(item.tag == "Actor")
            {
                //print("Death good");
                var enemy = item.GetComponent<EnemyAI>();
                enemy.isRagdolling = true;
            }
            rigidbody = item.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                var direction = (rigidbody.transform.position - pos).normalized;
                rigidbody.AddForce((5 - Vector3.Distance(rigidbody.position, pos)) * (direction) * 200 * shockForce, ForceMode.Impulse);
            }*/
        Rigidbody tempBody;
        Destroy(tempBull, waitTime);
    }

    public void OnAimStart()
    {
        trajectory?.gameObject.SetActive(true);
    }

    public void OnAimEnd()
    {
        trajectory?.gameObject.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            UnleashSpell();
        }


    }
}
