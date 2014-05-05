/*
*	FUNCTION:
*	- This scirpt handles the creation and destruction of the environment patches.
*
*	USED BY:
*	This script is a part of the "Player" prefab.
*
*/
using UnityEngine;
using System.Collections;

public class PatchesRandomizerCS : MonoBehaviour {

	public GameObject[] patchesPrefabs;//patches that will be generated

	private GameObject goPreviousPatch;//the patch the the player passed
	private GameObject goCurrentPatch;//the patch the player is currently on
	private GameObject goNextPatch;//the next patch located immediatly after current patch
	private GameObject goNextPatch2;
	private float fPatchDistance;//default displacement of patch
	private Transform tPlayer;//player transform
	
	private float fPreviousTotalDistance = 0.0f;//total displacement covered
	private float fTotalPath = 0.0f;
	private float fPrePath = 0.0f;
	private int iCurrentPNum = 1;//number of patches generated
	
	//script references
	private InGameScriptCS hInGameScriptCS;
	private ElementsGeneratorCS hElementsGeneratorCS;
	private CheckPointsMainCS hCheckPointsMainCS;
	private double dElapseTime = 0.0;

	private Vector3 vPatchStartPos;

	private bool isCross = false;
	
	//get the current path length
	public float getCoveredDistance() { return fPreviousTotalDistance; } 
	
	void Start()
	{
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		hCheckPointsMainCS = (CheckPointsMainCS)GetComponent(typeof(CheckPointsMainCS));
		//hElementsGeneratorCS = (ElementsGeneratorCS)this.GetComponent(typeof(ElementsGeneratorCS));
		
		iCurrentPNum = 1;
		fPreviousTotalDistance = 0.0f;
		fPatchDistance = hCheckPointsMainCS.getDefaultPathLength();
		
		instantiateStartPatch();	
		goPreviousPatch = goCurrentPatch;	
		
		tPlayer = GameObject.Find("Player").transform;
		hCheckPointsMainCS.setChildGroups();
		
		hCheckPointsMainCS.SetCurrentPatchCPs();
		hCheckPointsMainCS.SetNextPatchCPs();
	}
	
	void Update()
	{
		if(hInGameScriptCS.isGamePaused()==true)
			return;
		
		//destroy the patch if the Player has crossed it
		//if(tPlayer.position.x>(iCurrentPNum*fPatchDistance)+100.0f)
		if(isCross)
		{
			dElapseTime += Time.deltaTime;
			if(dElapseTime > 1){
				isCross = false;
				Destroy(goPreviousPatch);
				iCurrentPNum++;
			}
		}
	}//end of update
	
	/*
	*	FUNCTION: Create a new Patch after the player reaches goNextPatch
	*	CALLED BY:	CheckPointsMainCS.SetNextMidPointandRotation(...)
	*/
	public void createNewPatch()
	{
		goPreviousPatch = goCurrentPatch;
		goCurrentPatch = goNextPatch;
		fPrePath = fTotalPath;
		fTotalPath += getPatchSize (goCurrentPatch);
		isCross = true;
		dElapseTime = 0.0;
		instantiateNextPatch();	
		hCheckPointsMainCS.setChildGroups();


		fPreviousTotalDistance += CheckPointsMainCS.fPathLength;
		
	//	hElementsGeneratorCS.generateElements();	//generate obstacles on created patch
	}
	
	private void instantiateNextPatch()
	{	
		//goNextPatch = (GameObject)Instantiate((GameObject)patchesPrefabs[UnityEngine.Random.Range(0,patchesPrefabs.Length)], new Vector3(fPatchDistance*(iCurrentPNum+1),0,0), new Quaternion());
		goNextPatch = (GameObject)Instantiate((GameObject)patchesPrefabs[UnityEngine.Random.Range(0,patchesPrefabs.Length)], new Vector3(0,0,0), new Quaternion());
		//relocateCP (goNextPatch,new Vector3(fTotalPath+getPatchSize(goCurrentPatch),0,-getCPStartPosition(goCurrentPatch).z-getCPStartPosition(goNextPatch).z+getCPFinishPosition(goCurrentPatch).z));
		connectPatch (goCurrentPatch,goNextPatch);
	}
	
	/*
	*	FUNCTION: Instantiate the first patch on start of the game.
	*	CALLED BY: Start()
	*/
	private void instantiateStartPatch()
	{
		goCurrentPatch = (GameObject)Instantiate((GameObject)patchesPrefabs[UnityEngine.Random.Range(0,patchesPrefabs.Length)], new Vector3(0,0,0), new Quaternion());
		relocateCP (goCurrentPatch,new Vector3(0,0,-getCPStartPosition(goCurrentPatch).z));
		fTotalPath += getPatchSize (goCurrentPatch);

		//goNextPatch = (GameObject)Instantiate((GameObject)patchesPrefabs[UnityEngine.Random.Range(0,patchesPrefabs.Length)], new Vector3(fPatchDistance,0,0), new Quaternion());
		goNextPatch = (GameObject)Instantiate((GameObject)patchesPrefabs[UnityEngine.Random.Range(0,patchesPrefabs.Length)], new Vector3(0,0,0), new Quaternion());
		//relocateCP (goNextPatch,new Vector3(getPatchSize(goCurrentPatch),0,-getCPStartPosition(goCurrentPatch).z-getCPStartPosition(goNextPatch).z+getCPFinishPosition(goCurrentPatch).z));
		//fTotalPath += getPatchSize (goNextPatch);
		connectPatch (goCurrentPatch,goNextPatch);

		//goNextPatch2 = (GameObject)Instantiate((GameObject)patchesPrefabs[UnityEngine.Random.Range(0,patchesPrefabs.Length)], new Vector3(0,0,0), new Quaternion());
	//	connectPatch (goNextPatch,goNextPatch2);
	}

	public void relocateCP(GameObject obj,Vector3 pos)
	{
		obj.transform.Translate (pos);
		((PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS> ())).relocate (obj.transform.position);
	}

	public float getPatchSize(GameObject obj)
	{
		/*
		float t = 0.0f;
		try{
			TerrainCollider collider = ((TerrainCollider)(obj.GetComponentInChildren<TerrainCollider>()));
			t = collider.bounds.size.x;
		}
		catch{
			Debug.Log("exception");
		}

		return t;
		*/
		//return getCPFinishPosition (obj).x - getCPStartPosition (obj).x;

		return ((PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS>())).fWidth;
	}



	public Vector3 getCPStartPosition(GameObject obj)
	{
		Matrix4x4 mat = new Matrix4x4 ();
		mat.SetTRS (new Vector3(0,0,0),obj.transform.rotation,new Vector3(1,1,1));
		PathLineDrawerCS ptr = (PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS>());
		return mat.MultiplyVector(ptr.Parameterized_CPPositions[1]) + ptr.pos;
		//CPPositions = new Vector3[((PathLineDrawerCS)tCPsGroup.GetComponent(typeof(PathLineDrawerCS))).Parameterized_CPPositions.Length];
		//return new Vector3();
	}
	public Vector3 getCPFinishPosition(GameObject obj)
	{
		Matrix4x4 mat = new Matrix4x4 ();
		mat.SetTRS (new Vector3(0,0,0),obj.transform.rotation,new Vector3(1,1,1));
		PathLineDrawerCS ptr = (PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS>());
		return mat.MultiplyVector(ptr.Parameterized_CPPositions[ptr.Parameterized_CPPositions.Length-2]) + ptr.pos;
	}



	public Vector3 getOriginCPStartPosition(GameObject obj)
	{
		PathLineDrawerCS ptr = (PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS>());
		return ptr.Parameterized_CPPositions[1];
		//CPPositions = new Vector3[((PathLineDrawerCS)tCPsGroup.GetComponent(typeof(PathLineDrawerCS))).Parameterized_CPPositions.Length];
		//return new Vector3();
	}
	public Vector3 getOriginCPFinishPosition(GameObject obj)
	{
		PathLineDrawerCS ptr = (PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS>());
		return ptr.Parameterized_CPPositions[ptr.Parameterized_CPPositions.Length-2];
	}

	public PathLineDrawerCS.EN_PATHOUT getPatchType(GameObject obj)
	{
		return ((PathLineDrawerCS)(obj.GetComponentInChildren<PathLineDrawerCS> ())).ePathOut;
	}

	private void connectPatch(GameObject cur,GameObject next)
	{
		Vector3 pos = new Vector3(0,0,0);
		Vector3 cpos = getCPStartPosition (cur);
		Vector3 fpos = getCPFinishPosition (cur);
		Vector3 ocpos = getOriginCPStartPosition (cur);
		Vector3 ofpos = getOriginCPFinishPosition (cur);
		Vector3 spos = getOriginCPStartPosition (next);


		Vector3 eulerAngles = cur.transform.eulerAngles;
		switch (getPatchType (cur)) 
		{
		case PathLineDrawerCS.EN_PATHOUT.PATHOUT_TOP:
			/*
			pos.x = ofpos.x-10.5903f;
			pos.y = ofpos.y;
			pos.z = ofpos.z - ocpos.z - spos.z;
			*/
		//	pos.x = fpos.x - 10.5903f;
			eulerAngles.y -= 90;

			break;
		case PathLineDrawerCS.EN_PATHOUT.PATHOUT_BOTTOM:
			break;
		case PathLineDrawerCS.EN_PATHOUT.PATHOUT_LEFT:
			break;
		case PathLineDrawerCS.EN_PATHOUT.PATHOUT_RIGHT:
			pos.x = ofpos.x-10.5903f;
			pos.y = ofpos.y;
			pos.z = ofpos.z - ocpos.z - spos.z;
			break;
		}
		next.transform.RotateAround (fpos, new Vector3 (0, 1, 0), eulerAngles.y);


		relocateCP (next,-ocpos+fpos);
	}

	public GameObject getCurrentPatch() { return goCurrentPatch; }
	public GameObject getNextPatch() { return goNextPatch; }
}
