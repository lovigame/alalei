using UnityEngine;
using System.Collections;

public class FloatCS : MonoBehaviour {

	private Vector3 dst;
	private Vector3 src;
	private float delta = 0.0f;

	private float duration = 1f;
	// Use this for initialization
	void Start () {
		src = this.transform.localPosition;
		dst = getDestination();
	}
	
	// Update is called once per frame
	void Update () {

		//if (Vector3.Distance (this.transform.localPosition, dst) < 0.2) {
		if(delta >= duration){
			src = this.transform.localPosition;
			dst = getDestination();
			delta = 0f;

		}
		delta += Time.deltaTime;
		this.transform.localPosition = new Vector3(Mathf.Lerp (src.x,dst.x,delta/duration),
		                                           Mathf.Lerp (src.y,dst.y,delta/duration),
		                                           Mathf.Lerp (src.z,dst.z,delta/duration));
	}

	Vector3 getDestination(){
		Matrix4x4 mat = new Matrix4x4 ();
		mat.SetTRS (new Vector3(0,0,0),Quaternion.Euler(new Vector3(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360))),new Vector3(1,1,1));

		float l = Random.Range (4,5);
		return mat.MultiplyVector(new Vector3(l,0,0));
	}
}
