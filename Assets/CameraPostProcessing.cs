using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;

public class CameraPostProcessing : MonoBehaviour
{
    // Singleton, everyones favorite anti-pattern
    public static CameraPostProcessing instance { get; private set; }

    [SerializeField]
    AnimationCurve hitEffectCurve;
    [Range(0.1f, 2f)]
    [SerializeField]
    float hitEffectDuration = 1;

    PostProcessVolume volume;
    bool enableHitEffect;
    float currentHitCurvePosition = 0;

    void Awake()
    {
        instance = this;
        volume = GetComponent<PostProcessVolume>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableHitEffect)
        {
            currentHitCurvePosition += Time.deltaTime;
            if (currentHitCurvePosition > 1)
            {
                enableHitEffect = false;
            }
            volume.weight = hitEffectCurve.Evaluate(currentHitCurvePosition / hitEffectDuration);
        }
        else
        {
            volume.weight = 0;
        }
    }

    public void TriggerHit()
    {
        enableHitEffect = true;
        currentHitCurvePosition = 0;
    }
}
