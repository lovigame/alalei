#pragma strict
/*
*	FUNCTION:
*	- Controls the HUD display which includes the score, currency and distance notifications.
*	
*	USED BY: This script is a part of the "Player" prefab.
*
*/

private var tPlayer : Transform;	//player transfrom

//script references
private var hInGameScript : InGameScript;
private var hMissionsController : MissionsController;
private var hGlobalAchievementController : GlobalAchievementController;
private var hPowerupsMainController : PowerupsMainController;
private var hControllerScript : ControllerScript;

private var tmHUDCurrencyText : TextMesh;
private var tmHUDScoreText : TextMesh;
private var tHUDScoreContainerMid : Transform;
private var tHUDCurrencyContainerMid : Transform;

//Calculate Score
private var fPreviousDistance : float = 0.0;	//mileage in the last frame
private var fCurrentDistance : float = 0.0;		//mileage in the current frame
private var fCurrentTime : float = 0.0;
private var fPreviousTime : float = 0.0;

//Distance covered notification
private var fDistanceNotification : float = 500;//distance after which notification will be shown
private var iDistanceNotifState : int = 0;
private var tDistanceNotification : Transform;
private var tmDistanceNotif : TextMesh;

//HUD element Container sizes
private var iDivisorScore : int;
private var iDivisorCurrency : int;
private var iDivisorMultiplier : int;

//HUD mission description drop down
private var tMissionDropDown : Transform;
private var missionDescription : TextMesh;

function Start()
{		
	tPlayer = this.transform;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;	
	hControllerScript = GameObject.Find("Player").GetComponent(ControllerScript) as ControllerScript;
	hMissionsController = GameObject.Find("Player").GetComponent(MissionsController) as MissionsController;
	hGlobalAchievementController = GameObject.Find("Player").GetComponent(GlobalAchievementController) as GlobalAchievementController;
	hPowerupsMainController = GameObject.Find("Player").GetComponent(PowerupsMainController) as PowerupsMainController;

	tmHUDCurrencyText = GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup/HUD_Currency_Text").GetComponent("TextMesh") as TextMesh;
	tmHUDScoreText = GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup/HUD_Score_Text").GetComponent("TextMesh") as TextMesh;
		
	tHUDScoreContainerMid = GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup/HUD_Score_BG").GetComponent(Transform) as Transform;	//	HUD Score Container	
	tHUDCurrencyContainerMid = GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup/HUD_Currency_BG").GetComponent(Transform) as Transform;	//	HUD Currency Container
	
	tMissionDropDown = this.transform.Find("HUDGroup/MissionNotifier");
	missionDescription = tMissionDropDown.Find("Text_MissionDescription").GetComponent("TextMesh") as TextMesh;
		
	//get time difference to calculate score
	fCurrentTime = Time.time;
	fPreviousTime = Time.time;
	
	fPreviousDistance = 0;
	fCurrentDistance = 0;
	fCurrentTime = 0;
	fPreviousTime = 0;
	
	iDivisorScore = 10;
	iDivisorCurrency = 10;
	iDivisorMultiplier = 10;
	
	tHUDScoreContainerMid.localScale.z = 0.45;
	tHUDCurrencyContainerMid.localScale.z = 0.45;
	
	//Distance Notification
	tmDistanceNotif = GameObject.Find("HUDMainGroup/HUDGroup/DistanceNotifier/Text_Distance").GetComponent("TextMesh") as TextMesh;
	tDistanceNotification = GameObject.Find("HUDMainGroup/HUDGroup/DistanceNotifier").GetComponent(Transform) as Transform;
	tDistanceNotification.gameObject.SetActive(false);
	
	//call the resize Dight Container function every .5 seconds
	InvokeRepeating("resizeDigitContainer", 1, 0.5);
	resizeDigitContainer();
}

function FixedUpdate()
{	
	if(hInGameScript.isGamePaused()==true)
		return;

	UpdateHUDStats();
	
	//show distance notification after covering 500 meters
	if (iDistanceNotifState == 0
	&& (Mathf.Round(fCurrentDistance) % fDistanceNotification) == 0 
	&& fCurrentDistance != 0)
	{		
		StartCoroutine("displayDistanceNotificaiton");
	}
}//end of Update

/*
* 	FUNCTION: The score is calculated and added up in Level_Score variable
*	CALLED BY:	FixedUpdate()
*/
private function UpdateHUDStats()
{	
	yield WaitForEndOfFrame();
	
	//skip time and check the difference in milage in the duration
	if ( (fCurrentTime - fPreviousTime) >= 0.1 )
	{
		var iCurrentFrameScore = (fCurrentDistance - fPreviousDistance);
		hInGameScript.incrementLevelScore(iCurrentFrameScore);
		
		hMissionsController.incrementMissionCount(MissionTypes.Score, iCurrentFrameScore);//mission score counter
		hGlobalAchievementController.incrementAchievementCount(GlobalAchievementTypes.Score, iCurrentFrameScore);//global achievements counter
		
		fPreviousDistance = fCurrentDistance;
		fCurrentDistance = hControllerScript.getCurrentMileage();
		
		hMissionsController.incrementMissionCount(MissionTypes.Distance);//mission milage counter
		hGlobalAchievementController.incrementAchievementCount(GlobalAchievementTypes.Distance);//global achievements counter
		
		fPreviousTime = fCurrentTime;
		fCurrentTime = Time.time;
	}
	else
	{
		fCurrentDistance = hControllerScript.getCurrentMileage();	//get the current mileage
		fCurrentTime = Time.time;
	}	
		
	tmHUDCurrencyText.text = hPowerupsMainController.getCurrencyUnits().ToString();	//update Currency on HUD
	tmHUDScoreText.text = hInGameScript.getLevelScore().ToString();	//update Score on HUD		
}

/*
*	FUNCTION: 	Show the distance covered in meters after every 'x' meters
*				defined by fNotificationDistance.
*	CALLED BY:	FixedUpdate()
*/
private function displayDistanceNotificaiton()
{
	while (true)
	{
		yield WaitForFixedUpdate();
		
		if (iDistanceNotifState == 0)
		{			
			tDistanceNotification.gameObject.SetActive(true);
			tmDistanceNotif.text = Mathf.Round(fCurrentDistance).ToString();
			tDistanceNotification.localScale = Vector3(0,0,0);
			
			iDistanceNotifState = 1;
		}
		else if (iDistanceNotifState == 1)
		{
			tDistanceNotification.localScale = Vector3.Lerp(tDistanceNotification.localScale, Vector3(1.79,1.79,1), Time.deltaTime*2.5);
			
			if (tDistanceNotification.localScale.x >= 1.65)
				iDistanceNotifState = 2;
		}
		else if (iDistanceNotifState == 2)
		{			
			tDistanceNotification.gameObject.SetActive(false);
			tmDistanceNotif.fontSize = 80;
			
			iDistanceNotifState = 3;			
			break;
		}
		else if (iDistanceNotifState == 3)
		{
			StopCoroutine("displayDistanceNotificaiton");
			iDistanceNotifState = 0;
			break;
		}
	}//end of while
}

/*
*	FUNCTION:	Display the drop down with the completed mission description
*	CALLED BY: MissionsController.markMissionComplete(...)
*/
private var missionDropDownState:int = -1;
public function displayMissionDescriptionDropDown(description:String)
{
	missionDropDownState = 0;
	missionDescription.text = description;
	
	while (true)
	{
		yield WaitForFixedUpdate();
		
		if (missionDropDownState == 0)//show the drop down
		{
			tMissionDropDown.position.y = Mathf.Lerp(tMissionDropDown.position.y, -1, Time.deltaTime*2.5);
			
			if (tMissionDropDown.position.y <= -0.9)
				missionDropDownState = 1;
		}
		else if (missionDropDownState == 1)//hide the drop down
		{
			tMissionDropDown.position.y = Mathf.Lerp(tMissionDropDown.position.y, 23, Time.deltaTime*4.5);
			
			if (tMissionDropDown.position.y >= 22)
				missionDropDownState = 2;
		}
		else if (missionDropDownState == 2)//stop the coroutine
		{
			missionDropDownState = -1;
			StopCoroutine("displayMissionDescriptionDropDown");
			break;
		}		
	}//end of while
}

/*
*	FUNCTION: Resize HUD Score and Currency containers according to digit count
*	CALLED BY:	Start() (invoke repeating)
*/
private function resizeDigitContainer()
{
	var fScore : int = hInGameScript.getLevelScore();
	var fCurrency : int = hPowerupsMainController.getCurrencyUnits();
		
	if ( (fScore / iDivisorScore) >= 1 )
	{
		tHUDScoreContainerMid.localScale.z += 0.4;	//expand the Score Container Mid
		iDivisorScore *= 10;
	}
	
	if ( (fCurrency / iDivisorCurrency) >= 1 )
	{
		tHUDCurrencyContainerMid.localScale.z += 0.4;		//expand the Currency Container Mid
		iDivisorCurrency *= 10;
	}
}