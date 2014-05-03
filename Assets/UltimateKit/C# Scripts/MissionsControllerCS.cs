/*
*	FUNCTION:
*	- This script stores all the missions.
*	- It keeps record of the currently active missions. Three missions
*	are active at a single time.
*	- It checks if a currently active mission has been completed.
*	- On completion of one mission, it is automatically replaced by
*	the next one. Once all missions have been completed, the missions
*	are restarted from the first three.
*/

using UnityEngine;
using System.Collections;

public class MissionsControllerCS : MonoBehaviour {

	//the different types of missions
	public enum MissionTypes
	{
		Score,
		Distance,
		Powerups,
		Jump,
		Duck,
		Currency,
		StartGame
	}
	
	//detail of a particular mission
	public class MissionDetail
	{
		public string missionDescription;
		public int missionCount;
		public MissionTypes missionType;
	}
	
	//script references
	private MenuScriptCS hMenuScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
	private InGameScriptCS hInGameScriptCS;
	private HUDControllerCS hHUDControllerCS;
	private NGUIHUDScript hNGUIHUDScript;
	
	//keeps record of all the missions
	private MissionDetail[] missions;//details of all missions
	private int iActiveMissionCount = 3;//number of missions active at a time (3 by default)
	private int[] iActiveMissions;//keeps record of currently active missions
	private int iNextMission = 0;//index of the next mission if a current one is completed
	private int iTotalMissionCount;//the total number available missions
	
	//store the current progress of missions
	private int[] missionsProgress;
	
	void Start ()
	{
		iActiveMissionCount = 3;//three missions active at a time by deafult
		missionsProgress = new int[System.Enum.GetValues(typeof(MissionTypes)).Length];
		
		//set the next mission index
		if (PlayerPrefs.HasKey("NextMissionIndex"))
		{
			iNextMission = PlayerPrefs.GetInt("NextMissionIndex");
		}
		else
		{
			iNextMission = 0;
			PlayerPrefs.SetInt("NextMissionIndex", iNextMission);
		}
		
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		
		if (hInGameScriptCS.isCustomMenuEnabled())
		{
			hMenuScriptCS = (MenuScriptCS)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScriptCS));
			hHUDControllerCS = (HUDControllerCS)GameObject.Find("HUDMainGroup").GetComponent(typeof(HUDControllerCS));
		}
		else
		{
			hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
			hNGUIHUDScript = hNGUIMenuScript.getNGUIHUDScriptReference();
		}
		
		//get the MissionList file from the resources folder
		TextAsset taFile = (TextAsset)Resources.Load("MissionsList");
		string[] lines = taFile.text.Split('\n');
		
		if (lines.Length == 0)//if the file was empty
		{
			Debug.Log("No missions found in file");
			this.enabled = false;
		}
		else//read file and extract mission detail
		{
			int lineIndex=0;
			int arrayIndex=0;
			iTotalMissionCount = lines.Length/3;
			missions = new MissionDetail[iTotalMissionCount];//allocate memory according to the number of missions
			for (int i=0; i<iTotalMissionCount; i++)
				missions[i] = new MissionDetail();
			
			while (lineIndex < lines.Length)//store the file content in mission array
			{
				missions[arrayIndex].missionDescription = lines[lineIndex++];
				missions[arrayIndex].missionCount = int.Parse(lines[lineIndex++]);
				missions[arrayIndex].missionType = (MissionTypes)System.Enum.Parse(typeof(MissionTypes), lines[lineIndex++]);
			
				arrayIndex++;
			}//end of while
					
			iActiveMissions = new int[iActiveMissionCount];
			for (int i=0; i<iActiveMissionCount; i++)//set the currently active missions
			{
				if (PlayerPrefs.HasKey("ActiveMission_"+i.ToString()))
					iActiveMissions[i] = PlayerPrefs.GetInt("ActiveMission_"+i.ToString());
				else
				{				
					iActiveMissions[i] = getNextMission();
					PlayerPrefs.SetInt("ActiveMission_"+i.ToString(), iActiveMissions[i]);
				}
			}//end of for
			
			updateMenuDescriptions();
			
		}//end of else
		
		PlayerPrefs.Save();
	}
	
	/*
	*	FUNCTION:	Increment mission counter by 1.
	*/
	public void incrementMissionCount(MissionTypes type)
	{
		missionsProgress[(int)type]++;
		checkCompletion();
	}
	
	/*
	*	FUNCTION:	Increment mission counter by required value.
	*/
	public void incrementMissionCount(MissionTypes type, int iVal)
	{
		missionsProgress[(int)type] += iVal;
		checkCompletion();
	}
	
	private void checkCompletion()
	{
		for (int i = 0; i<iActiveMissionCount; i++)//check if an active misson has been completed
		{
			if (missionsProgress[ (int)missions[iActiveMissions[i]].missionType ] >= missions[ iActiveMissions[i] ].missionCount)
				markMissionComplete(i);		
		}//end of for
		
		updateMenuDescriptions();
	}
	
	/*
	*	FUNCTION:	Compiles all the missions' descriptions and
	*				tells the MenuScript to display it on Pause Menu
	*				and the Missions Menu
	*/
	public void updateMenuDescriptions()
	{
		string combinedText = string.Empty;
		
		//combine all the description text in one string
		for (int i=0; i<iActiveMissionCount; i++)
		{		
			combinedText += (i+1).ToString() +". " + missions[ iActiveMissions[i] ].missionDescription
			+ "\n (" + missionsProgress[ (int)missions[iActiveMissions[i]].missionType ] + "/" 
			+ missions[ iActiveMissions[i] ].missionCount + ")\n\n";
		}
		
		//tell the MenuScript.js to update the missions description on Pause Menu
		if (hInGameScriptCS.isCustomMenuEnabled())
		{
			hMenuScriptCS.updatePauseMenuMissions(combinedText);
			hMenuScriptCS.updateMissionsMenuMissions(combinedText);
		}
		else//if the NGUI menus are in use
		{
			hNGUIMenuScript.updatePauseMenuMissions(combinedText);
			hNGUIMenuScript.updateMissionsMenuMissions(combinedText);
		}
	}
	
	/*
	*	FUNCTION:	Mark the currently active mission complete, announce on HUD
	*				about the completed mission and load the next one.
	*	PARAMETER 1:	The index of the completed mission.
	*/
	private void markMissionComplete(int missionIndex)
	{
		//announce mission completion on HUD
		if (hInGameScriptCS.isCustomMenuEnabled())
			StartCoroutine(hHUDControllerCS.displayMissionDescriptionDropDown("DONE!\n" + missions[ iActiveMissions[missionIndex] ].missionDescription));
		else
			StartCoroutine(hNGUIHUDScript.displayMissionDescriptionDropDown("DONE!\n" + missions[ iActiveMissions[missionIndex] ].missionDescription));
		
		//replace the completed mission with a new one
		iActiveMissions[missionIndex] = getNextMission();
		//reset the new active mission count
		missionsProgress[ (int)missions[iActiveMissions[missionIndex]].missionType ] = 0;
		
		//permenantly save the new active mission
		PlayerPrefs.SetInt("ActiveMission_"+missionIndex, iActiveMissions[missionIndex]);
			
		//update the mission decription on the pause menu and missions menu
		updateMenuDescriptions();	
	}
	
	/*
	*	FUNCTION:	Check the next mission int the list. Start from the beginning
	*				if all missions have been completed.
	*/
	private int getNextMission()
	{
		int tempNext = iNextMission;
		
		if ( (iNextMission+1) == iTotalMissionCount)//if all missions completed, restart mission list
		{
			iNextMission = 0;
			PlayerPrefs.SetInt("NextMissionIndex", iNextMission);	
		}
		else//return next mission's index located in the 'missions' array
		{
			iNextMission++;
			PlayerPrefs.SetInt("NextMissionIndex", iNextMission);		
		}
		
		PlayerPrefs.Save();
		return tempNext;
	}
}
