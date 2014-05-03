#pragma strict

/*
*	FUNCTION:
*	- This script keeps track of the global/ game center achievements.
*	- All of the achievements if not completed are kept active.
*	- Counters funcitons are called in every relevant script to track the
*	progress of the achievements.
*/

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

public class GlobalAchievementDetail extends System.ValueType
{
	public var achievementDescription : String;//description of the achievement
	public var achievementCount : int;			//number of actions to perform to complete the achievement
	public var achievementType : GlobalAchievementTypes;//what actions to perform (jump, collect currency etc.)
	public var achievementComplete : boolean;	//if the achievement has been completed
}

//script references
private var hMenuScript : MenuScript;
private var hInGameScript : InGameScript;

private var iTotalAchievementsCount:int;//amount of available achievements
private var achievements : GlobalAchievementDetail[];//information of all the achievements

private var achievementsProgress:int[];//store the current progress of achievements

function Start ()
{
	achievementsProgress = new int[System.Enum.GetValues(GlobalAchievementTypes).Length];
		
	hInGameScript = this.GetComponent(InGameScript) as InGameScript;	
	hMenuScript = GameObject.Find("MenuGroup").GetComponent(MenuScript) as MenuScript;		
	
	//get the GlobalAchievementsList file from the resources folder
	var achievementsFile = Resources.Load("GlobalAchievementsList", typeof(TextAsset));
    var stringReader = new StringReader(achievementsFile.text);//open the text file
    var fileContents = stringReader.ReadToEnd();//read the text from the text file
    var lines = fileContents.Split("\n"[0]);//tokenize on line seperators
    
    if (lines.Length == 0)//if the file was empty
	{
		Debug.Log("No achievements found in file");
		this.enabled = false;
	}
	else//read file and extract achievements detail
	{
		var lineIndex:int=0;
		var arrayIndex:int=0;
		iTotalAchievementsCount = lines.Length/3;//get the total number of achievements in the file
		achievements = new GlobalAchievementDetail[iTotalAchievementsCount];//allocate memory according to the number of achievement
		
		while (lineIndex < lines.Length)//store the file content in achievement array
		{
			achievements[arrayIndex].achievementDescription = lines[lineIndex++];
			achievements[arrayIndex].achievementCount = int.Parse(lines[lineIndex++]);
			achievements[arrayIndex].achievementType = System.Enum.Parse(typeof(GlobalAchievementTypes), lines[lineIndex++]);
			achievements[arrayIndex].achievementComplete = false;	//mark achievement incomplete by default
						
			if (PlayerPrefs.HasKey("GlobalAchievement_"+arrayIndex))//check achievement progress
			{				
				achievementsProgress[ achievements[arrayIndex].achievementType ] = PlayerPrefs.GetInt("GlobalAchievement_"+arrayIndex);
				
				//check if the achievement has been completed
				if (achievementsProgress[ achievements[arrayIndex].achievementType ] >= achievements[arrayIndex].achievementCount)
					achievements[arrayIndex].achievementComplete = true;
			}
			else//if this is the first game launch
			{
				PlayerPrefs.SetInt("GlobalAchievement_"+arrayIndex, 0);
				achievementsProgress[ achievements[arrayIndex].achievementType ] = 0;
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
public function incrementAchievementCount(type:GlobalAchievementTypes) 
{ 
	achievementsProgress[type]++;
	checkCompletion(type);
}

/*
*	FUNCTION:	Increment achievement counter by required value.
*/
public function incrementAchievementCount(type:GlobalAchievementTypes, iVal:int)
{
	achievementsProgress[type]+= iVal;
	checkCompletion(type);
}

/*
*	FUNCTION:	Check if an achievement has been completed. Save the progress if
*				changes have been made in the achievement progress.
*	CALLED BY:	incrementAchievementCount(...)
*/
private function checkCompletion(achievementType:GlobalAchievementTypes)
{	
	for (var i:int = 0; i<iTotalAchievementsCount; i++)
	{
		if (achievements[i].achievementType == achievementType	//is the updated counter of the current achievement
		&& achievements[i].achievementComplete == false)	//has the achievement been completed previously
		{
			if (achievementsProgress[achievementType] >= achievements[i].achievementCount)//has the achievement been completed
				achievements[i].achievementComplete = true;	//mark achievement as complete
			
			PlayerPrefs.SetInt("GlobalAchievement_"+i, achievementsProgress[achievementType]);//save progress permanently	
		}//end of outer if
	}//end of for	
}//end of check completion function

/*
*	FUNCTION:	Compiles all the achievements' descriptions and
*				tells the MenuScript to display it on Pause Menu
*				and the Missions Menu
*/
private function updateMenuDescription()
{
	var combinedText:String = String.Empty;
	
	//traverse through the achievements and compile a list to display on achievements menu
	for (var i:int = 0; i<iTotalAchievementsCount; i++)
	{
		if (achievements[i].achievementComplete == true)//is the achievement completed
			combinedText += (i+1).ToString() + ". " + achievements[i].achievementDescription + " (Done) \n";
		else
			combinedText += (i+1).ToString() + ". " + achievements[i].achievementDescription + " (" +
			achievementsProgress[ achievements[i].achievementType ] + "/" + achievements[i].achievementCount + ") \n";
	}//end of for
	
	hMenuScript.updateAchievementsMenuDescription(combinedText);
}//end of update menu description function