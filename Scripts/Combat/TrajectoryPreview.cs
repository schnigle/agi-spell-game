using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Usage: the object with trajectory script should be placed at the tip of the wand so that the
    objects forward (positive Z-axis) is pointing in the direction of the wand. You then only need
    to specify the velocity of the projectile and enable or disable the trajectory.
*/
public class TrajectoryPreview : MonoBehaviour
{
    LineRenderer lineRenderer;
    [Range(0.01f, 1f)]
    [SerializeField]
    float previewDeltaTime = 0.1f;
    [SerializeField]
    int previewPositions = 100;
    public float velocity = 10;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void DisableTrajectory()
    {
        lineRenderer.enabled = false;
    }

    public void EnableTrajectory()
    {
        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (lineRenderer.enabled)
        {
            Vector3 initialVelocity = velocity * transform.forward;
            lineRenderer.positionCount = previewPositions;
            var currentPreviewPosition = Vector3.zero;
            float currentPreviewTime = 0;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, transform.worldToLocalMatrix * currentPreviewPosition);
                currentPreviewTime = i * previewDeltaTime;
                currentPreviewPosition = initialVelocity * currentPreviewTime + Physics.gravity * currentPreviewTime * currentPreviewTime / 2;
            }
        }
    }
}
