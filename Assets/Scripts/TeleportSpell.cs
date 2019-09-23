using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSpell : MonoBehaviour, ISpell
{
	public GestureRecognition.Gesture SpellGesture => GestureRecognition.Gesture.circle_cw;

	[SerializeField]
    TrajectoryPreview trajectory;

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
        if(Physics.Raycast(trajectory.transform.position, trajectory.transform.forward, out hit))
        {
            // setting transform.position directly does not seem to work with character controller
            GetComponent<CharacterController>().Move(hit.point - transform.position);
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
