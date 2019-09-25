using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTester : MonoBehaviour
{
    ISpell spell;
    Camera cam;

    void Awake()
    {
        spell = GetComponent<ISpell>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (spell != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                spell.OnAimStart();
            }
            if (Input.GetMouseButtonUp(0))
            {
                spell.OnAimEnd();
                spell.UnleashSpell();
            }
            if (cam)
            {
                transform.position = cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,1));
                transform.LookAt(cam.transform.position);
                transform.rotation =  Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(180, 0, 0));
            }
        }
    }
}
