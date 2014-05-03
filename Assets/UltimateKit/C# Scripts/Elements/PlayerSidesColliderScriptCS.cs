/*
*	FUNCTION: Control all the collisions from the sides.
*
*	USED BY: This script is part of the PlayerSidesCollider prefab.
*
*/

using UnityEngine;
using System.Collections;

public class PlayerSidesColliderScriptCS : MonoBehaviour {

	//script references
	private InGameScriptCS hInGameScriptCS;
	private PlayerFrontColliderScriptCS hPlayerFrontColliderScriptCS;
	private ControllerScriptCS hControllerScriptCS;
	
	void Start()
	{		
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hPlayerFrontColliderScriptCS = (PlayerFrontColliderScriptCS)GameObject.Find("PlayerFrontCollider").GetComponent(typeof(PlayerFrontColliderScriptCS));
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));
	}
	
	/*
	*	FUNCTION: Called when player bumps into an obstacle side-ways
	*/
	void OnCollisionEnter(Collision collision)
	{	
		if (hInGameScriptCS.isEnergyZero())
			return;
		else
		{
			hPlayerFrontColliderScriptCS.deactivateCollider();//pause front collision detection till stumble is processed
			hControllerScriptCS.processStumble();	//handle the collision
		}
	}
	
	public bool isColliderActive() { return this.collider.enabled; }
	public void deactivateCollider() { this.collider.enabled = false; }
	public void activateCollider() { this.collider.enabled = false; }
}
