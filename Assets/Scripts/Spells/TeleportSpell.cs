using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TeleportSpell : MonoBehaviour, ISpell
{
	[SerializeField]
    GestureRecognition.Gesture gesture;
	public GestureRecognition.Gesture SpellGesture => gesture;
    [SerializeField]
    Color color = Color.white;
	public Color OrbColor => color;


	[SerializeField]
    TrajectoryPreview trajectory;
	[SerializeField]
	GameObject effectPrefab;
	[SerializeField]
	LayerMask groundMask;
	[SerializeField]
	new Transform collider;
	// [SerializeField]
	float velocity;

	public void OnAimEnd()
	{
		trajectory?.gameObject.SetActive(false);
		// trajectory.velocity = 10000;
	}

	public void OnAimStart()
	{
		trajectory?.gameObject.SetActive(true);
		// trajectory.velocity = velocity;
	}

	public void UnleashSpell()
	{
        RaycastHit hit;
        if(Physics.Raycast(trajectory.transform.position, trajectory.transform.forward, out hit, 500, groundMask))
        // if(RaycastArc(trajectory.transform.position, trajectory.transform.forward, out hit, 500, groundMask))
        {
            if (effectPrefab)
			{
				var newObj = Instantiate(effectPrefab);
				newObj.transform.position = collider.position - Vector3.up;
				Destroy(newObj, 5);
			}
			// setting transform.position directly does not seem to work with character controller
			GetComponent<CharacterController>().Move(hit.point - transform.position);
			if (effectPrefab)
			{
				var newObj = Instantiate(effectPrefab);
				newObj.transform.position = collider.position;
				Destroy(newObj, 5);
			}
        }

	}

	bool RaycastArc(Vector3 start, Vector3 direction, out RaycastHit hit, float maxDistance, LayerMask layerMask)
	{
		Vector3.Normalize(direction);
		float stepSize = 0.2f;
		int numberOfSegments = 1000;
		Vector3 currentPos = start;
		Vector3 endPos = start + (direction * velocity + Physics.gravity / 2 * stepSize) * stepSize;
		float currentDrop = Physics.gravity.y;
		for (int i = 0; i < numberOfSegments; i++)
		{
			var delta = endPos - currentPos;
			// Debug.DrawLine(currentPos, endPos, color, 1);
			if (Physics.Raycast(currentPos, delta, out hit, delta.magnitude, layerMask))
			{
				return true;
			}
			currentPos = endPos;
			endPos = start + (i + 1) * (direction * velocity + Physics.gravity * (i+1) / 2 * stepSize) * stepSize;
		}
		hit = new RaycastHit();
		return false;
	}

	// Start is called before the first frame update
	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("j"))
        {
            UnleashSpell();
        }
        // RaycastHit hit;
        // RaycastArc(trajectory.transform.position, trajectory.transform.forward, out hit, 500, groundMask);
    }
}
