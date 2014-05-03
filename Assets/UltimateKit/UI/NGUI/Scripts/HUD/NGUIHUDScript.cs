/*
*	FUNCTION:
*	- Controls the HUD display which includes the score, currency and distance notifications.
*	
*	USED BY: This script is a part of the "Player" prefab.
*
*/

using UnityEngine;
using System.Collections;

public class NGUIHUDScript : MonoBehaviour {
	
	//script references
	private InGameScriptCS hInGameScriptCS;
	private MissionsControllerCS hMissionsControllerCS;
	private GlobalAchievementControllerCS hGlobalAchievementControllerCS;
	private PowerupsMainControllerCS hPowerupsMainControllerCS;
	private ControllerScriptCS hControllerScriptCS;
	
	//HUD components for NGUI menu	
	private UILabel uilScoreText;//the score on HUD
	private UILabel uilCurrencyText;	//the currency on HUD
		
	private Transform tMissionDropDown;//drop down displayed on completing a mission
	private UILabel uilMissionDescription;//description of the mission in mission drop down
	
	//Calculate Score
	private float fPreviousDistance = 0.0f;	//mileage in the last frame
	private float fCurrentDistance = 0.0f;		//mileage in the current frame
	private float fCurrentTime = 0.0f;
	private float fPreviousTime = 0.0f;
	
	//Distance covered notification
	private float fDistanceNotification = 500;//distance after which notification will be shown
	private int iDistanceNotifState = 0;
	private GameObject goDistanceNotification;
	private Vector3 distanceNotifDefaultPosition;
	private UILabel uilDistanceNotif;	//the distance notification value
	
	void Start () 
	{			
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));
		hMissionsControllerCS = (MissionsControllerCS)GameObject.Find("Player").GetComponent(typeof(MissionsControllerCS));
		hGlobalAchievementControllerCS = (GlobalAchievementControllerCS)GameObject.Find("Player").GetComponent(typeof(GlobalAchievementControllerCS));
		hPowerupsMainControllerCS = (PowerupsMainControllerCS)GameObject.Find("Player").GetComponent(typeof(PowerupsMainControllerCS));
							
		uilScoreText = (UILabel)this.transform.Find("ScoreGroup/Text_Score").GetComponent(typeof(UILabel));//the score text on HUD
		uilCurrencyText = (UILabel)this.transform.Find("CurrencyGroup/Text_Currency").GetComponent(typeof(UILabel));//the currency text on HUD
		
		goDistanceNotification = GameObject.Find("UI Root (2D)/Camera/Anchor/HUDGroup/DistanceNotifier");//the distance notification group
		uilDistanceNotif = (UILabel)this.transform.Find("DistanceNotifier/Text_Distance").GetComponent(typeof(UILabel));//the text in the distance notification group
		distanceNotifDefaultPosition = goDistanceNotification.transform.position;//get the default position of the distance notification meter
		goDistanceNotification.transform.position = new Vector3(distanceNotifDefaultPosition.x,1000,distanceNotifDefaultPosition.z);//remove the distance notification from camera
		
		tMissionDropDown = (Transform)this.transform.Find("MissionNotifier").GetComponent(typeof(Transform));//the mission description drop down
		uilMissionDescription = (UILabel)this.transform.Find("MissionNotifier/Text_MissionDescription").GetComponent(typeof(UILabel));
		
		//get time difference to calculate score
		fCurrentTime = Time.time;
		fPreviousTime = Time.time;
		
		fPreviousDistance = 0;
		fCurrentDistance = 0;
		fCurrentTime = 0;
		fPreviousTime = 0;
	}//end of Start
	
	void FixedUpdate () 
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
	}//end of Fixed Update
	
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
				
		uilScoreText.text = hInGameScriptCS.getLevelScore().ToString();	//update Score on HUD
		uilCurrencyText.text = hPowerupsMainControllerCS.getCurrencyUnits().ToString();	//update Currency on HUD		
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
				//set the position of the distance notificaiton component to default
				goDistanceNotification.transform.position = distanceNotifDefaultPosition;				
					
				uilDistanceNotif.text = Mathf.Round(fCurrentDistance).ToString();
				goDistanceNotification.transform.localScale = new Vector3(0,0,0);
				
				iDistanceNotifState = 1;
			}
			else if (iDistanceNotifState == 1)//display the component in front of camera
			{
				goDistanceNotification.transform.localScale = Vector3.Lerp(goDistanceNotification.transform.localScale, new Vector3(1.79f,1.79f,1), Time.deltaTime*2.5f);
				
				if (goDistanceNotification.transform.localScale.x >= 1.65f)
					iDistanceNotifState = 2;
			}
			else if (iDistanceNotifState == 2)//hide the component
			{	
				goDistanceNotification.transform.position = new Vector3(distanceNotifDefaultPosition.x,1000,distanceNotifDefaultPosition.z);
								
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
	*	FUNCTION:	Display the drop down with the completed mission's description.
	*	CALLED BY:	MissionControllerCS.markMissionComplete(...)
	*/
	private int missionDropDownState = -1;
	public IEnumerator displayMissionDescriptionDropDown(string description)
	{
		missionDropDownState = 0;
		uilMissionDescription.text = description;
		
		while (true)
		{
			yield return new WaitForFixedUpdate();
			
			if (missionDropDownState == 0)//show the drop down
			{
				tMissionDropDown.localPosition = new Vector3(tMissionDropDown.localPosition.x,
					Mathf.Lerp(tMissionDropDown.localPosition.y, 0, Time.deltaTime*2.5f), tMissionDropDown.localPosition.z);
								
				if (tMissionDropDown.localPosition.y <= 1)
					missionDropDownState = 1;
			}
			else if (missionDropDownState == 1)//hide the drop down
			{				
				tMissionDropDown.localPosition = new Vector3(tMissionDropDown.localPosition.x, 
					Mathf.Lerp(tMissionDropDown.localPosition.y, 100, Time.deltaTime*4.5f), tMissionDropDown.localPosition.z);
				
				if (tMissionDropDown.localPosition.y >= 99)
					missionDropDownState = 2;
			}
			else if (missionDropDownState == 2)//stop the coroutine
			{
				missionDropDownState = -1;
				StopCoroutine("displayMissionDescriptionDropDown");
				break;
			}		
		}//end of while
	}//end of display mission description drop down function
	
	/*
	 * FUNCTION:	Disable unneeded compnents when the HUD Group is enabled
	 * 				or disabled.
	 * CALLED BY:	NGUIMenuScript.Start()
	 * 				InGameScriptCS.Update()
	 * */
	public void toggleHUDGroupState(bool state)
	{		
	}//end of toggle HUD Group State function
}
