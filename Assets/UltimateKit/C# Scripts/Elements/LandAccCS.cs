using UnityEngine;
using System.Collections;

public class LandAccCS : LandScriptCS {

	public float fSpeed = 5.0f;
	public float fTime = 1.0f;
	
	public override void hit(){
		if (triggered) {
			return;
		}
		if (hControllerScriptCS.isFlying ()) {
			return;		
		}
		base.hit();
		hControllerScriptCS.speedUP (fSpeed,fTime);
	}
}
