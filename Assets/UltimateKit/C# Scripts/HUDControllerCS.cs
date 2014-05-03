/*
*	FUNCTION:
*	- Controls the HUD display which includes the score, currency and distance notifications.
*	
*	USED BY: This script is a part of the "Player" prefab.
*
*/

using UnityEngine;
using System.Collections;

public class HUDControllerCS : MonoBehaviour {
	
	private GameObject goHUDGroup;

	//script references
	private InGameScriptCS hInGameScriptCS;
	private MissionsControllerCS hMissionsControllerCS;
	private GlobalAchievementControllerCS hGlobalAchievementControllerCS;
	private PowerupsMainControllerCS hPowerupsMainControllerCS;
	private ControllerScriptCS hControllerScriptCS;
	
	//HUD componenets for custom menu
	private TextMesh tmHUDCurrencyText;
	private TextMesh tmHUDScoreText;
	private Transform tHUDScoreContainerMid;
	private Transform tHUDCurrencyContainerMid;
	
	//HUD components for NGUI menu	
	private UILabel uilScoreText;
	private UILabel uilCurrencyText;	
	private UILabel uilDistanceNotif;	
	private UILabel uilMissionDescription;
	
	//Calculate Score
	private float fPreviousDistance = 0.0f;	//mileage in the last frame
	private float fCurrentDistance = 0.0f;		//mileage in the current frame
	private float fCurrentTime = 0.0f;
	private float fPreviousTime = 0.0f;
	
	//Distance covered notification
	private float fDistanceNotification = 500;//distance after which notification will be shown
	private int iDistanceNotifState = 0;
	private Transform tDistanceNotification;
	private TextMesh tmDistanceNotif;
	
	//HUD element Container sizes
	private int iDivisorScore;
	private int iDivisorCurrency;
	private int iDivisorMultiplier;
	
	//HUD mission description drop down
	private Transform tMissionDropDown;
	private TextMesh missionDescription;
	
	void Start()
	{			
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));
		hMissionsControllerCS = (MissionsControllerCS)GameObject.Find("Player").GetComponent(typeof(MissionsControllerCS));
		hGlobalAchievementControllerCS = (GlobalAchievementControllerCS)GameObject.Find("Player").GetComponent(typeof(GlobalAchievementControllerCS));
		hPowerupsMainControllerCS = (PowerupsMainControllerCS)GameObject.Find("Player").GetComponent(typeof(PowerupsMainControllerCS));
	
		tMissionDropDown = this.transform.Find("HUDGroup/MissionNotifier");
		missionDescription = tMissionDropDown.Find("Text_MissionDescription").GetComponent("TextMesh") as TextMesh;
		tmHUDCurrencyText = GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup/HUD_Currency_Text").GetComponent("TextMesh") as TextMesh;
		tmHUDScoreText = GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup/HUD_Score_Text").GetComponent("TextMesh") as TextMesh;				
		tHUDScoreContainerMid = (Transform)GameObject.Find("HUDMainGroup/HUDGroup/HUDScoreGroup/HUD_Score_BG").GetComponent(typeof(Transform));	//	HUD Score Container	
		tHUDCurrencyContainerMid = (Transform)GameObject.Find("HUDMainGroup/HUDGroup/HUDCurrencyGroup/HUD_Currency_BG").GetComponent(typeof(Transform));	//	HUD Currency Container
		tHUDScoreContainerMid.localScale = new Vector3(tHUDScoreContainerMid.localScale.x, tHUDScoreContainerMid.localScale.y, 0.45f);
		tHUDCurrencyContainerMid.localScale = new Vector3(tHUDCurrencyContainerMid.localScale.x, tHUDCurrencyContainerMid.localScale.y, 0.45f);
					
		//Distance Notification
		tmDistanceNotif = GameObject.Find("HUDMainGroup/HUDGroup/DistanceNotifier/Text_Distance").GetComponent("TextMesh") as TextMesh;
		tDistanceNotification = (Transform)GameObject.Find("HUDMainGroup/HUDGroup/DistanceNotifier").GetComponent(typeof(Transform));
		tDistanceNotification.gameObject.SetActive(false);
						
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
		
		//call the resize Dight Container function every .5 seconds
		InvokeRepeating("resizeDigitContainer", 1, 0.5f);
		resizeDigitContainer();
	}
	
	void FixedUpdate()
	{	
		if(hInGameScriptCS.isGamePaused()==true)
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
	private void UpdateHUDStats()
	{	
		//skip time and check the difference in milage in the duration
		if ( (fCurrentTime - fPreviousTime) >= 0.1f )
		{
			//calculate the score
			float iCurrentFrameScore = (fCurrentDistance - fPreviousDistance);
			hInGameScriptCS.incrementLevelScore((int)iCurrentFrameScore);
			
			hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.Score, (int)iCurrentFrameScore);//mission score counter
			hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.Score, (int)iCurrentFrameScore);//global achievements counter
			
			fPreviousDistance = fCurrentDistance;
			fCurrentDistance = hControllerScriptCS.getCurrentMileage();
			
			hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.Distance);//mission milage counter
			hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.Distance);//global achievements counter
			
			fPreviousTime = fCurrentTime;
			fCurrentTime = Time.time;
		}
		else
		{
			fCurrentDistance = hControllerScriptCS.getCurrentMileage();	//get the current mileage
			fCurrentTime = Time.time;
		}	
		
		//display the currency and score on the HUD
		tmHUDCurrencyText.text = hPowerupsMainControllerCS.getCurrencyUnits().ToString();	//update Currency on HUD
		tmHUDScoreText.text = hInGameScriptCS.getLevelScore().ToString();	//update Score on HUD
	}//end of Update HUD Stats function
	
	/*
	*	FUNCTION: 	Show the distance covered in meters after every 'x' meters
	*				defined by fNotificationDistance.
	*	CALLED BY:	FixedUpdate()
	*/
	private IEnumerator displayDistanceNotificaiton()
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			
			if (iDistanceNotifState == 0)//enable and update component
			{	
				NGUITools.SetActive(tDistanceNotification.gameObject, true);
					
				tmDistanceNotif.text = Mathf.Round(fCurrentDistance).ToString();
				tDistanceNotification.localScale = new Vector3(0,0,0);
				
				iDistanceNotifState = 1;
			}
			else if (iDistanceNotifState == 1)//display the component in front of camera
			{
				tDistanceNotification.localScale = Vector3.Lerp(tDistanceNotification.localScale, new Vector3(1.79f,1.79f,1), Time.deltaTime*2.5f);
				
				if (tDistanceNotification.localScale.x >= 1.65f)
					iDistanceNotifState = 2;
			}
			else if (iDistanceNotifState == 2)//hide the component
			{	
				NGUITools.SetActive(tDistanceNotification.gameObject, false);
								
				iDistanceNotifState = 3;			
				break;
			}
			else if (iDistanceNotifState == 3)//stop the coroutine
			{
				StopCoroutine("displayDistanceNotificaiton");
				iDistanceNotifState = 0;
				break;
			}
		}//end of while
	}
	
	/*
	*	FUNCTION:	Display the drop down with the completed mission description
	*/
	private int missionDropDownState = -1;
	public IEnumerator displayMissionDescriptionDropDown(string description)
	{
		missionDropDownState = 0;
		missionDescription.text = description;
		
		while (true)
		{
			yield return new WaitForFixedUpdate();
			
			if (missionDropDownState == 0)//show the drop down
			{
				tMissionDropDown.position = new Vector3(tMissionDropDown.position.x,
					Mathf.Lerp(tMissionDropDown.position.y, -1, Time.deltaTime*2.5f), tMissionDropDown.position.z);
								
				if (tMissionDropDown.position.y <= -0.9f)
					missionDropDownState = 1;
			}
			else if (missionDropDownState == 1)//hide the drop down
			{				
				tMissionDropDown.position = new Vector3(tMissionDropDown.position.x, 
					Mathf.Lerp(tMissionDropDown.position.y, 23, Time.deltaTime*4.5f), tMissionDropDown.position.z);
				
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
	private void resizeDigitContainer()
	{
		int fScore = hInGameScriptCS.getLevelScore();
		int fCurrency = hPowerupsMainControllerCS.getCurrencyUnits();
			
		if ( (fScore / iDivisorScore) >= 1 )
		{			
			tHUDScoreContainerMid.localScale += new Vector3(0,0,0.4f);//expand the Score Container Mid
			iDivisorScore *= 10;
		}
		
		if ( (fCurrency / iDivisorCurrency) >= 1 )
		{			
			tHUDCurrencyContainerMid.localScale += new Vector3(0,0,0.4f);//expand the Currency Container Mid
			iDivisorCurrency *= 10;
		}
	}
}
