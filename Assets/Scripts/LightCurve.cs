using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCurve : MonoBehaviour
{
    [SerializeField]
    AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    [SerializeField]
    float duration = 1;
    float timer;
    new Light light;
    float defaultIntensity;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
        defaultIntensity = light.intensity;
        light.intensity = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        light.intensity = curve.Evaluate(timer);
    }
}
