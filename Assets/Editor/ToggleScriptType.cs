/*
 * FUNCITON:	Remove the currently used script and replace them with the alternative script.
 * 				Forexample if you are using Javacript, running this script will remove the Javascript
 * 				script files and attach C# script files.
 * 
 * ADDITIONAL INFO:	Please note that once you change the scipt type, you will need to poplulate the
 * 					exposed variables again for the project to run correctly.
 * */

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;

public class ToggleScriptType : ScriptableObject {
	
	[MenuItem ("Wizards/Toogle Script Type")]
	static void DoApply()
	{
		bool javascriptEnabled = true;
		TextAsset taFile = (TextAsset)Resources.Load("EditorAttributes");
		string[] lines = taFile.text.Split('\n');
		
		//change the JavascirptEnabled attribute in file
		for (int i=0; i<lines.Length; i++)
		{
			if (string.Compare(lines[i], "JavascriptEnabled") == 0)
			{
				if (string.Compare(lines[i+1], "True") == 0)
					javascriptEnabled = true;
				else
					javascriptEnabled = false;
				
				lines[i+1] = (!javascriptEnabled).ToString();
				break;
			}//end of if		
		}//end of for
				
		File.WriteAllLines("Assets/Resources/EditorAttributes.txt", lines);//write the updates in file
				
		if (javascriptEnabled == true)//remove/ disable javascript scripts and add/ enable C# scripts
		{
			DestroyImmediate((framespersecond)GameObject.Find("DebugInfo").GetComponent(typeof(framespersecond)));
			DestroyImmediate((EnemyController)GameObject.Find("Enemy").GetComponent(typeof(EnemyController)));
			DestroyImmediate((HUDController)GameObject.Find("HUDMainGroup").GetComponent(typeof(HUDController)));
			DestroyImmediate((CameraController)GameObject.Find("Main Camera").GetComponent(typeof(CameraController)));
			GameObject.Find("DebugInfo").AddComponent(typeof(framespersecondCS));
			GameObject.Find("Enemy").AddComponent(typeof(EnemyControllerCS));
			GameObject.Find("HUDMainGroup").AddComponent(typeof(HUDControllerCS));
			GameObject.Find("Main Camera").AddComponent(typeof(CameraControllerCS));
			
			//switch the scripts attached to the menus
			DestroyImmediate((MenuScript)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScript)));
			DestroyImmediate((ShopScript)GameObject.Find("MenuGroup/Shop").GetComponent(typeof(ShopScript)));
			GameObject.Find("MenuGroup").AddComponent(typeof(MenuScriptCS));
			GameObject.Find("MenuGroup/Shop").AddComponent(typeof(ShopScriptCS));
			
			//swtich all scripts attached to the Player prefab
			DestroyImmediate((InGameScript)GameObject.Find("Player").GetComponent(typeof(InGameScript)));
			DestroyImmediate((PowerupsMainController)GameObject.Find("Player").GetComponent(typeof(PowerupsMainController)));
			DestroyImmediate((PitsMainController)GameObject.Find("Player").GetComponent(typeof(PitsMainController)));
			DestroyImmediate((ElementsGenerator)GameObject.Find("Player").GetComponent(typeof(ElementsGenerator)));
			DestroyImmediate((PatchesRandomizer)GameObject.Find("Player").GetComponent(typeof(PatchesRandomizer)));
			DestroyImmediate((CheckPointsMain)GameObject.Find("Player").GetComponent(typeof(CheckPointsMain)));
			DestroyImmediate((GlobalAchievementController)GameObject.Find("Player").GetComponent(typeof(GlobalAchievementController)));
			DestroyImmediate((MissionsController)GameObject.Find("Player").GetComponent(typeof(MissionsController)));
			DestroyImmediate((ControllerScript)GameObject.Find("Player").GetComponent(typeof(ControllerScript)));
			DestroyImmediate((SwipeControls)GameObject.Find("Player").GetComponent(typeof(SwipeControls)));
			GameObject.Find("Player").AddComponent(typeof(InGameScriptCS));
			GameObject.Find("Player").AddComponent(typeof(PowerupsMainControllerCS));
			GameObject.Find("Player").AddComponent(typeof(PitsMainControllerCS));
			GameObject.Find("Player").AddComponent(typeof(ElementsGeneratorCS));
			GameObject.Find("Player").AddComponent(typeof(PatchesRandomizerCS));
			GameObject.Find("Player").AddComponent(typeof(CheckPointsMainCS));
			GameObject.Find("Player").AddComponent(typeof(GlobalAchievementControllerCS));
			GameObject.Find("Player").AddComponent(typeof(MissionsControllerCS));
			GameObject.Find("Player").AddComponent(typeof(ControllerScriptCS));
			GameObject.Find("Player").AddComponent(typeof(SwipeControlsCS));
			
			//switch the scripts attached to the colliders
			DestroyImmediate((PlayerFrontColliderScript)GameObject.Find("PlayerFrontCollider").GetComponent(typeof(PlayerFrontColliderScript)));
			DestroyImmediate((PlayerSidesColliderScript)GameObject.Find("PlayerSidesCollider").GetComponent(typeof(PlayerSidesColliderScript)));
			GameObject.Find("PlayerFrontCollider").AddComponent(typeof(PlayerFrontColliderScriptCS));
			GameObject.Find("PlayerSidesCollider").AddComponent(typeof(PlayerSidesColliderScriptCS));
			
			//( (SoundManager)GameObject.Find("SoundManager").GetComponent(typeof(SoundManager)) ).toggleScriptState(false);//disable soundmanager.js
			( (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS)) ).toggleScriptState(true);//enable soundmanagerCS
		}
		else//remove C# scripts and add javascript scripts
		{
			DestroyImmediate((framespersecondCS)GameObject.Find("DebugInfo").GetComponent(typeof(framespersecondCS)));
			DestroyImmediate((EnemyControllerCS)GameObject.Find("Enemy").GetComponent(typeof(EnemyControllerCS)));
			DestroyImmediate((HUDControllerCS)GameObject.Find("HUDMainGroup").GetComponent(typeof(HUDControllerCS)));
			DestroyImmediate((CameraControllerCS)GameObject.Find("Main Camera").GetComponent(typeof(CameraControllerCS)));
			GameObject.Find("DebugInfo").AddComponent(typeof(framespersecond));
			GameObject.Find("Enemy").AddComponent(typeof(EnemyController));
			GameObject.Find("HUDMainGroup").AddComponent(typeof(HUDController));
			GameObject.Find("Main Camera").AddComponent(typeof(CameraController));
			
			//switch the scripts attached to the menus
			DestroyImmediate((MenuScriptCS)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScriptCS)));
			DestroyImmediate((ShopScriptCS)GameObject.Find("MenuGroup/Shop").GetComponent(typeof(ShopScriptCS)));
			GameObject.Find("MenuGroup").AddComponent(typeof(MenuScript));
			GameObject.Find("MenuGroup/Shop").AddComponent(typeof(ShopScript));
			
			//swtich all scripts attached to the Player prefab
			DestroyImmediate((InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS)));
			DestroyImmediate((PowerupsMainControllerCS)GameObject.Find("Player").GetComponent(typeof(PowerupsMainControllerCS)));
			DestroyImmediate((PitsMainControllerCS)GameObject.Find("Player").GetComponent(typeof(PitsMainControllerCS)));
			DestroyImmediate((ElementsGeneratorCS)GameObject.Find("Player").GetComponent(typeof(ElementsGeneratorCS)));
			DestroyImmediate((PatchesRandomizerCS)GameObject.Find("Player").GetComponent(typeof(PatchesRandomizerCS)));
			DestroyImmediate((CheckPointsMainCS)GameObject.Find("Player").GetComponent(typeof(CheckPointsMainCS)));
			DestroyImmediate((GlobalAchievementControllerCS)GameObject.Find("Player").GetComponent(typeof(GlobalAchievementControllerCS)));
			DestroyImmediate((MissionsControllerCS)GameObject.Find("Player").GetComponent(typeof(MissionsControllerCS)));
			DestroyImmediate((ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS)));
			DestroyImmediate((SwipeControlsCS)GameObject.Find("Player").GetComponent(typeof(SwipeControlsCS)));
			GameObject.Find("Player").AddComponent(typeof(InGameScript));
			GameObject.Find("Player").AddComponent(typeof(PowerupsMainController));
			GameObject.Find("Player").AddComponent(typeof(PitsMainController));
			GameObject.Find("Player").AddComponent(typeof(ElementsGenerator));
			GameObject.Find("Player").AddComponent(typeof(PatchesRandomizer));
			GameObject.Find("Player").AddComponent(typeof(CheckPointsMain));
			GameObject.Find("Player").AddComponent(typeof(GlobalAchievementController));
			GameObject.Find("Player").AddComponent(typeof(MissionsController));
			GameObject.Find("Player").AddComponent(typeof(ControllerScript));
			GameObject.Find("Player").AddComponent(typeof(SwipeControls));
			
			//switch the scripts attached to the colliders
			DestroyImmediate((PlayerFrontColliderScriptCS)GameObject.Find("PlayerFrontCollider").GetComponent(typeof(PlayerFrontColliderScriptCS)));
			DestroyImmediate((PlayerSidesColliderScriptCS)GameObject.Find("PlayerSidesCollider").GetComponent(typeof(PlayerSidesColliderScriptCS)));
			GameObject.Find("PlayerFrontCollider").AddComponent(typeof(PlayerFrontColliderScript));
			GameObject.Find("PlayerSidesCollider").AddComponent(typeof(PlayerSidesColliderScript));
			
		//	( (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS)) ).toggleScriptState(false);//disable soundmanagerCS.cs
		//	( (SoundManager)GameObject.Find("SoundManager").GetComponent(typeof(SoundManager)) ).toggleScriptState(true);//enable soundmanager.js
		}
		
		Debug.Log("Script type switched");
	}//end of Do Apply function
	
	/*
	 * FUNCITON:	Return true if javascript scripts are in use.
	 * */
	public static bool isJavascriptTypeEnabled()
	{		
		TextAsset taFile = (TextAsset)Resources.Load("EditorAttributes");
		string[] lines = taFile.text.Split('\n');
		
		for (int i=0; i<lines.Length; i++)
		{
			if (string.Compare(lines[i], "JavascriptEnabled") == 0)
			{
				if (string.Compare(lines[i+1], "True") == 0)
					return true;
				else
					return false;				
			}//end of if		
		}//end of for
		
		return false;
	}
}