/*
 * FUNCTION:	Handle what the back button will do on each menu.
 * USED BY:		This script is a part of the Back button component located in
 * 				each menu.
 * */

using UnityEngine;
using System.Collections;

public class HomeButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;
	private InGameScriptCS hInGameScriptCS;
	
	void Start ()
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
	}
	
	void OnClick()
	{
		if (hNGUIMenuScript.getCurrentMenu() == NGUIMenuScript.NGUIMenus.GameOverMenu)//if this is GameOver menu
			hInGameScriptCS.procesClicksDeathMenu(MenuScriptCS.GameOverMenuEvents.Back);		
		else if (hNGUIMenuScript.getCurrentMenu() == NGUIMenuScript.NGUIMenus.PauseMenu)//if this is Pause menu
			hInGameScriptCS.processClicksPauseMenu(MenuScriptCS.PauseMenuEvents.MainMenu);
		else if (hNGUIMenuScript.getCurrentMenu() == NGUIMenuScript.NGUIMenus.ShopCostumes//if Shop Costumes menu is active
			|| hNGUIMenuScript.getCurrentMenu() == NGUIMenuScript.NGUIMenus.ShopIAPs//if Shop IAPs menu is active
			|| hNGUIMenuScript.getCurrentMenu() == NGUIMenuScript.NGUIMenus.ShopPowerups//if Shop Powerups menu is active
			|| hNGUIMenuScript.getCurrentMenu() == NGUIMenuScript.NGUIMenus.ShopUtilities)//if Shop Utilities menu is active
		{
			hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.ShopHome);//show the Shop Home menu
			NGUITools.SetActive(this.transform.parent.gameObject, false);//hide the current menu
		}
		else
		{
			hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.MainMenu);//show the main menu
			NGUITools.SetActive(this.transform.parent.gameObject, false);//hide the current menu
		}//end of else
	}	//end of OnClick function
}
