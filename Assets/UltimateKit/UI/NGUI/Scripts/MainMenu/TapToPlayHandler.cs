/*
 * FUNCTION:	Handles what happens when the Tap to play button is tapped.
 * 				The button is located in the Main Menu.
 * */

using UnityEngine;
using System.Collections;

public class TapToPlayHandler : MonoBehaviour {

	private InGameScriptCS hInGameScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
	
	void Start()
	{
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
	
	void OnClick()
	{
		hInGameScriptCS.launchGame();	//start the gameplay
		
		hNGUIMenuScript.NGUIMenuScriptEnabled(false);//turn off the NGUI Menu Script (to improve performance)
		NGUITools.SetActive(this.transform.parent.gameObject, false);//close/ disable the current menu
	}
}
