using UnityEngine;
using System.Collections;

public class MaskBossEndCS : MaskCS {



	override public void _trigger(){
		hControllerScriptCS.endBattle ();
	}
}
