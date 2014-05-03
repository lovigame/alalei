/*
 * FUNCITON:	This script generates the wizard window used to add a new mission.
 * */

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class AddNewMission : EditorWindow
{
	private string description;//create a description field
	private int count;		//create a count field
	
	static bool isJavascriptEnabled;//which type of scripting language is in use
	private MissionTypes missionTypeJS;
	private MissionsControllerCS.MissionTypes missionTypeCS;
		
	[MenuItem("Wizards/Add a New Mission")]
	static void Init () 
	{
		// Get existing open window or if none, make a new one:
		AddNewMission window = (AddNewMission)EditorWindow.GetWindow (typeof (AddNewMission));
		
		//check which type of scripting language is active
		isJavascriptEnabled = ToggleScriptType.isJavascriptTypeEnabled();
	}
	
	void OnGUI ()
	{
		GUILayout.Label ("Mission Properties", EditorStyles.boldLabel);//create editor window
		
		description = EditorGUILayout.TextField ("Description", description);//add description field
		count = EditorGUILayout.IntField("Count", count);//add count field
		
		//add the achievement type drop down field based on the active scripting language
		if (isJavascriptEnabled)
			missionTypeJS = (MissionTypes)EditorGUILayout.EnumPopup("Mission Type", missionTypeJS);
		else
			missionTypeCS = (MissionsControllerCS.MissionTypes)EditorGUILayout.EnumPopup("Mission Type", missionTypeCS);
				
		if (GUILayout.Button("Add Mission"))
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
			string fileName = "Assets/Resources/MissionsList.txt";
			
			missionInfo += "\n" + description + "\n";
			missionInfo += count.ToString() + "\n";
			
			if (isJavascriptEnabled)
				missionInfo += missionTypeJS.ToString();
			else
				missionInfo += missionTypeCS.ToString();
			
			//write the new mission to file
			using (StreamWriter writer = new StreamWriter(fileName, true))
			{
				writer.Write(missionInfo);
				writer.Close();
			}
			
			Debug.Log("Mission Added Successfully!");
			this.Close();//close the editor window
		}
	}//end of add Button Handler
}
