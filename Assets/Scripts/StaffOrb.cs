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
    [SerializeField]
    MeshRenderer icon;
    [SerializeField]
    new Light light;
    Material orbMaterial;
    // Texture iconTexture;
    Color oldColor;
    // float colorSwitchTimer;
    Color currentColor;
    // [SerializeField]
    // [ColorUsage(true, true)]
    // Color iconColor;
    // Start is called before the first frame update
    [HideInInspector]
    public bool iconEnabled;
    Vector3 defaultIconSize;

    void Start()
    {
        if (orb)
        {
            orbMaterial = orb.material;
        }
        // if (icon)
        // {
        //     iconTexture = icon.material.mainTexture;
        // }
        currentColor = mainColor;
        defaultIconSize = icon.transform.localScale;
        icon.transform.localScale = Vector3.zero;
    }

    public void SetIcon(Texture iconTexture)
    {
        icon.material.mainTexture = iconTexture;
    }

    public void StartDraw()
    {
        // orb.gameObject.SetActive(false);
    }

    public void EndDraw()
    {
        // orb.gameObject.SetActive(true);
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
        var orbColorFactor = 0.40f;
        orbColor.r *= orbColorFactor;
        orbColor.g *= orbColorFactor;
        orbColor.b *= orbColorFactor;
        orbMaterial.SetColor("_Color", orbColor);
        // iconColor = mainColor;
        // iconColor *= 1.4f;
        // print(iconColor);
        icon.material.SetColor("_Color", currentColor * 2.5f);
        main = stars.main;
        main.startColor = currentColor;
        main.simulationSpeed = Mathf.Lerp(main.simulationSpeed, 1, Time.deltaTime * 10);
        main = haze.main;
        main.startColor = currentColor;
        main.simulationSpeed = Mathf.Lerp(main.simulationSpeed, 1, Time.deltaTime * 10);

        if (iconEnabled)
        {
            icon.transform.localScale = Vector3.Lerp(icon.transform.localScale, defaultIconSize, Time.deltaTime * 30f);
        }
        else
        {
            icon.transform.localScale = Vector3.Lerp(icon.transform.localScale, Vector3.zero, 1);
        }

        light.color = currentColor;
    }
}
