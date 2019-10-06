using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public void OnAimEnd()
	{
		trajectory?.gameObject.SetActive(false);
	}

	public void OnAimStart()
	{
		trajectory?.gameObject.SetActive(true);
	}

	public void UnleashSpell()
	{
        RaycastHit hit;
        if(Physics.Raycast(trajectory.transform.position, trajectory.transform.forward, out hit, 500, groundMask))
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

	// Start is called before the first frame update
	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
