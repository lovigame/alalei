using UnityEngine;
using System.Collections;

public class LandScriptCS : MonoBehaviour {

	public bool triggered = false;
	public ControllerScriptCS hControllerScriptCS;
	// Use this for initialization
	void Start () {
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));

	}


	virtual public void hit(){



		triggered = true;


	}
	

}
