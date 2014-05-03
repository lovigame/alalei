/*
 * FUNCTION:	Handles what happens when the Achievement button is tapped.
 * 				The achievement button is located in the Main Menu.
 * */

using UnityEngine;
using System.Collections;

public class AchievementsButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;

	void Start ()
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
	
	void OnClick ()
	{
		hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.AchievementsMenu);
		NGUITools.SetActive(this.transform.parent.gameObject, false);
	}
}
