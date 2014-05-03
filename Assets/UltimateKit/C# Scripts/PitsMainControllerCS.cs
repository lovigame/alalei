/*
*	FUNCITON:
*	This script checks if there is a terrain layer under the player character.
*	If there isn't the player will fall and its energy will be reduced to zero.
*	
*	USED BY: This script is part of the "Player" prefab.
*
*/
using UnityEngine;
using System.Collections;

public class PitsMainControllerCS : MonoBehaviour {

	private Transform tPlayer;
	private bool bPitFallingStart = false;
	private float fCurrentEnergyDepletionSpeed = 10.0f;
	
	private InGameScriptCS hInGameScriptCS;
	private ControllerScriptCS hControllerScriptCS;
	
	void Start()
	{
		tPlayer = GameObject.Find("Player").transform;
		bPitFallingStart = false;
		
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		hControllerScriptCS = (ControllerScriptCS)this.GetComponent(typeof(ControllerScriptCS));
	}
	
	void Update()
	{
		if(hInGameScriptCS.isGamePaused()==true)
			return;
		
		if(bPitFallingStart)
		{
			hInGameScriptCS.decrementEnergy( (hInGameScriptCS.getCurrentEnergy()/10) + (int)Time.deltaTime*100);
		}
	}
	
	/*
	*	FUNCTION: Reduce energy if player fell int a pit
	*/
	public void setPitValues()
	{
		bPitFallingStart = true;
			
		hControllerScriptCS.setPitFallLerpValue(Time.time);
		hControllerScriptCS.setPitFallForwardSpeed(hControllerScriptCS.getCurrentForwardSpeed());
				
		print("Fell in a Pit");
	}
	
	public bool isFallingInPit() { return bPitFallingStart; }
}
