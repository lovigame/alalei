/*
*	FUNCTION: Controls all frontal collisions.
*
*	USED BY: This script is part of the PlayerFrontCollider prefab.
*
*/
using UnityEngine;
using System.Collections;

public class PlayerFrontColliderScriptCS : MonoBehaviour {

	private PlayerSidesColliderScriptCS hPlayerSidesColliderScriptCS;
	private InGameScriptCS hInGameScriptCS;
	
	void Start()
	{				
		hPlayerSidesColliderScriptCS = (PlayerSidesColliderScriptCS)GameObject.Find("PlayerSidesCollider").GetComponent(typeof(PlayerSidesColliderScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
	}
	
	void OnCollisionEnter(Collision collision)
	{		
		MoveForwardCS obj = (MoveForwardCS)collision.collider.GetComponent (typeof(MoveForwardCS));
		if (obj != null) {
						obj.stop ();	
				} else {
			obj = (MoveForwardCS)collision.collider.GetComponentInChildren (typeof(MoveForwardCS));
			if (obj != null) {
				obj.stop ();	
			}
			else{
				obj = (MoveForwardCS)collision.collider.transform.parent.GetComponent (typeof(MoveForwardCS));
				if (obj != null) {
					obj.stop ();	
				}
			}
		}

		hPlayerSidesColliderScriptCS.deactivateCollider();	//dont detect stumbles on death
		hInGameScriptCS.collidedWithObstacle();	//play the death scene
	}
	
	public bool isColliderActive() { return this.collider.enabled; }
	public void activateCollider() { this.collider.enabled = true; }
	public void deactivateCollider() { this.collider.enabled = false; }
}
