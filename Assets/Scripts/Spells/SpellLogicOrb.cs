using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogicOrb : MonoBehaviour
{
    public GameObject muzzlePrefab, hitPrefab;
    bool hit = false;
    private GameObject latesthitObject;

    private Vector3 playerPos;

    void Start()
    {
        objectsToPullIn = new List<GameObject>();
        objectsPulled = new Dictionary<GameObject, bool>();

        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward;

            var psMuzzle = muzzleVFX.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
            {
                Destroy(muzzleVFX, psMuzzle.main.duration);
            }
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }
        }

        playerPos = GameObject.Find("PlayerObject").GetComponent<Transform>().position;

    }

    //////////////////// Code from https://answers.unity.com/questions/1484056/tornado-physics-2.html

    public float pullInSpeed = 50f;
    public float rotateSpeed = 2.25f;
    public float radius = 30;
    public List<GameObject> objectsToPullIn;
    public Dictionary<GameObject, bool> objectsPulled;

    void RotateObjects()
    {
        foreach (GameObject thing in objectsPulled.Keys)
        {
            if (objectsPulled[thing] == true)
            {
                thing.transform.RotateAround(Vector3.zero, transform.up, thing.GetComponent<Rigidbody>().mass * rotateSpeed * Time.deltaTime);
            }
        }
    }

    void GetObjectsToPullIn()
    {
        Collider[] objects = Physics.OverlapSphere(GetComponent<Collider>().bounds.center, radius);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!(objectsToPullIn.Contains(objects[i].gameObject))
                && objects[i].gameObject != gameObject
                && objects[i].GetComponent<Rigidbody>() != null)
            {

                if (LayerMask.NameToLayer("Default") != objects[i].gameObject.layer)
                {
                    continue;
                }

                if (objects[i].tag == "Actor")
                {
                    var enemy = objects[i].GetComponent<EnemyAI>();
                    enemy.isRagdolling = true;
                }

                objectsToPullIn.Add(objects[i].gameObject);
                objectsPulled.Add(objects[i].gameObject, false);
            }
        }
    }

    void PullObjectsIn()
    {
        foreach (GameObject thing in objectsToPullIn)
        {
            if (objectsPulled[thing] != true)
            {
                var otherRig = thing.GetComponent<Rigidbody>();
                Vector3 dir = (gameObject.transform.position - otherRig.position);
                dir.y = 0.01f;
                dir.Normalize();
                otherRig.AddForce(dir * 80f* otherRig.mass);
               // thing.transform.position = Vector3.MoveTowards(thing.transform.position, transform.position, thing.GetComponent<Rigidbody>().mass * Time.deltaTime * pullInSpeed);
               // float dist = Vector3.Distance(thing.transform.position, transform.position);

            }
        }
    }

   /* void OnCollisionEnter(Collision other)
    {
        if (objectsToPullIn.Contains(other.gameObject))
        {
            objectsPulled[other.gameObject] = true;
            RotateObjects();
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (objectsToPullIn.Contains(other.gameObject))
        {
            objectsPulled[other.gameObject] = true;
            RotateObjects();
        }
    }

    void OnCollisionExit(Collision other)
    {
      //  if (objectsToPullIn.Contains(other.gameObject))
      //  {
          //  objectsPulled[other.gameObject] = false;
      //  }
    }*/


    ///////////////////////////////////////////

    void OnDestroy()
    {
        foreach (var obj in objectsToPullIn)
        {
            var otherRig = obj.GetComponent<Rigidbody>();
            if (otherRig == null)
                continue;

            Vector3 dir = (gameObject.transform.position - obj.gameObject.transform.position);
            dir.y = 0.1f;
            dir.Normalize();
            otherRig.AddForce(dir*otherRig.mass*700);

            if (obj.tag == "Actor")
            {
            }
        }

        if(hitPrefab != null)
            Instantiate(hitPrefab,transform);
    }



    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    void moveTornado()
    {
        Vector3 dir = (gameObject.transform.position - playerPos);
        dir.y = 0.1f;
        dir.Normalize();
        var rigidBod = gameObject.GetComponent<Rigidbody>();
        rigidBod.AddRelativeForce(dir * 0.01f, ForceMode.VelocityChange);
        if(transform.position.y < 50)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 50f, transform.position.z), GetComponent<Rigidbody>().mass * Time.deltaTime * pullInSpeed);
        }

    }

    void Update()
    {
        moveTornado();
        GetObjectsToPullIn();
        PullObjectsIn();
        RotateObjects();
    }
}
