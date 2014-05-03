#pragma strict
/*
*	FUNCITON:
*	- This script holds the global variables that the other scripts are dependent on.
*	- It provides interaction among scripts.
*	- This script controls game states (launch, pause, death etc.)
*
*	USED BY: This script is a part of the "Player" prefab.
*
*/

private var CurrentEnergy : int = 100;	//player's energy (set to zero on death)
private var iLevelScore : int = 0;	//current score (calculated based on distance traveled)
private var iCurrencyCount:int = 9000;//user's cash in hand

//script references
private var hMenuScript : MenuScript;
private var hControllerScript : ControllerScript;
private var hSoundManager : SoundManager;
private var hPowerupsMainController : PowerupsMainController;
private var hEnemyController : EnemyController;
private var hCameraController : CameraController;
private var hMissionsController : MissionsController;
private var hGlobalAchievementController : GlobalAchievementController;

private var iPauseStatus : int = 0;
private var iDeathStatus : int = 0;
private var iMenuStatus : int;

private var bGameOver : boolean = false;
private var bGamePaused : boolean = false;

function Start()
{
	//PlayerPrefs.DeleteAll();	//DEBUG
	Application.targetFrameRate = 60;		//ceiling the frame rate on 60 (debug only)
	
	RenderSettings.fog = true;				//turn on fog on launch
	
	hMenuScript = GameObject.Find("MenuGroup").GetComponent(MenuScript) as MenuScript;
	hSoundManager = GameObject.Find("SoundManager").GetComponent(SoundManager) as SoundManager;	
	hControllerScript = this.GetComponent(ControllerScript) as ControllerScript;
	hPowerupsMainController = this.GetComponent(PowerupsMainController) as PowerupsMainController;
	hCameraController = GameObject.Find("Main Camera").GetComponent(CameraController) as CameraController;
	hEnemyController = this.GetComponent(EnemyController) as EnemyController;
	hMissionsController = GameObject.Find("Player").GetComponent(MissionsController) as MissionsController;
	hGlobalAchievementController = GameObject.Find("Player").GetComponent(GlobalAchievementController) as GlobalAchievementController;

	CurrentEnergy = 100;
	iPauseStatus = 0;
	iMenuStatus = 1;
	
	bGameOver = false;
	bGamePaused = true;
}//end of Start

function Update()
{	
	if (iMenuStatus == 0)	//normal gameplay
		;
	else if (iMenuStatus == 1)//display main menu and pause game
	{		
		hMenuScript.setMenuScriptStatus(true);
				
		bGamePaused = true;
		iMenuStatus = 2;
	}
	
	//Pause GamePlay
	if(iPauseStatus==0)	//normal gameplay
		;
	else if(iPauseStatus==1)//pause game
	{	
		hMenuScript.setMenuScriptStatus(true);		
		hMenuScript.displayPauseMenu();
		
		iPauseStatus = 2;
	}
	else if(iPauseStatus==3)//resume game
	{		
		bGamePaused = false;		
		hMenuScript.setMenuScriptStatus(false);
		
		iPauseStatus = 0;
	}
	
	if(iDeathStatus==0)	//normal gameplay
		;
	else if(iDeathStatus==1)//call death menu
	{
		hPowerupsMainController.deactivateAllPowerups();	//deactivate if a powerup is enabled
		
		iDeathStatus = 2;
	}
	else if (iDeathStatus == 2)
	{	
		hMenuScript.setMenuScriptStatus(true);
		hMenuScript.displayGameOverMenu();	//display the Game Over menu
		
		iDeathStatus = 0;
	}
	
	if (bGamePaused == true)
		return;
	
}//end of Update()

/*
*	FUNCTION: Pause the game
*	CALLED BY: ControllerScript.getClicks()
*/
public function pauseGame()
{
	if (!hControllerScript.isMechAnimEnabled())
		hControllerScript.togglePlayerAnimation(false);//stop character animations
	
	bGamePaused = true;//signal all scripts to pause
	iPauseStatus = 1;
	
	hSoundManager.stopAllSounds();
	PlayerPrefs.Save();//save changes in player prefs
}

/*
*	FUNCTION: start the gameplay and display all related elements
*	CALLED BY: MenuScript.MainMenuGui()
*			   MenuScript.MissionsGui()
*/
public function launchGame()
{	
	iMenuStatus = 0;
	bGamePaused = false;
		
	hMenuScript.showHUDElements();		
	hControllerScript.launchGame();
	hCameraController.launchGame();
	
	//count how many time the game has started
	hMissionsController.incrementMissionCount(MissionTypes.StartGame);
	hGlobalAchievementController.incrementAchievementCount(GlobalAchievementTypes.StartGame);
}

/*
*	FUNCTION: Display death menu and end game
*	CALLED BY:	ControllerScript.DeathScene()
*/
public function setupDeathMenu()
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
public function processClicksPauseMenu(index : PauseMenuEvents)
{
	if (index == PauseMenuEvents.MainMenu)
		Application.LoadLevel(0);
	else if (index == PauseMenuEvents.Resume)
	{		
		hMenuScript.showHUDElements();		
		iPauseStatus = 3;
		
		if (!hControllerScript.isMechAnimEnabled())
			hControllerScript.togglePlayerAnimation(true);//pause legacy animations
	}
}

/*
*	FUNCTION: Execute a function based on button press in Death Menu
*	CALLED BY: MenuScript.GameOverMenu()
*/
public function procesClicksDeathMenu(index : GameOverMenuEvents)
{
	if (index == GameOverMenuEvents.Play)
		Application.LoadLevel(0);
	else if (index == GameOverMenuEvents.Back)	
		Application.LoadLevel(0);
}//end of DM_ProcessClicks

/*
*	FUNCTION: Is called when a collision occurs
*	CALLED BY:	PlayerFrontColliderScript.OnCollisionEnter
*				processStumble()
*/
public function collidedWithObstacle()
{
	decrementEnergy(100);		// deduct energy after collision
	hCameraController.setCameraShakeImpulseValue(5);
}//end of Collided With Obstacle

/*
*	FUNCTION: Pause game if application closed/ switched on device
*/
function OnApplicationPause (pause : boolean) : void
{
	Debug.Log("Application Paused : "+pause);
	if(Application.isEditor==false)
	{
		if(bGamePaused==false&&pause==false)
		{
			pauseGame();
		}
	}	
}

//paused state
public function isGamePaused() { return bGamePaused; }

//score
public function getLevelScore() { return iLevelScore; }
public function incrementLevelScore(iValue:int) { iLevelScore += iValue; }

//energy
public function getCurrentEnergy() { return CurrentEnergy; }
public function isEnergyZero():boolean { return (CurrentEnergy <= 0 ? true : false); }
public function decrementEnergy(iValue:int) { CurrentEnergy -= iValue; }

//currency
public function getCurrencyCount() { return iCurrencyCount; }
public function alterCurrencyCount(iVal:int) { iCurrencyCount+=iVal; }//increment or decrement currency