using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSpellExample : MonoBehaviour, ISpell
{
	public GestureRecognition.Gesture SpellGesture => GestureRecognition.Gesture.circle_ccw;

	[SerializeField]
    Color color = Color.white;
	public Color OrbColor => color;

	public void OnAimEnd()
	{
		print("Aim end");
	}

	public void OnAimStart()
	{
		print("Aim start");
	}

	public void UnleashSpell()
	{
		print("Unleash");
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
