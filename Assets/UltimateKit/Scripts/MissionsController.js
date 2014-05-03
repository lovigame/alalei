#pragma strict

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
public class MissionDetail extends System.ValueType
{
	public var missionDescription : String;
	public var missionCount : int;
	public var missionType : MissionTypes;
}

//script references
private var hMenuScript : MenuScript;
private var hInGameScript : InGameScript;
private var hHUDController : HUDController;

//keeps record of all the missions
private var missions : MissionDetail[];//details of all missions
private var iActiveMissionCount:int = 3;//number of missions active at a time (3 by default)
private var iActiveMissions : int[];//keeps record of currently active missions
private var iNextMission : int = 0;//index of the next mission if a current one is completed
private var iTotalMissionCount:int;//the total number available missions

//store the current progress of missions
private var missionsProgress:int[];

function Start ()
{
	iActiveMissionCount = 3;//three missions active at a time by deafult
	missionsProgress = new int[System.Enum.GetValues(MissionTypes).Length];
	
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
	
	hInGameScript = this.GetComponent(InGameScript) as InGameScript;	
	hMenuScript = GameObject.Find("MenuGroup").GetComponent(MenuScript) as MenuScript;
	hHUDController = GameObject.Find("HUDMainGroup").GetComponent(HUDController) as HUDController;

	//get the MissionList file from the resources folder
	var missionsFile = Resources.Load("MissionsList", typeof(TextAsset));
    var stringReader = new StringReader(missionsFile.text);//open the text file
    var fileContents = stringReader.ReadToEnd();//read the text from the text file
    var lines = fileContents.Split("\n"[0]);//tokenize on line seperators
	
	if (lines.Length == 0)//if the file was empty
	{
		Debug.Log("No missions found in file");
		this.enabled = false;
	}
	else//read file and extract mission detail
	{
		var lineIndex:int=0;
		var arrayIndex:int=0;
		iTotalMissionCount = lines.Length/3;
		missions = new MissionDetail[iTotalMissionCount];//allocate memory according to the number of missions
		
		while (lineIndex < lines.Length)//store the file content in mission array
		{
			missions[arrayIndex].missionDescription = lines[lineIndex++];
			missions[arrayIndex].missionCount = int.Parse(lines[lineIndex++]);
			missions[arrayIndex].missionType = System.Enum.Parse(typeof(MissionTypes), lines[lineIndex++]);
		
			arrayIndex++;
		}//end of while
				
		iActiveMissions = new int[iActiveMissionCount];
		for (var i:int=0; i<iActiveMissionCount; i++)//set the currently active missions
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
public function incrementMissionCount(type:MissionTypes)
{
	missionsProgress[type]++;
	checkCompletion();
}

/*
*	FUNCTION:	Increment mission counter by required value.
*/
public function incrementMissionCount(type:MissionTypes, iVal:int)
{
	missionsProgress[type] += iVal;
	checkCompletion();
}

private function checkCompletion()
{
	for (var i:int = 0; i<iActiveMissionCount; i++)//check if an active misson has been completed
	{
		if (missionsProgress[ missions[iActiveMissions[i]].missionType ] >= missions[ iActiveMissions[i] ].missionCount)
			markMissionComplete(i);		
	}//end of for
	
	updateMenuDescriptions();
}

/*
*	FUNCTION:	Compiles all the missions' descriptions and
*				tells the MenuScript to display it on Pause Menu
*				and the Missions Menu
*/
public function updateMenuDescriptions()
{
	var combinedText:String = String.Empty;
	
	//combine all the description text in one string
	for (var i:int=0; i<iActiveMissionCount; i++)
	{		
		combinedText += (i+1).ToString() +". " + missions[ iActiveMissions[i] ].missionDescription
		+ "\n (" + missionsProgress[ missions[iActiveMissions[i]].missionType ] + "/" 
		+ missions[ iActiveMissions[i] ].missionCount + ")\n\n";
	}
	
	//tell the MenuScript.js to update the missions description on Pause Menu
	hMenuScript.updatePauseMenuMissions(combinedText);
	hMenuScript.updateMissionsMenuMissions(combinedText);	
}

/*
*	FUNCTION:	Mark the currently active mission complete, announce on HUD
*				about the completed mission and load the next one.
*	PARAMETER 1:	The index of the completed mission.
*/
private function markMissionComplete(missionIndex:int)
{
	//announce mission completion on HUD
	StartCoroutine(hHUDController.displayMissionDescriptionDropDown(missions[ iActiveMissions[missionIndex] ].missionDescription));
	
	//replace the completed mission with a new one
	iActiveMissions[missionIndex] = getNextMission();
	//reset the new active mission count
	missionsProgress[ missions[iActiveMissions[missionIndex]].missionType ] = 0;
	
	//permenantly save the new active mission
	PlayerPrefs.SetInt("ActiveMission_"+missionIndex, iActiveMissions[missionIndex]);
		
	//update the mission decription on the pause menu and missions menu
	updateMenuDescriptions();	
}

/*
*	FUNCTION:	Check the next mission int the list. Start from the beginning
*				if all missions have been completed.
*/
private function getNextMission()
{
	var tempNext:int = iNextMission;
	
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