
/*
*	FUNCTION:
*	- This script controls the Enemy (Police Car) based on the player’s movement.
*	- It controls the enemy's animatations and it's behavior if the player stumbles.
*
*	USED BY:
*	This script is a part of the “Enemy” prefab.
*
*/

using UnityEngine;
using System.Collections;

public class EnemyControllerCS : MonoBehaviour {

	private Transform tEnemy;	//enemy transform
	private Transform tPlayer;//player transform
	
	private int iEnemyState = 0;
	private float fDeathRotation = 0.0f;
	private float fCosLerp = 0.0f;	//used for Lerp
	
	//script references
	private InGameScriptCS hInGameScriptCS;
	private ControllerScriptCS hControllerScriptCS;
	private SoundManagerCS hSoundManagerCS;
	
	//enemy logic
	private float fEnemyPosition = 0.0f;
	private float fEnemyPositionX = -5;
	private float fEnemyPositionY = 0;
	private float fStumbleStartTime;
	private float fChaseTime = 5;
	
	void Start()
	{	
		tPlayer = GameObject.Find("Player").transform;
		tEnemy = this.transform;
		
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
	}
	
	/*
	*	FUNCTION: Starting the chasing sequence
	*	CALLED BY: ControllerScript.launchGame()
	*/
	public void launchEnemy()
	{
		iEnemyState = 2;
	}
	
	void FixedUpdate ()
	{
		if(hInGameScriptCS.isGamePaused()==true)
			return;
			
		//set the position of guard in current frame		
		tEnemy.position = new Vector3(Mathf.Lerp(tEnemy.position.x, (tPlayer.position.x - fEnemyPosition), Time.deltaTime*10), 
			tEnemy.position.y, tEnemy.position.z);
			
		if (!hControllerScriptCS.isInAir())//follow the player in y-axis if he's not jumping (cars cant jump)
			tEnemy.position = new Vector3(tEnemy.position.x, Mathf.Lerp(tEnemy.position.y, tPlayer.position.y + fEnemyPositionY, Time.deltaTime*8),
				tEnemy.position.z);			
		
		//ignore y-axis rotation and horizontal movement in idle and death state
		if (iEnemyState < 4)
		{
			tEnemy.position = new Vector3(tEnemy.position.x, tEnemy.position.y,	Mathf.Lerp(tEnemy.position.z, tPlayer.position.z, Time.deltaTime*10));
			tEnemy.localEulerAngles = new Vector3(tEnemy.localEulerAngles.x,-hControllerScriptCS.getCurrentPlayerRotation(), tEnemy.localEulerAngles.z);
		}
		
		if (iEnemyState == 1)//hide the chasing character
		{
			fCosLerp += (Time.deltaTime/10);
			fEnemyPosition = Mathf.Lerp(fEnemyPosition, fEnemyPositionX + 45, Mathf.Cos(fCosLerp)/1000);
			
			if (fCosLerp >= 0.7f)
			{
				fCosLerp = 0.0f;
				iEnemyState = 0;
				
				hSoundManagerCS.stopSound(SoundManagerCS.EnemySounds.Siren);
			}
		}
		else if (iEnemyState == 2)//show the chasing character
		{
			hSoundManagerCS.playSound(SoundManagerCS.EnemySounds.Siren);
			
			fCosLerp += (Time.deltaTime/4);
			fEnemyPosition = Mathf.Lerp(fEnemyPosition, fEnemyPositionX, Mathf.Cos(fCosLerp));
			
			if (fCosLerp >= 1.5f)
			{
				fCosLerp = 0.0f;
				iEnemyState = 3;
			}
		}
		else if (iEnemyState == 3)//wait for 'fChaseTime' after showing character
		{
			if ( (Time.time - fStumbleStartTime)%60 >= fChaseTime)
				iEnemyState = 1;
		}
		
		//DEATH SEQUENCE
		else if (iEnemyState == 4)//on death
		{	
			//to ensure correct rotation animation
			tEnemy.localEulerAngles = new Vector3(tEnemy.localEulerAngles.x, 350, tEnemy.localEulerAngles.z);
			
			hSoundManagerCS.playSound(SoundManagerCS.EnemySounds.TiresSqueal);
			iEnemyState = 5;
		}
		else if (iEnemyState == 5)//pin behind the player
		{
			fEnemyPosition = Mathf.Lerp(fEnemyPosition, fEnemyPositionX+20, Time.fixedDeltaTime*50);//vertical position after skid
			tEnemy.position = new Vector3(tEnemy.position.x, tEnemy.position.y,
				Mathf.Lerp(tEnemy.position.z, tPlayer.position.z + 20, Time.deltaTime*10));//horizontal position after skid
						
			tEnemy.localEulerAngles = Vector3.Lerp(tEnemy.localEulerAngles, new Vector3(0,260,0), Time.deltaTime*10);//90 degree rotation
			if (tEnemy.localEulerAngles.y <= 261)
				iEnemyState = 6;
		}
		else if (iEnemyState == 6)
		{
			hSoundManagerCS.stopSound(SoundManagerCS.EnemySounds.Siren);
		}
	}//end of Update
	
	/*
	*	FUNCTION: Animate enemy
	*	RETURNS:	'true' if the enemy was already chasing player
	*				'false' if the enemy was not chasing the player
	*	CALLED BY: ControllerScript.processStumble()
	*/
	public bool processStumble()
	{
		if (isEnemyActive())//if enemy is already chasing player
		{
			iEnemyState = 0;		
			return true;
		}
		else
		{
			fStumbleStartTime = Time.time;
			iEnemyState = 2;		
			return false;
		}
	}
	
	public void playDeathAnimation() { iEnemyState = 4; }
	public void hideEnemy() { iEnemyState = 1; }
	
	/*
	*	FUNCTION: Check if the enemy is chasing the player
	*/
	public bool isEnemyActive()
	{
		if (iEnemyState == 2 || iEnemyState == 3)
			return true;
		else
			return false;
	}
}
