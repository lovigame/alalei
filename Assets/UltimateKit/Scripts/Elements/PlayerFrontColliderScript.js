#pragma strict

/*
*	FUNCTION: Controls all frontal collisions.
*
*	USED BY: This script is part of the PlayerFrontCollider prefab.
*
*/

private var hPlayerSidesColliderScript : PlayerSidesColliderScript;
private var hInGameScript : InGameScript;

function Start()
{
	//bFrontColliderFlag = true;
	
	hPlayerSidesColliderScript = GameObject.Find("PlayerSidesCollider").GetComponent(PlayerSidesColliderScript) as PlayerSidesColliderScript;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
}

function OnCollisionEnter(collision : Collision)
{		
	hPlayerSidesColliderScript.deactivateCollider();	//dont detect stumbles on death
	hInGameScript.collidedWithObstacle();	//play the death scene
}

public function isColliderActive() { return this.collider.enabled; }
public function activateCollider() { this.collider.enabled = true; }
public function deactivateCollider() { this.collider.enabled = false; }