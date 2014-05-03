/*
 * FUNCTION:	Handles what happens when the Costumes Button is clicked.
 * 				The costume button is located in the Shop Home menu.
 * 
 * */

using UnityEngine;
using System.Collections;

public class CostumesButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;
	
	void Start () 
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
		
	void OnClick () 
	{
		hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.ShopCostumes);
		NGUITools.SetActive(this.transform.parent.gameObject, false);
	}
}
