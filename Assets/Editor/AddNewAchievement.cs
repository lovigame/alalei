/*
 * FUNCITON:	This script generates the wizard window used to add a new achievement.
 * */

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;

public class AddNewAchievement : ScriptableWizard {

	private string description;//create a description field
	private int count;		//create a count field
	
	static bool isJavascriptEnabled;//which type of scripting language is in use
	private GlobalAchievementTypes achievementTypeJS;
	private GlobalAchievementControllerCS.GlobalAchievementTypes achievementTypeCS;
		
	[MenuItem("Wizards/Add a New Achievement")]
	static void Init () 
	{
		// Get existing open window or if none, make a new one:
		AddNewAchievement window = (AddNewAchievement)EditorWindow.GetWindow (typeof (AddNewAchievement));
		
		//check which type of scripting language is active
		isJavascriptEnabled = ToggleScriptType.isJavascriptTypeEnabled();
	}
	
	void OnGUI ()
	{
		GUILayout.Label ("Achievement Properties", EditorStyles.boldLabel);//create editor window
		
		description = EditorGUILayout.TextField ("Description", description);//add description field
		count = EditorGUILayout.IntField("Count", count);//add count field
		
		//add the achievement type drop down field based on the active scripting language
		if (isJavascriptEnabled)
			achievementTypeJS = (GlobalAchievementTypes)EditorGUILayout.EnumPopup("Achievement Type", achievementTypeJS);
		else
			achievementTypeCS = (GlobalAchievementControllerCS.GlobalAchievementTypes)EditorGUILayout.EnumPopup("Achievement Type", achievementTypeCS);
				
		if (GUILayout.Button("Add Achievement"))//add button
			addButtonHandler();
	}
	
	/*
	 * FUNCTION:	Compile the given info and write it in file.
	 * */
	void addButtonHandler()
	{
		if (description == "" || count <= 0)//check if all fields have been used
		{
			Debug.Log("EXCEPTION: Please input value for all fields");
			return;
		}
		else
		{
			string missionInfo = string.Empty;
			string fileName = "Assets/Resources/GlobalAchievementsList.txt";
			
			missionInfo += "\n" + description + "\n";
			missionInfo += count.ToString() + "\n";
			
			if (isJavascriptEnabled)
				missionInfo += achievementTypeJS.ToString();
			else
				missionInfo += achievementTypeCS.ToString();
			
			//write the new achievement to file
			using (StreamWriter writer = new StreamWriter(fileName, true))
			{
				writer.Write(missionInfo);
				writer.Close();
			}
			
			Debug.Log("Achievement Added Successfully!");
			this.Close();//close the editor window
		}
	}//end of add Button Handler
}
