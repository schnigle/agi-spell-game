using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    Vector3 defaultScale;
    public bool destroying = false;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        defaultScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (destroying)
        {
            timer -= Time.deltaTime;
        }
        if (timer <= 0 && destroying)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime*10);
            if (transform.localScale.sqrMagnitude < 0.01f)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, Time.deltaTime*10);
        }
    }

    public void SetTimer(float timer)
    {
        this.timer = timer;
        destroying = true;
    }
}
