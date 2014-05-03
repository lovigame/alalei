/*
*	FUNCTION:
*	- This script keeps track of the global/ game center achievements.
*	- All of the achievements if not completed are kept active.
*	- Counters funcitons are called in every relevant script to track the
*	progress of the achievements.
*/

using UnityEngine;
using System.Collections;

public class GlobalAchievementControllerCS : MonoBehaviour {

	//the types of Global Achievements
	public enum GlobalAchievementTypes
	{
		Score,
		Distance,
		Powerups,
		Jump,
		Duck,
		Currency,
		StartGame
	}
	
	public struct GlobalAchievementDetail
	{
		public string achievementDescription;//description of the achievement
		public int achievementCount;			//number of actions to perform to complete the achievement
		public GlobalAchievementTypes achievementType;//what actions to perform (jump, collect currency etc.)
		public bool achievementComplete;	//if the achievement has been completed
	}
	
	//script references
	private MenuScriptCS hMenuScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
	private InGameScriptCS hInGameScriptCS;
	
	private int iTotalAchievementsCount;//amount of available achievements
	private GlobalAchievementDetail[] achievements;//information of all the achievements
	
	private int[] achievementsProgress;//store the current progress of achievements
	
	void Start ()
	{
		achievementsProgress = new int[System.Enum.GetValues(typeof(GlobalAchievementTypes)).Length];
			
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		if (hInGameScriptCS.isCustomMenuEnabled())
			hMenuScriptCS = (MenuScriptCS)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScriptCS));
		else
			hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
		
		//get the GlobalAchievementsList file from the resources folder		
		TextAsset taFile = (TextAsset)Resources.Load("GlobalAchievementsList");
		string[] lines = taFile.text.Split('\n');
	    
	    if (lines.Length == 0)//if the file was empty
		{
			Debug.Log("No achievements found in file");
			this.enabled = false;
		}
		else//read file and extract achievements detail
		{
			int lineIndex=0;
			int arrayIndex=0;
			iTotalAchievementsCount = lines.Length/3;//get the total number of achievements in the file
			achievements = new GlobalAchievementDetail[iTotalAchievementsCount];//allocate memory according to the number of achievement
			/*for (var i=0; i<iTotalAchievementsCount; i++)
				achievements[i] = new GlobalAchievementDetail();*/
			
			while (lineIndex < lines.Length)//store the file content in achievement array
			{
				achievements[arrayIndex].achievementDescription = lines[lineIndex++].ToString();
				achievements[arrayIndex].achievementCount = int.Parse(lines[lineIndex++].ToString());
				achievements[arrayIndex].achievementType = (GlobalAchievementTypes)System.Enum.Parse(typeof(GlobalAchievementTypes), lines[lineIndex++].ToString());
				achievements[arrayIndex].achievementComplete = false;	//mark achievement incomplete by default
							
				if (PlayerPrefs.HasKey("GlobalAchievement_"+arrayIndex))//check achievement progress
				{				
					achievementsProgress[ (int)achievements[arrayIndex].achievementType ] = PlayerPrefs.GetInt("GlobalAchievement_"+arrayIndex);
					
					//check if the achievement has been completed
					if (achievementsProgress[ (int)achievements[arrayIndex].achievementType ] >= achievements[arrayIndex].achievementCount)
						achievements[arrayIndex].achievementComplete = true;
				}
				else//if this is the first game launch
				{
					PlayerPrefs.SetInt("GlobalAchievement_"+arrayIndex, 0);
					achievementsProgress[ (int)achievements[arrayIndex].achievementType ] = 0;
				}
				
				arrayIndex++;
			}//end of while
					
			updateMenuDescription();
			
		}//end of else
		
		PlayerPrefs.Save();
	}
	
	/*
	*	FUNCTION:	Increment achievement counter by 1.
	*/
	public void incrementAchievementCount(GlobalAchievementTypes type)
	{ 
		achievementsProgress[(int)type]++;
		checkCompletion(type);
	}
	
	/*
	*	FUNCTION:	Increment achievement counter by required value.
	*/
	public void incrementAchievementCount(GlobalAchievementTypes type, int iVal)
	{
		achievementsProgress[(int)type]+= iVal;
		checkCompletion(type);
	}
	
	/*
	*	FUNCTION:	Check if an achievement has been completed. Save the progress if
	*				changes have been made in the achievement progress.
	*	CALLED BY:	incrementAchievementCount(...)
	*/
	private void checkCompletion(GlobalAchievementTypes achievementType)
	{	
		for (int i = 0; i<iTotalAchievementsCount; i++)
		{
			if (achievements[i].achievementType == achievementType	//is the updated counter of the current achievement
			&& achievements[i].achievementComplete == false)	//has the achievement been completed previously
			{
				if (achievementsProgress[(int)achievementType] >= achievements[i].achievementCount)//has the achievement been completed
					achievements[i].achievementComplete = true;	//mark achievement as complete
				
				PlayerPrefs.SetInt("GlobalAchievement_"+i, achievementsProgress[(int)achievementType]);//save progress permanently	
			}//end of outer if
		}//end of for	
	}//end of check completion function
	
	/*
	*	FUNCTION:	Compiles all the achievements' descriptions and
	*				tells the MenuScript to display it on Pause Menu
	*				and the Missions Menu
	*/
	private void updateMenuDescription()
	{
		string combinedText = string.Empty;
		
		//traverse through the achievements and compile a list to display on achievements menu
		for (int i = 0; i<iTotalAchievementsCount; i++)
		{
			if (achievements[i].achievementComplete == true)//is the achievement completed
				combinedText += (i+1).ToString() + ". " + achievements[i].achievementDescription + " (Done) \n";
			else
				combinedText += (i+1).ToString() + ". " + achievements[i].achievementDescription + " (" +
				achievementsProgress[ (int)achievements[i].achievementType ].ToString() + "/" + achievements[i].achievementCount + ") \n";
		}//end of for
		
		if (hInGameScriptCS.isCustomMenuEnabled())//if the custom menu is in use
			hMenuScriptCS.updateAchievementsMenuDescription(combinedText);
		else//if the NGUI menu is in use
			hNGUIMenuScript.updateAchievementsMenuDescription(combinedText);
	}//end of update menu description function
}
