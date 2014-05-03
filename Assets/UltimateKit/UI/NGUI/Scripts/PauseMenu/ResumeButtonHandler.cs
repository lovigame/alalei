using UnityEngine;
using System.Collections;

public class ResumeButtonHandler : MonoBehaviour {
	
	private NGUIMenuScript hNGUIMenuScript;
	
	void Start () 
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
	}
	
	void OnClick()
	{
		hNGUIMenuScript.startResumeGameCounter();
	}
}
