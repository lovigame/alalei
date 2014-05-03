/*
 * FUNCTION:	Handles what happens when the Instruction button is tapped.
 * 				The button is located in the Main Menu.
 * */

using UnityEngine;
using System.Collections;

public class InstructionButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;

	void Start ()
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
	
	void OnClick ()
	{
		hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.InstructionsMenu);
		NGUITools.SetActive(this.transform.parent.gameObject, false);
	}
}
