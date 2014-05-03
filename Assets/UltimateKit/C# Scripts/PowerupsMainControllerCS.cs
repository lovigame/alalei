/*
*	FUNCTION:
*	- This script activates and deactivates powerups.
*	- It defines the execution of powerups.
*	- It handles and triggers events when a currency unit is collected.
*
*/
using UnityEngine;
using System.Collections;

public class PowerupsMainControllerCS : MonoBehaviour {

	//all the powerups used in the game
	public enum PowerUps
	{	
		Magnetism = 0,
		Ghost = 1,
		Currency = 2
	}
	
	private Transform tPlayer;	//player transform
	private int iCurrencyUnits = 0;	//curency earned in a particular run
	private float fMangetismRadius;	//when to pull currency if magnetism is active
	private float fMagnetismDefaultRadius;	//when to pull currency
	private int iPowerupCount;	//a count of types of powerups
	
	private bool[] bPowerupStatus;	//if and which powerup is active
	private float[] fPowerupStartTime;//the time when a powerup is started
	private float[] fPowerupTotalDuration;//total time to keep the powerup active
	
	//script references
	private InGameScriptCS hInGameScriptCS;
	private SoundManagerCS hSoundManagerCS;
	private ControllerScriptCS hControllerScriptCS;
	private MissionsControllerCS hMissionsControllerCS;
	private GlobalAchievementControllerCS hGlobalAchievementControllerCS;
	private PlayerFrontColliderScriptCS hPlayerFrontColliderScriptCS;
	private PlayerSidesColliderScriptCS hPlayerSidesColliderScriptCS;
	
	private Transform tHUDPUMeter;//the HUD powerup meter
	private Transform tHUDPUMeterBar;//the bar in the powerup meter on HUD
	private UISlider uisHUDPUMeter;//the power-up meter in the NGUI menus
	
	void Start()
	{
		tPlayer = transform;	
		
		//powerup meter visual		
		iPowerupCount = PowerUps.GetValues(typeof(PowerUps)).Length-1;//get the total number of powerups
		
		bPowerupStatus = new bool[iPowerupCount];
		fPowerupStartTime = new float[iPowerupCount];	
		fPowerupTotalDuration = new float[iPowerupCount];
	
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		hControllerScriptCS = (ControllerScriptCS)this.GetComponent(typeof(ControllerScriptCS));
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
		hMissionsControllerCS = (MissionsControllerCS)this.GetComponent(typeof(MissionsControllerCS));
		hGlobalAchievementControllerCS = (GlobalAchievementControllerCS)this.GetComponent(typeof(GlobalAchievementControllerCS));
		hPlayerFrontColliderScriptCS = (PlayerFrontColliderScriptCS)GameObject.Find("PlayerFrontCollider").GetComponent(typeof(PlayerFrontColliderScriptCS));
		hPlayerSidesColliderScriptCS = (PlayerSidesColliderScriptCS)GameObject.Find("PlayerSidesCollider").GetComponent(typeof(PlayerSidesColliderScriptCS));
		
		//check which type of menu (Custom or NGUI) to work with
		if (hInGameScriptCS.isCustomMenuEnabled())
		{
			tHUDPUMeter = (Transform)GameObject.Find("HUDMainGroup/HUDPUMeter").GetComponent(typeof(Transform));
			tHUDPUMeterBar = (Transform)GameObject.Find("HUDMainGroup/HUDPUMeter/HUD_PU_Meter_Bar_Parent").GetComponent(typeof(Transform));
			tHUDPUMeter.transform.position -= new Vector3(0, 100, 0);//hide the powerup meter
		}
		else
		{
			uisHUDPUMeter = (UISlider)GameObject.Find("UI Root (2D)/Camera/Anchor/HUDGroup/PowerupMeter/Progress Bar").GetComponent(typeof(UISlider));
			uisHUDPUMeter.transform.position = new Vector3(0,1000,0);//hide the power-up meter by default
		}
				
		fMagnetismDefaultRadius = 200;
		fMangetismRadius = 200;		//default: pull currency toward the character
		iCurrencyUnits = 0;
		
		for(var i = 0; i <iPowerupCount ; i++)
		{
			bPowerupStatus[i] = false;
			fPowerupTotalDuration[i] = 10.0f;//active time duration of the powerups
		}
	}
	
	void FixedUpdate ()
	{	
		//pause the powerup's time if the game is paused
		if(hInGameScriptCS.isGamePaused()==true)
		{
			for (int j=0; j<iPowerupCount; j++)
			{
				if (bPowerupStatus[j] == true)
					fPowerupStartTime[j] += Time.deltaTime;
			}
			return;
		}
	
		//count down timer for the active powerup
		for(int i = 0; i < iPowerupCount; i++)
		{
			if(bPowerupStatus[i]==true)
			{
				//reduce the meter bar
				PowerupHUDVisual( (Time.time - fPowerupStartTime[i]), fPowerupTotalDuration[i] );
				
				if(Time.time - fPowerupStartTime[i]>=fPowerupTotalDuration[i])//deactivate the PU when time runs out
				{
					deactivatePowerup(i);
				}
			}//end of if PU Active == true
		}//end of for i
	}
	
	/*
	*	FUNCTION: Add collected currency or activate powerup
	*	CALLED BY: PowerupScript.Update()
	*/
	public void collectedPowerup(int index)
	{
		if(index == (int)PowerUps.Currency)//if a currency unit is collected
		{
			iCurrencyUnits += 1;	//add 1 to the currency count
			hSoundManagerCS.playSound(SoundManagerCS.PowerupSounds.CurrencyCollection);//play collection sound
	
			hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.Currency);//count the collected currency for mission script
			//count the collected currency for achievement script
			hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.Currency);
			
			return;
		}
		
		fPowerupStartTime[index] = Time.time;	//set the time when powerup collected
		activatePowerUp(index);		//activate powerup if collected
		
		hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.Powerups);//count the collected powerups for mission script
		//count the collected powerups for global achievements script
		hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.Powerups);
	}
	
	/*
	*	FUNCTION: Enable the powerup's functionality
	*	CALLED BY:	collectedPowerup()
	*/
	private void activatePowerUp(int index)
	{
		//display power-up meter
		if (hInGameScriptCS.isCustomMenuEnabled())//is custom menu is in use
		{
			tHUDPUMeter.transform.position = new Vector3(tHUDPUMeter.transform.position.x,
			-88.6f, tHUDPUMeter.transform.position.z);
		}
		else
			uisHUDPUMeter.transform.position = new Vector3(0,0,0);
		
		bPowerupStatus[index] = true;
		
		if(index == (int)PowerUps.Magnetism)//Magnetism Powerup
		{
			fMangetismRadius =  fMagnetismDefaultRadius + 2300;
		}
		else if (index == (int)PowerUps.Ghost)
		{
			hPlayerFrontColliderScriptCS.deactivateCollider();
			hPlayerSidesColliderScriptCS.deactivateCollider();
		}
	}
	
	/*
	*	FUNCTION: Dactivate powerup when it time expires
	*	CALLED BY: Update()
	*/
	public void deactivatePowerup(int index)
	{			
		if (hInGameScriptCS.isCustomMenuEnabled())//is custom menu is in use
		{
			tHUDPUMeter.transform.position = new Vector3(tHUDPUMeter.transform.position.x,
			1000, tHUDPUMeter.transform.position.z);//hide the power-up meter
		}
		else
			uisHUDPUMeter.transform.position = new Vector3(0,1000,0);
		
		bPowerupStatus[index] = false;
		
		if(index == (int)PowerUps.Magnetism)//Magnetism Powerup
		{
			fMangetismRadius = fMagnetismDefaultRadius;
		}
		else if (index == (int)PowerUps.Ghost)
		{
			hPlayerFrontColliderScriptCS.activateCollider();
			hPlayerSidesColliderScriptCS.activateCollider();
		}
	}
	
	/*
	*	FUNCTION: Deactivate all active powerups
	*	CALLED BY:	InGameScript.Update()
	*/
	public void deactivateAllPowerups()
	{
		for (int i=0; i< (PowerUps.GetValues(typeof(PowerUps)).Length-2); i++)
		{
			if (bPowerupStatus[i] == true)
				deactivatePowerup(i);
		}
	}
	
	/*
	*	FUNCTION: Reduce the powerup meter's bar when a powerup is activated
	*	CALLED BY: Update()
	*/
	private float iBarLength;
	private void PowerupHUDVisual(float fCurrentTime, float fTotalTime)
	{
		if (hInGameScriptCS.isCustomMenuEnabled())
			iBarLength = tHUDPUMeterBar.transform.localScale.x;
		
		if (fCurrentTime <= 0)
			return;
			
		iBarLength = (fTotalTime-fCurrentTime)/fTotalTime;//calculate powerup meter bar's length
		//set the length
		if (hInGameScriptCS.isCustomMenuEnabled())
		{			
			tHUDPUMeterBar.transform.localScale = new Vector3(iBarLength,
			tHUDPUMeterBar.transform.localScale.y, tHUDPUMeterBar.transform.localScale.z);
		}
		else					
			uisHUDPUMeter.sliderValue = iBarLength;		
	}
	
	/*
	*	FUNCTION: Get the radius of magnetism effect
	*/
	public float getMagnetismRadius() { return fMangetismRadius; }
	
	/*
	*	FUNCTION: Get the currency collected in current run
	*/
	public int getCurrencyUnits() { return iCurrencyUnits; }
	
	/*
	*	FUNCTION: Check if any powerup is active
	*	CALLED BY:	ElementsGenerator.getRandomElement()
	*/
	public bool isPowerupActive()
	{
		for (int i=0; i<iPowerupCount; i++)
		{
			if (bPowerupStatus[i] == true)
				return true;
		}
		
		return false;
	}
	
	/*
	*	FUNCTION: Check if a particular powerup is active
	*	PARAMETER 1: The powerup which needs to be checked
	*	CALLED BY:	PowerupScript.Update()
	*/
	public bool isPowerupActive(PowerUps ePUType)
	{
		return bPowerupStatus[(int)ePUType];
	}
	
	/*
	*	FUNCTION:	Increase the duration of the power-up.
	*	CALLED BY:	ShopPowerupScript.handlerPowerupItem(...)
	*/
	public void upgradePowerup(PowerUps powerupIndex)
	{
		fPowerupTotalDuration[(int)powerupIndex] += 3;//increase the duraiton of the power-up by 3 seconds
	}
}
