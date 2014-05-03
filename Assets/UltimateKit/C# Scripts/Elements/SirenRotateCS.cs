/*
*	FUNCTION: Rotate the siren on the police car.
*
*/

using UnityEngine;
using System.Collections;

public class SirenRotateCS : MonoBehaviour {

	private Transform tBackgroundRotation;
	private float fBackgroundRotateValue = 0.0f;
	
	void Start () 
	{
		tBackgroundRotation = this.transform;
	}
	
	void FixedUpdate () 
	{
		fBackgroundRotateValue = Mathf.Lerp(fBackgroundRotateValue, 8.0f, Time.deltaTime);
		tBackgroundRotation.transform.Rotate(0,fBackgroundRotateValue,0);
	}
}
