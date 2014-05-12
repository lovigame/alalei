using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour {
	public float waterLevel, floatHeight;
	public Vector3 buoyancyCentreOffset;
	public float bounceDamp;
	
	private float starty;
	void Start(){
		starty = transform.position.y;
		}

	void FixedUpdate () {
		Vector3 actionPoint = transform.position + transform.TransformDirection(buoyancyCentreOffset);
		float forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);
		
		if (forceFactor > 0f) {
			if (forceFactor >= 0.98f && forceFactor <= 1.02f) {	
				//Debug.Log ("factor" + forceFactor);
				//rigidbody.AddForceAtPosition(new Vector3(0,200,0),actionPoint);
				//transform.position = new Vector3(transform.position.x,starty,transform.position.z);
			}
			//Debug.Log (transform.position + " f "+actionPoint);
			Vector3 uplift = -Physics.gravity * (forceFactor - rigidbody.velocity.y * bounceDamp);
			//Debug.Log(uplift.y);
			rigidbody.AddForceAtPosition(uplift, actionPoint);
		}


	}
}
