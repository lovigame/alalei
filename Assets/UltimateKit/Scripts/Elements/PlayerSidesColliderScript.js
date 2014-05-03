#pragma strict

/*
*	FUNCTION: Control all the collisions from the sides.
*
*	USED BY: This script is part of the PlayerSidesCollider prefab.
*
*/

//script references
private var hInGameScript : InGameScript;
private var hPlayerFrontColliderScript : PlayerFrontColliderScript;
private var hControllerScript : ControllerScript;

function Start()
{
	//bSidesColliderFlag = true;
	
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hPlayerFrontColliderScript = GameObject.Find("PlayerFrontCollider").GetComponent(PlayerFrontColliderScript) as PlayerFrontColliderScript;
	hControllerScript = GameObject.Find("Player").GetComponent(ControllerScript) as ControllerScript;
}

/*
*	FUNCTION: Called when player bumps into an obstacle side-ways
*/
function OnCollisionEnter(collision : Collision)
{	
	if (hInGameScript.isEnergyZero())
		return;
	else
	{
		hPlayerFrontColliderScript.deactivateCollider();//pause front collision detection till stumble is processed
		hControllerScript.processStumble();	//handle the collision
	}
}

public function isColliderActive() { return this.collider.enabled; }
public function deactivateCollider() { this.collider.enabled = false; }
public function activateCollider() { this.collider.enabled = false; }