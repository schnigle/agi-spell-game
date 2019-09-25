using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSpell : MonoBehaviour, ISpell
{
    [SerializeField]
    Shield shieldPrefab = null;
    Shield instantiatedShield = null;
    [SerializeField]
    Transform staffKnob = null;
    [SerializeField]
    float shieldDistance = 2;
    bool aimingShield = false;

	public GestureRecognition.Gesture SpellGesture => GestureRecognition.Gesture.circle_ccw;

	public void OnAimEnd()
	{
        instantiatedShield.SetTimer(10);
        aimingShield = false;
		instantiatedShield = null;
	}

	public void OnAimStart()
	{
		if (instantiatedShield)
        {
            Destroy(instantiatedShield);
            instantiatedShield = null;
        }
        if (staffKnob)
        {
            aimingShield = true;
            instantiatedShield = Instantiate(shieldPrefab);
            UpdateShieldPosition();
        }
	}

    void UpdateShieldPosition()
    {
        if (staffKnob != null && instantiatedShield != null)
        {
            instantiatedShield.transform.rotation = staffKnob.rotation;
            instantiatedShield.transform.position = staffKnob.position + staffKnob.forward * shieldDistance;
        }
    }

	public void UnleashSpell()
	{
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (aimingShield)
        {
            UpdateShieldPosition();
        }
    }
}
