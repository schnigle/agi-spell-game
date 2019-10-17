using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchLight : MonoBehaviour
{
    [SerializeField]
    float intensityFlickerStrength = 1;
    [SerializeField]
    float intensityFlickerFrequency = 1;
    new Light light;
    float defaultIntensity;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
        defaultIntensity = light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = defaultIntensity + Mathf.Sin(Time.time * intensityFlickerFrequency) * intensityFlickerStrength + Mathf.Sin(Time.time * intensityFlickerFrequency / 3) * intensityFlickerStrength;
    }
}
