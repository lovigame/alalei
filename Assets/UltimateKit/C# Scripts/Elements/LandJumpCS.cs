using UnityEngine;
using System.Collections;

public class LandJumpCS : LandScriptCS {

	public float fJumpHeight = 800;
	public override void hit(){
		if (triggered) {
			return;
		}
		if (hControllerScriptCS.isFlying ()) {
			return;		
		}
		base.hit();
		Debug.Log ("f"+fJumpHeight);
		hControllerScriptCS.jumpUP (fJumpHeight);
	}
}
