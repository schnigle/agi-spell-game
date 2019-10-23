using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSpellExample : SpellBase
{
	public override void OnAimEnd()
	{
		print("Aim end");
	}

	public override void OnAimStart()
	{
		print("Aim start");
	}

	public override void UnleashSpell()
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
