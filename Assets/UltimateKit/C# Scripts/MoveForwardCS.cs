using UnityEngine;
using System.Collections;

public class MoveForwardCS : MonoBehaviour {

	public float fStartDistance = 200.0f;
	public float fSpeed = 3.0f;
	public float fPercent = 0.0f;

	private bool bMoving = false;
	private CheckPointsMainCS hCheckPointsMainCS;
	private float fSpeedPercent = 0.0f;
	private GameObject patch;
	private PatchesRandomizerCS hPatchesRandomizerCS;
	private PathLineDrawerCS hPathLineDrawerCS;
	private Vector3 offset;
	private bool isNext = false;
	private bool isStoped = false;
	private bool isFirst = false;
	private float fAngle = 0.0f;
	// Use this for initialization
	void Start () {
		bMoving = true;

		hCheckPointsMainCS = (CheckPointsMainCS)GameObject.Find("Player").GetComponent(typeof(CheckPointsMainCS));
		hPatchesRandomizerCS = (PatchesRandomizerCS)GameObject.Find("Player").GetComponent(typeof(PatchesRandomizerCS));
		if (this.transform.parent.parent == null) {
			hPathLineDrawerCS = (PathLineDrawerCS)this.transform.parent.GetComponentInChildren(typeof(PathLineDrawerCS));
			patch = this.transform.parent.gameObject;
				} else {
			hPathLineDrawerCS = (PathLineDrawerCS)this.transform.parent.parent.GetComponentInChildren(typeof(PathLineDrawerCS));
			patch = this.transform.parent.parent.gameObject;
				}

		fSpeedPercent = fSpeed/hPathLineDrawerCS.fPathLength;

		isNext = patch == hPatchesRandomizerCS.getNextPatch();
		isFirst = patch == hPatchesRandomizerCS.getCurrentPatch();
		Vector3 pos;
		if (isNext) {
			pos = hCheckPointsMainCS.getNextWSPointBasedOnPercent(fPercent);
			fAngle=hPatchesRandomizerCS.getNextPatch().transform.eulerAngles.y;
		} else {
			pos = hCheckPointsMainCS.getCurrentWSPointBasedOnPercent(fPercent);
			fAngle=hPatchesRandomizerCS.getCurrentPatch().transform.eulerAngles.y;
		}
		
		offset = this.transform.position - pos;


	}

	public void stop(){
		isStoped = true;
	}

	private float PosAngleofVector(Vector3 InputVector)
	{
		float AngleofInputVector = 57.3f * (Mathf.Atan2(InputVector.z,InputVector.x));
		if(AngleofInputVector<0.0f)
			AngleofInputVector = AngleofInputVector + 360.0f;
		return AngleofInputVector;
	}
	
	// Update is called once per frame
	void Update () {
		if (isStoped) {
			return;		
		}

		if (bMoving) {
			fPercent  -= fSpeedPercent;

			Vector3 pos;
			if(fPercent <= 0){

				if(isNext){
					isNext = false;
					fPercent += 1f;

				}
				else{
					Destroy(this.gameObject);
					return;
				}


			}

			if(isNext){
				if(patch == hPatchesRandomizerCS.getNextPatch())
				{
					pos = hCheckPointsMainCS.getNextWSPointBasedOnPercent(fPercent);
					//this.transform.localEulerAngles = new Vector3(this.transform.rotation.x, hCheckPointsMainCS.getNextAngle(fPercent),this.transform.rotation.y);
				}
				else if(patch == hPatchesRandomizerCS.getCurrentPatch())
				{
					pos = hCheckPointsMainCS.getCurrentWSPointBasedOnPercent(fPercent);
					//this.transform.localEulerAngles = new Vector3(this.transform.rotation.x, hCheckPointsMainCS.getCurrentAngle(fPercent),this.transform.rotation.y);
				}
				else{
					Destroy(this.gameObject);
					return;
				}
			}
			else{

				if(isFirst){
					if(patch == hPatchesRandomizerCS.getCurrentPatch())
					{
						pos = hCheckPointsMainCS.getCurrentWSPointBasedOnPercent(fPercent);
						//this.transform.localEulerAngles = new Vector3(this.transform.rotation.x, hCheckPointsMainCS.getCurrentAngle(fPercent),this.transform.rotation.y);
					}
					else{
						Destroy(this.gameObject);
						return;
					}
				}
				else{
					if(patch == hPatchesRandomizerCS.getCurrentPatch()){
						Destroy(this.gameObject);
						return;
					}
					pos = hCheckPointsMainCS.getCurrentWSPointBasedOnPercent(fPercent);
					//this.transform.localEulerAngles = new Vector3(this.transform.rotation.x, hCheckPointsMainCS.getCurrentAngle(fPercent) - (fAngle-hPatchesRandomizerCS.getCurrentPatch().transform.eulerAngles.y),this.transform.rotation.y);
				}

			}

			Vector3 dir = (pos+offset) - this.transform.position;
			float angle = PosAngleofVector(dir);
			if(angle>180.0f)
				angle = angle-360.0f;
	
			this.transform.eulerAngles = new Vector3(this.transform.rotation.x, -angle,this.transform.rotation.z);


			this.transform.position = pos + offset;
		
				} else {
			if(Vector3.Distance(GameObject.Find("Player").transform.position,this.transform.position) <= fStartDistance){
				bMoving = true;
			}


				}
	}
}
