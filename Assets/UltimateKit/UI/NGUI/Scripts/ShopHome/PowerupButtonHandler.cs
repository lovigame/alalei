/*
 * FUNCTION:	Displays the Shop Power-up menu and hides the
 * 				Shop Home menu.
 * */

using UnityEngine;
using System.Collections;

public class PowerupButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;
	
	void Start () 
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
		
	void OnClick () 
	{
		hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.ShopPowerups);
		NGUITools.SetActive(this.transform.parent.gameObject, false);
	}
}
