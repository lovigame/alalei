/*
*	FUNCTION:
*	- This script keeps a record of the CPs of active patches.
*	- This script translates the player position (controlled by ControllerScript.js) 
*	on the spline into Vector3 cooridiantes.
*	- It calculates the angle of player and obstacles according to the spline.
*	- It supports the ElementsGenerator.js to generate obstacles on the path also by
*	translating position on spline to Vector3 coordinates.
*
*	USED BY:
*	This script is a part of the "Player" prefab.
*
*	INFO: Understanding of algorithms that make the calculation based on spline is not
*	necessary. A high level undertstanding of each function can however can be handy.
*
*/

using UnityEngine;
using System.Collections;

public class CheckPointsMainCS : MonoBehaviour {

	//Constants
	private float defaultPathLength = 3000.0f;
	public float getDefaultPathLength() { return defaultPathLength; }
	
	//Variables
	public static float fPathLength = 0.0f;//length displacement of current patch
	public static float fNextPathLength = 0.0f;//length displacement of next patch
	public static float fTotalLength = 0.0f;
	
	private float CurrentAngle = 0.0f;
	private Vector3 CurrentDir;
	private Vector2 Current_MidPoint;
	
	private Vector3[] CPPositions;
	private Vector3[] NextCPPositions;
	
	private float WaypointAngle = 0.0f;
	private float CurrentPercent = 0.0f;
	
	private GameObject goCPsGroup;
	private GameObject goNextCPsGroup;
	private Transform tCPsGroup;
	private Transform tNextCPsGroup;
	private GameObject bosspatch;
	
	//script references
	private InGameScriptCS hInGameScriptCS;
	private ControllerScriptCS hControllerScriptCS;
	private PatchesRandomizerCS hPatchesRandomizerCS;
	private ElementsGeneratorCS hElementsGeneratorCS;

	private float BossAngle = 0.0f;
	private Vector3 BossDir;
	private Vector2 Boss_MidPoint;
	private float BossPercent = 0.0f;
	private int PatchNumber = 0;

	private Vector3 vNextStartPosition;


	
	void Start()
	{
		WaypointAngle = 0.0f;
		fPathLength = defaultPathLength;
		fNextPathLength = defaultPathLength;
		CurrentPercent = 0.0f;
		
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		hControllerScriptCS = (ControllerScriptCS)this.GetComponent(typeof(ControllerScriptCS));
		hPatchesRandomizerCS = (PatchesRandomizerCS)this.GetComponent(typeof(PatchesRandomizerCS));
		//hElementsGeneratorCS = (ElementsGeneratorCS)this.GetComponent(typeof(ElementsGeneratorCS));
	}


	public Vector3 getCPStartPosition()
	{
		return CPPositions[0];
	}

	public Vector3 getCPEndPosition()
	{
		return CPPositions[0];
	}

	/*
	*	FUNCTION: Get the CP gameobjects of the currently active patches.
	*	USED BY:	PatchesRandomizer.Start()
	*				PatchesRandomizer.Start()
	*/
	public void setChildGroups()
	{
		foreach (Transform child in hPatchesRandomizerCS.getCurrentPatch().transform)
		{
			if(child.name.Contains("CheckPoints"))
				goCPsGroup = child.gameObject;
		}
		foreach (Transform child in hPatchesRandomizerCS.getNextPatch().transform)
		{
			if(child.name.Contains("CheckPoints"))
				goNextCPsGroup = child.gameObject;
		}		
	}
	
	/*
	*	FUNCTION: Get the CP transforms of the currently active patch and store them in array.
	*	USED BY: PatchesRandomizer.Start()
	*/
	public void SetCurrentPatchCPs()
	{	
		CurrentAngle = 90.0f;
		
		tCPsGroup = goCPsGroup.transform;

		PathLineDrawerCS lineDrawer = ((PathLineDrawerCS)tCPsGroup.GetComponent (typeof(PathLineDrawerCS)));
		fPathLength = lineDrawer.fPathLength;
		
		CPPositions = new Vector3[lineDrawer.Parameterized_CPPositions.Length];


		Matrix4x4 mat = new Matrix4x4 ();
		mat.SetTRS (new Vector3(0,0,0),hPatchesRandomizerCS.getCurrentPatch().transform.rotation,new Vector3(1,1,1));
		for(int i=0;i<CPPositions.Length;i++)
		{
			//CPPositions[i] = lineDrawer.Parameterized_CPPositions[i];
			CPPositions[i] = lineDrawer.Parameterized_CPPositions[i];//CPPositions[i].x ;//+ fTotalLength;//PatchNumber * defaultPathLength;
			CPPositions[i] = mat.MultiplyVector(CPPositions[i]) + lineDrawer.pos;

		}

		fTotalLength += hPatchesRandomizerCS.getPatchSize(hPatchesRandomizerCS.getCurrentPatch());

		PatchNumber++;
	}
	
	/*
	*	FUNCTION: Get the CP transforms of the next active patch and store them in array.
	*	USED BY: PatchesRandomizer.Start()
	*/
	public void SetNextPatchCPs()
	{		
		tNextCPsGroup = goNextCPsGroup.transform;
		PathLineDrawerCS lineDrawer = ((PathLineDrawerCS)tNextCPsGroup.GetComponent (typeof(PathLineDrawerCS)));



		fNextPathLength = lineDrawer.fPathLength;
		
		NextCPPositions = new Vector3[lineDrawer.Parameterized_CPPositions.Length];
		Matrix4x4 mat = new Matrix4x4 ();
		mat.SetTRS (new Vector3(0,0,0),hPatchesRandomizerCS.getNextPatch().transform.rotation,new Vector3(1,1,1));
		for(int i=0;i<NextCPPositions.Length;i++)
		{
			//NextCPPositions[i] = lineDrawer.Parameterized_CPPositions[i];			
			NextCPPositions[i] = lineDrawer.Parameterized_CPPositions[i] ;//+ fTotalLength;//PatchNumber * defaultPathLength;
			NextCPPositions[i] = mat.MultiplyVector(NextCPPositions[i]) + lineDrawer.pos;
		}
	}
	
	/*
	*	FUNCTION: Gets the vector position from the location on the spline.
	*	
	*	PARAMETER 1: The array of CPs of the patch.
	*	PATAMETER 2: Location on spline.
	*/
	//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
	private Vector3 Interp(Vector3[] pts , float t)
	{
		t = Mathf.Clamp(t,0.0f,2.0f);
		int numSections  = pts.Length - 3;
		int currPt  = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
		float u = t * numSections - currPt;
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
	}
	
	/*
	*	FUNCTION: Set distance on patch based on the spline.
	*	CALLED BY: ControllerScript.SetTransform()
	*/
	public float SetNextMidPointandRotation(float CurrentDistanceOnPath, float CurrentForwardSpeed)
	{
		CurrentPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/fPathLength;
		
		if(CurrentPercent>=1.0)//if the Player has crossed the current patch
		{
			float PreviousPathLength = fPathLength;
			((PatchesRandomizerCS)GameObject.Find("Player").GetComponent(typeof(PatchesRandomizerCS) )).createNewPatch();
			
			CurrentDistanceOnPath = (CurrentDistanceOnPath+CurrentForwardSpeed) - PreviousPathLength;
			CurrentDistanceOnPath = Mathf.Abs(CurrentDistanceOnPath);
			hControllerScriptCS.setCurrentDistanceOnPath(CurrentDistanceOnPath);
			SetCurrentPatchCPs();
			SetNextPatchCPs();
			
			CurrentPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/fPathLength;
		}
		
		Vector3 MidPointVector3 = Interp(CPPositions,CurrentPercent);
	
		Current_MidPoint.x = MidPointVector3.x;
		Current_MidPoint.y = MidPointVector3.z;
		
		Vector3 ForwardPointVector3 = Interp(CPPositions,CurrentPercent+0.001f);
		Vector3 BackPointVector3 = Interp(CPPositions,CurrentPercent-0.001f);
		CurrentDir = ForwardPointVector3 - BackPointVector3;
		CurrentAngle = PosAngleofVector(CurrentDir);
		if(CurrentAngle>180.0f)
			CurrentAngle = CurrentAngle-360.0f;
			
		return (CurrentDistanceOnPath+CurrentForwardSpeed);
	}

	public GameObject getBossPatch(){
		return bosspatch;
	}

	public float setBossNextMidPointandRotation(float CurrentDistanceOnPath, float CurrentForwardSpeed)
	{



		Vector3 pos;
		BossPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/fPathLength;
		Vector3[] poss;
		Vector3 ForwardPointVector3;
		Vector3 BackPointVector3;
		if (BossPercent >= 1.0) {//if the Player has crossed the current patch
						float PreviousPathLength = fPathLength;
			
						CurrentDistanceOnPath = (CurrentDistanceOnPath + CurrentForwardSpeed) - PreviousPathLength;
						CurrentDistanceOnPath = Mathf.Abs (CurrentDistanceOnPath);
			
			BossPercent = (CurrentDistanceOnPath + CurrentForwardSpeed) / hPatchesRandomizerCS.getNextPathLength();
			poss = NextCPPositions;
			pos = getNextWSPointBasedOnPercent(BossPercent);
			ForwardPointVector3 = getNextWSPointBasedOnPercent(BossPercent+0.001f);
			BackPointVector3 = getNextWSPointBasedOnPercent(BossPercent-0.001f);
			bosspatch = hPatchesRandomizerCS.getNextPatch();
				} else {
			poss = CPPositions;
			pos = getCurrentWSPointBasedOnPercent(BossPercent);
			ForwardPointVector3 = getCurrentWSPointBasedOnPercent(BossPercent+0.001f);
			BackPointVector3 = getCurrentWSPointBasedOnPercent(BossPercent-0.001f);
			bosspatch = hPatchesRandomizerCS.getCurrentPatch();
				}
		Vector3 MidPointVector3 = Interp (poss, BossPercent);
		MidPointVector3 = pos;
		 
		Boss_MidPoint.x = MidPointVector3.x;
		Boss_MidPoint.y = MidPointVector3.z;

		BossDir = ForwardPointVector3 - BackPointVector3;
		BossAngle = PosAngleofVector(BossDir);
		if(BossAngle>180.0f)
			BossAngle = BossAngle-360.0f;
		
		return (CurrentDistanceOnPath+CurrentForwardSpeed);
	}

	public float getCurrentAngle(float percent){
		Vector3 ForwardPointVector3 = getCurrentWSPointBasedOnPercent(percent+0.001f);
		Vector3 BackPointVector3 = getCurrentWSPointBasedOnPercent(percent-0.001f);
		Vector3 dir = ForwardPointVector3 - BackPointVector3;
		float angle = PosAngleofVector(dir) + hPatchesRandomizerCS.getCurrentPatch().transform.eulerAngles.y;
		if(angle>180.0f)
			angle = angle-360.0f;
		return angle ;
	}

	public float getNextAngle(float percent){
		Vector3 ForwardPointVector3 = getNextWSPointBasedOnPercent(percent+0.001f);
		Vector3 BackPointVector3 = getNextWSPointBasedOnPercent(percent-0.001f);
		Vector3 dir = ForwardPointVector3 - BackPointVector3;
		float angle = PosAngleofVector(dir) + hPatchesRandomizerCS.getNextPatch().transform.eulerAngles.y;
		if(angle>180.0f)
			angle = angle-360.0f;
		return angle;
	}
	
	/*
	*	FUNCTION:	Calculate the rotation along y-axis based on the position.
	*	USED BY:	SetNextMidPointandRotation(...)
	*				SetNextMidPointandRotation(...)
	*				getCurrentWSPointBasedOnPercent(...)
	*				getNextWSPointBasedOnPercent(...)
	*	
	*/
	private float PosAngleofVector(Vector3 InputVector)
	{
		float AngleofInputVector = 57.3f * (Mathf.Atan2(InputVector.z,InputVector.x));
		if(AngleofInputVector<0.0f)
			AngleofInputVector = AngleofInputVector + 360.0f;
		return AngleofInputVector;
	}
	
	/*
	*	FUNCTION: The angle the spline makes on the percentile position on current patch.
	*	USED BY: ControllerScript.SetTransform()
	*	INFO: The CurrentAngle is calculated byte the SetNextMidPointandRotation(...) and
	*	returned to the ControllerScript using this getter function in each frame.
	*/
	public float getCurrentAngle()
	{
		return CurrentAngle;
	}

	public float getBossAngle()
	{
		return BossAngle;
	}

	
	/*
	*	FUNCTION: Get the Vector3 position based on area covered on spline of current patch.
	*	USED BY: ElementsGenerator.getPosition(...)
	*	INFO: The spline is assumed to be 1. The starting point is 0.0 and
	*	the end point is 1.0. Any value between 0 and 1 is the percentile position on
	*	the spline. 
	*/
	public Vector3 getCurrentWSPointBasedOnPercent(float Percent_Val)
	{
		Vector3 NextSideDir = new Vector3(0,0,1);
		
		Vector3 ForwardPointVector3 = Interp(CPPositions,(Percent_Val+0.001f));
		Vector3 BackPointVector3 = Interp(CPPositions,(Percent_Val-0.001f));
		Vector3 NextCurrentDir = ForwardPointVector3 - BackPointVector3;
		NextSideDir = RotateY0Vector(NextCurrentDir, 90.0f);
		NextSideDir.Normalize();
		WaypointAngle = PosAngleofVector(NextCurrentDir);
		if(WaypointAngle>180.0f)
			WaypointAngle = WaypointAngle-360.0f;
		
		return Interp(CPPositions,Percent_Val);
	}
	
	/*
	*	FUNCTION: Get the Vector3 position based on area covered on spline of next patch.
	*	USED BY: ElementsGenerator.getPosition(...)
	*	INFO: The spline is assumed to be 1. The starting point is 0.0 and
	*	the end point is 1.0. Any value between 0 and 1 is the percentile position on
	*	the spline. 
	*/
	public Vector3 getNextWSPointBasedOnPercent(float Percent_Val)
	{
		Vector3 NextSideDir = new Vector3(0,0,1);
		
		Vector3 ForwardPointVector3 = Interp(NextCPPositions,(Percent_Val+0.001f));
		Vector3 BackPointVector3 = Interp(NextCPPositions,(Percent_Val-0.001f));
		Vector3 NextCurrentDir = ForwardPointVector3 - BackPointVector3;
		NextSideDir = RotateY0Vector(NextCurrentDir, 90.0f);
		NextSideDir.Normalize();
		WaypointAngle = PosAngleofVector(NextCurrentDir);
		if(WaypointAngle>180.0f)
			WaypointAngle = WaypointAngle-360.0f;
		
		return Interp(NextCPPositions,Percent_Val);
	}
	
	/*
	*	FUNCTION: The angle the spline makes on the percentile position on current patch.
	*	USED BY: ElementsGenerator.generateElements(...)
	*	INFO: The WaypointAngle is calculated int the getNextWSPointBasedOnPercent(...)
	*	function and used by ElementsGenerator whenever an element is placed on the path.
	*/
	public float getWaypointAngle()
	{
		return WaypointAngle;
	}
	
	/*
	*	FUNCTION: Get the current direction of the player on the spline.
	*	USED BY: ControllerScript.SetTransform()
	*/
	public Vector3 getCurrentDirection() { return CurrentDir; }
	
	/*
	*	FUNCTION: Get the exact position under the spline.
	*	USED BY: ControllerScript.calculateHorizontalPosition(...)
	*	INFO: This is used to position the player character in one of the three lanes.
	*/
	public Vector2 getCurrentMidPoint() { return Current_MidPoint; }

	public Vector3 getBossDirection() { return BossDir; }
	
	/*
	*	FUNCTION: Get the exact position under the spline.
	*	USED BY: ControllerScript.calculateHorizontalPosition(...)
	*	INFO: This is used to position the player character in one of the three lanes.
	*/
	public Vector2 getBossMidPoint() { return Boss_MidPoint; }

	
	/*
	*	FUNCTION: Calculate the needed rotation based on spline direction.
	*	USED BY: 	getCurrentWSPointBasedOnPercent(...)
	*				getNextWSPointBasedOnPercent(...)
	*/
	private Vector3 RotateY0Vector(Vector3 inputVector, float angletoRotate)
	{
		Vector3 FinalVector = Vector3.zero;
		angletoRotate = angletoRotate/57.3f;
		FinalVector.x = Mathf.Cos(angletoRotate) * inputVector.x - Mathf.Sin(angletoRotate) * inputVector.z;
		FinalVector.z = Mathf.Sin(angletoRotate) * inputVector.x + Mathf.Cos(angletoRotate) * inputVector.z;
		
		return FinalVector;
	}
}
