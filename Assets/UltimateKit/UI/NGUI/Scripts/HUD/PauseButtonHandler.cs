/*
 * FUNCTION:	This script is part of the pause button of the NGUI Pause Menu.
 * 				It pauses the game by calling the appropriate function on click event.
 * */

using UnityEngine;
using System.Collections;

public class PauseButtonHandler : MonoBehaviour {
	
	private InGameScriptCS hInGameScriptCS;
	
	void Start () 
	{		
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
	}
		
	void OnClick () 
	{
		hInGameScriptCS.pauseGame();		
	}
}
