using UnityEngine;
using System.Collections;

public class MaskBossStartCS : MaskCS {



	override public void _trigger(){
		hControllerScriptCS.startBattle ();
	}
}
