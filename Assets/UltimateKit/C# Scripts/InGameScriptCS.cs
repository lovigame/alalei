/*
*	FUNCITON:
*	- This script holds the global variables that the other scripts are dependent on.
*	- It provides interaction among scripts.
*	- This script controls game states (launch, pause, death etc.)
*
*	USED BY: This script is a part of the "Player" prefab.
*
*/

using UnityEngine;
using System.Collections;

public class InGameScriptCS : MonoBehaviour {

	private int CurrentEnergy = 100;	//player's energy (set to zero on death)
	private int iLevelScore = 0;	//current score (calculated based on distance traveled)
	private int iCurrencyCount = 9000;//
	private bool customMenuEnabled = false;
	
	//script references
	private MenuScriptCS hMenuScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
	private ControllerScriptCS hControllerScriptCS;
	private SoundManagerCS hSoundManagerCS;
	private PowerupsMainControllerCS hPowerupsMainControllerCS;
	private EnemyControllerCS hEnemyControllerCS;
	private CameraControllerCS hCameraControllerCS;
	private MissionsControllerCS hMissionsControllerCS;
	private GlobalAchievementControllerCS hGlobalAchievementControllerCS;
	
	private int iPauseStatus = 0;
	private int iDeathStatus = 0;
	private int iMenuStatus;
	
	private bool bGameOver = false;
	private bool bGamePaused = false;
	
	// Use this for initialization
	void Start () 
	{
		//PlayerPrefs.DeleteAll();	//DEBUG
		Application.targetFrameRate = 60;		//ceiling the frame rate on 60 (debug only)
		
		RenderSettings.fog = true;				//turn on fog on launch
		
		if (GameObject.Find("MenuGroup"))//check while type of menu is active (custom or ngui)
		{
			customMenuEnabled = true;
			hMenuScriptCS = (MenuScriptCS)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScriptCS));
		}
		else
			hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
			
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
		hControllerScriptCS = (ControllerScriptCS)this.GetComponent(typeof(ControllerScriptCS));
		hPowerupsMainControllerCS = (PowerupsMainControllerCS)this.GetComponent(typeof(PowerupsMainControllerCS));
		hCameraControllerCS = (CameraControllerCS)GameObject.Find("Main Camera").GetComponent(typeof(CameraControllerCS));
		hEnemyControllerCS = (EnemyControllerCS)this.GetComponent(typeof(EnemyControllerCS));
		hMissionsControllerCS = (MissionsControllerCS)this.GetComponent(typeof(MissionsControllerCS));
		hGlobalAchievementControllerCS = (GlobalAchievementControllerCS)this.GetComponent(typeof(GlobalAchievementControllerCS));
		
		CurrentEnergy = 100;
		iPauseStatus = 0;
		iDeathStatus = 0;
		iMenuStatus = 1;
		
		bGameOver = false;
		bGamePaused = true;
	}
	
	void Update()
	{	
		if (iMenuStatus == 0)	//normal gameplay
			;
		else if (iMenuStatus == 1)//display main menu and pause game
		{	
			if (isCustomMenuEnabled())
				hMenuScriptCS.setMenuScriptStatus(true);
			else
				hNGUIMenuScript.NGUIMenuScriptEnabled(true);
					
			bGamePaused = true;
			iMenuStatus = 2;
		}
		
		//Pause GamePlay
		if(iPauseStatus == 1)//pause game
		{	
			if (isCustomMenuEnabled())
			{
				hMenuScriptCS.setMenuScriptStatus(true);
				hMenuScriptCS.displayPauseMenu();
			}
			else
			{
				hNGUIMenuScript.NGUIMenuScriptEnabled(true);
				hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.PauseMenu);
			}			
			
			iPauseStatus = 2;
		}
		else if(iPauseStatus==3)//resume game
		{		
			if (isCustomMenuEnabled())
				hMenuScriptCS.setMenuScriptStatus(false);
			else
				hNGUIMenuScript.NGUIMenuScriptEnabled(false);
			
			bGamePaused = false;
			iPauseStatus = 0;
		}
		
		if(iDeathStatus==1)//call death menu
		{
			hPowerupsMainControllerCS.deactivateAllPowerups();	//deactivate if a powerup is enabled
			
			iDeathStatus = 2;
		}
		else if (iDeathStatus == 2)
		{	
			if (isCustomMenuEnabled())//if custom menu is in use
			{
				hMenuScriptCS.setMenuScriptStatus(true);
				hMenuScriptCS.displayGameOverMenu();	//display the Game Over menu
			}
			else//if NGUI menu is in use
			{
				hNGUIMenuScript.NGUIMenuScriptEnabled(true);
				hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.GameOverMenu);				
			}
						
			iDeathStatus = 0;
		}
		
		if (bGamePaused == true)
			return;
		
	}//end of Update()
	
	/*
	*	FUNCTION: Pause the game
	*	CALLED BY: ControllerScript.getClicks()
	*/
	public void pauseGame()
	{
		if (!hControllerScriptCS.isMechAnimEnabled())//if legacy animaitons are enabled
			hControllerScriptCS.togglePlayerAnimation(false);//stop character animations
		
		bGamePaused = true;//signal all scripts to pause
		iPauseStatus = 1;
		
		hSoundManagerCS.stopAllSounds();//stop all sounds
		PlayerPrefs.Save();//save changes in player prefs
	}
	
	/*
	*	FUNCTION: start the gameplay and display all related elements
	*	CALLED BY: MenuScript.MainMenuGui()
	*			   MenuScript.MissionsGui()
	*/
	public void launchGame()
	{	
		iMenuStatus = 0;
		bGamePaused = false;//tell all scripts to resume
		
		if (isCustomMenuEnabled())//if custom menu is in use
			hMenuScriptCS.showHUDElements();
		else//if NGUI menu is in use
			hNGUIMenuScript.toggleHUDGroupState(true);
			
		hControllerScriptCS.launchGame();//tell the ControllerScriptCS to start game
		hCameraControllerCS.launchGame();//tell the CameraControllerCS to start game
		
		//count how many time the game has started
		hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.StartGame);
		hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.StartGame);
	}
	
	/*
	*	FUNCTION: Display death menu and end game
	*	CALLED BY:	ControllerScript.DeathScene()
	*/
	public void setupDeathMenu()
	{	
		bGameOver = true;
		bGamePaused = true;	
		iDeathStatus = 1;
		
		PlayerPrefs.Save();//save changes in player prefs
	}//end of Setup Death Menu
	
	/*
	*	FUNCTION: Execute a function based on button press in Pause Menu
	*	CALLED BY: MenuScript.PauseMenu()
	*/
	public void processClicksPauseMenu(MenuScriptCS.PauseMenuEvents index)
	{
		if (index == MenuScriptCS.PauseMenuEvents.MainMenu)
			Application.LoadLevel(0);
		else if (index == MenuScriptCS.PauseMenuEvents.Resume)
		{	
			if (isCustomMenuEnabled())
				hMenuScriptCS.showHUDElements();
			else
				hNGUIMenuScript.toggleHUDGroupState(true);
			
			iPauseStatus = 3;
			
			if (!hControllerScriptCS.isMechAnimEnabled())//if legacy animation is enabled
				hControllerScriptCS.togglePlayerAnimation(true);//pause legacy animations
		}
	}//end of process click pause menu
	
	/*
	*	FUNCTION: Execute a function based on button press in Death Menu
	*	CALLED BY: MenuScript.GameOverMenu()
	*/
	public void procesClicksDeathMenu(MenuScriptCS.GameOverMenuEvents index)
	{
		if (index == MenuScriptCS.GameOverMenuEvents.Play)
			Application.LoadLevel(0);
		else if (index == MenuScriptCS.GameOverMenuEvents.Back)	
			Application.LoadLevel(0);
	}//end of DM_ProcessClicks
	
	/*
	*	FUNCTION: Is called when a collision occurs
	*	CALLED BY:	PlayerFrontColliderScript.OnCollisionEnter
	*				processStumble()
	*/
	public void collidedWithObstacle()
	{
		decrementEnergy(100);		// deduct energy after collision
		hCameraControllerCS.setCameraShakeImpulseValue(5);
	}//end of Collided With Obstacle
	
	/*
	*	FUNCTION: Pause game if application closed/ switched on device
	*/
	void OnApplicationPause (bool pause)
	{
		Debug.Log("Application Paused : "+pause);
		if(Application.isEditor==false)
		{
			if(bGamePaused==false&&pause==false)
			{
				pauseGame();
			}
		}	
	}//end of OnApplication function
	
	//paused state
	public bool isGamePaused() { return bGamePaused; }
	
	//score
	public int getLevelScore() { return iLevelScore; }
	public void incrementLevelScore(int iValue) { iLevelScore += iValue; }
	
	//energy
	public int getCurrentEnergy() { return CurrentEnergy; }
	public bool isEnergyZero() { return (CurrentEnergy <= 0 ? true : false); }
	public void decrementEnergy(int iValue) { CurrentEnergy -= iValue; }
	
	//currency
	public int getCurrencyCount() { return iCurrencyCount; }
	public void alterCurrencyCount(int iVal) { iCurrencyCount+=iVal; }//increment or decrement currency
	
	//check if the custom or NGUI is enabled
	public bool isCustomMenuEnabled() { return customMenuEnabled; }
}
