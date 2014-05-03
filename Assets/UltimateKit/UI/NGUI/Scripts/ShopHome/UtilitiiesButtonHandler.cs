/*
 * FUNCTION:	Displays the Shop Utilities menu and hides the
 * 				Shop home menu.
 * */

using UnityEngine;
using System.Collections;

public class UtilitiiesButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;
	
	void Start () 
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
		
	void OnClick () 
	{
		hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.ShopUtilities);
		NGUITools.SetActive(this.transform.parent.gameObject, false);
	}
}
