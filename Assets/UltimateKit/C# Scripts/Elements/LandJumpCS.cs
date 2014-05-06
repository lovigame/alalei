using UnityEngine;
using System.Collections;

public class LandJumpCS : LandScriptCS {

	public float fJumpHeight = 800;
	public override void hit(){
		base.hit();
		hControllerScriptCS.jumpUP (fJumpHeight);
	}
}
