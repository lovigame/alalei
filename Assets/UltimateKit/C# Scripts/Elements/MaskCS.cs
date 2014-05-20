using UnityEngine;
using System.Collections;

public class MaskCS : MonoBehaviour {

	public ControllerScriptCS hControllerScriptCS;
	private bool triggered = false;
	public void Start(){
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));
	}
	 public void trigger(){
		if (triggered) {
			return;
		}
		triggered = true;
		_trigger ();
	}

	virtual public void _trigger(){
	}
}
