using UnityEngine;
using System.Collections;

public class IAPButtonHandler : MonoBehaviour {

	private NGUIMenuScript hNGUIMenuScript;
	
	void Start () 
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
		
	void OnClick () 
	{
		hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.ShopIAPs);
		NGUITools.SetActive(this.transform.parent.gameObject, false);
	}
}
