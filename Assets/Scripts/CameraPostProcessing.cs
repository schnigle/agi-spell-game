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

    bool enableHitEffect;
    float currentHitCurvePosition = 0;

    [SerializeField]
    PostProcessVolume hitVolume;

    [SerializeField]
    PostProcessVolume fadeVolume;

    bool fade = false;
    float fadeWeight = 0;


    void Awake()
    {
        instance = this;
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
            hitVolume.weight = hitEffectCurve.Evaluate(currentHitCurvePosition / hitEffectDuration);
        }
        else
        {
            hitVolume.weight = 0;
        }
        if (fade)
        {
            fadeWeight += Time.deltaTime;
            fadeVolume.weight = fadeWeight;
        }
        else
        {
            fadeVolume.weight = 0;
        }
    }

    public void TriggerHit()
    {
        enableHitEffect = true;
        currentHitCurvePosition = 0;
    }

    public void FadeOut()
    {
        fade = true;
    }
}
