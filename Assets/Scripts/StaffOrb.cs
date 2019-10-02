using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffOrb : MonoBehaviour
{
    [SerializeField]
    public Color mainColor = Color.cyan;
    [SerializeField]
    ParticleSystem stars;
    [SerializeField]
    ParticleSystem haze;
    [SerializeField]
    MeshRenderer orb;
    Material orbMaterial;
    Color oldColor;
    // float colorSwitchTimer;
    Color currentColor;
    // Start is called before the first frame update
    void Start()
    {
        if (orb)
        {
            orbMaterial = orb.material;
        }
        currentColor = mainColor;
    }

    // Update is called once per frame
    void Update()
    {
        currentColor = Color.Lerp(currentColor, mainColor, Time.deltaTime * 20);
        UnityEngine.ParticleSystem.MainModule main;
        if (mainColor != oldColor)
        {
            main = haze.main;
            main.simulationSpeed = 150;
            main = stars.main;
            main.simulationSpeed = 150;
            // colorSwitchTimer = 0.5f;
        }
        oldColor = mainColor;
        // if (colorSwitchTimer > 0)
        // {
        //     colorSwitchTimer -= Time.deltaTime;
        // }
        var orbColor = currentColor;
        var orbColorFactor = 0.65f;
        orbColor.r *= orbColorFactor;
        orbColor.g *= orbColorFactor;
        orbColor.b *= orbColorFactor;
        orbMaterial.SetColor("_Color", orbColor);
        main = stars.main;
        main.startColor = currentColor;
        main.simulationSpeed = Mathf.Lerp(main.simulationSpeed, 1, Time.deltaTime * 10);
        main = haze.main;
        main.startColor = currentColor;
        main.simulationSpeed = Mathf.Lerp(main.simulationSpeed, 1, Time.deltaTime * 10);
    }
}
