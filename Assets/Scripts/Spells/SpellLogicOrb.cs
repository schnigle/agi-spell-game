using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogicOrb : MonoBehaviour
{
    public GameObject muzzlePrefab, hitPrefab;
    bool hit = false;
    private GameObject latesthitObject;
    private Vector3 playerPos;
    private Rigidbody rigidbody;

    public bool marked = false;
    public float targetMag = 1.0f;
    public bool dieAnim = false;

    public Material dissolvMat;

    float startTime = 0;
    float maxTime = 0;
    const float endTime = 1500.0f;

    public void setStartTime(float startTime)
    {
        this.startTime = startTime;
    }

    public void setMaxTime(float maxTime)
    {
        this.maxTime = maxTime;
    }

    public float getTimeLeft()
    {
        float nowTime = Time.time * 1000.0f;
        float passedTime = nowTime - startTime;
        float timeLeft = (maxTime * 1000.0f) - passedTime;
        return timeLeft;
    }

    public bool endStarted()
    {
        return getTimeLeft() < endTime;
    }

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
        rigidbody = GetComponent<Rigidbody>();


        var ps = GetComponentsInChildren<Transform>();
        foreach (var x in ps)
        {
            if (!dieAnim)
            {
                if (x.name == "_DarkOrb")
                {
                    var tmpMat = x.GetComponent<Renderer>().material;
                    dissolvMat = tmpMat;
                }
            }
        }


    }

    //////////////////// Code from https://answers.unity.com/questions/1484056/tornado-physics-2.html

    public float pullInSpeed = 40f;
    public float rotateSpeed = 2.25f;
    public float radius = 5;
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

                if("blackorb" == objects[i].gameObject.tag)
                {
                    var tmpScript = objects[i].gameObject.GetComponent<SpellLogicOrb>();
                    if (tmpScript != null && !tmpScript.marked)
                    {
                        Transform otherTrans = objects[i].gameObject.transform;
                        Vector3 otherTarg = otherTrans.position;
                        float step = 1.0f * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(rigidbody.position, otherTarg, step);
                    }
                    continue;
                }else
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
            }
        }
    }


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

        if (!dieAnim)
        {
            moveTornado();
            GetObjectsToPullIn();
            PullObjectsIn();
            RotateObjects();
        }


        var ps = GetComponentsInChildren<Transform>();
        foreach(var x in ps)
        {
            if (!dieAnim)
            {
                if (x.name == "_DarkOrb")
                {
                    Vector3 scale = x.transform.localScale;
                    if (scale.magnitude < targetMag)
                    {
                        scale *= 1.03f;
                        x.transform.localScale = scale;
                    }
                }
            }
            else //destroy animation
            {
                if (x.name == "_DarkOrb")
                {
                    Vector3 scale = x.transform.localScale;
                    scale *= 0.96f;
                    x.transform.localScale = scale;
                    if (scale.magnitude < 0.01f)
                        Destroy(this);
                }
            }
        }

        float timeLeft = getTimeLeft();
        if(!marked && timeLeft < endTime)
        {
            float tempTime = 1.0f - ((endTime - timeLeft) / endTime);
            dissolvMat.SetFloat("Vector1_BC86B7E7", tempTime);
        }

        //Debug.Log("TIME: " + timeLeft);

       // float number = Random.Range(0.3f, 1.0f);
      //  dissolvMat.SetFloat("Vector1_BC86B7E7", number);
    }
}
