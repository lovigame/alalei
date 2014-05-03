using UnityEngine;
using System.Collections;

public class NGUIMenuScript : MonoBehaviour {

	public enum NGUIMenus
	{
		MainMenu = 0, 
		PauseMenu = 1,
		GameOverMenu = 2,
		InstructionsMenu = 3,
		SettingsMenu = 4,		
		MissionsMenu = 5,
		AchievementsMenu = 6,
		
		ShopHome = 7,
		ShopCostumes = 8,
		ShopIAPs = 9,
		ShopPowerups = 10,
		ShopUtilities = 11
	}
	
	private GameObject[] goNGUIMenus;//all the NGUI menu game objects
	private GameObject goHUDGroup;//HUDGroup game object
	private UILabel uilAchievementsText;//achievement text located on the achievement menu
	private UILabel uilMissionPauseMenuText;//list of missions on the pause menu
	private UILabel uilMissionMissionMenuText;//list of missions on the missions menu
		
	private int iMenuCount;//total number of menus
	private NGUIMenus CurrentMenu;//currently active menu
	
	//variables needed for the resume game counter
	private UILabel uilPauseCounter;//countdown timer
	private int iResumeGameState = 0;
	private float iResumeGameStartTime;//when the resume button was pressed
	
	//scripts references
	private InGameScriptCS hInGameScriptCS;
	private NGUIHUDScript hNGUIHUDScript;
	
	void Start () 
	{
		//scirpt references
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hNGUIHUDScript = (NGUIHUDScript)this.transform.Find("Camera/Anchor/HUDGroup").GetComponent(typeof(NGUIHUDScript));
		
		//get the total number of menus used
		iMenuCount = System.Enum.GetValues(typeof(NGUIMenus)).Length;
		
		goHUDGroup = GameObject.Find("Camera/Anchor/HUDGroup");//gameobject of the HUD Group
		
		//get the gameobjects of all menus used for later access
		goNGUIMenus = new GameObject[iMenuCount];
		goNGUIMenus[(int)NGUIMenus.MainMenu] = GameObject.Find("Camera/Anchor/MainMenu");
		goNGUIMenus[(int)NGUIMenus.PauseMenu] = GameObject.Find("Camera/Anchor/PauseMenu");
		goNGUIMenus[(int)NGUIMenus.GameOverMenu] = GameObject.Find("Camera/Anchor/GameOverMenu");
		goNGUIMenus[(int)NGUIMenus.InstructionsMenu] = GameObject.Find("Camera/Anchor/InstructionsMenu");
		goNGUIMenus[(int)NGUIMenus.SettingsMenu] = GameObject.Find("Camera/Anchor/SettingsMenu");		
		goNGUIMenus[(int)NGUIMenus.MissionsMenu] = GameObject.Find("Camera/Anchor/MissionsMenu");
		goNGUIMenus[(int)NGUIMenus.AchievementsMenu] = GameObject.Find("Camera/Anchor/AchievementsMenu");
		
		goNGUIMenus[(int)NGUIMenus.ShopHome] = GameObject.Find("Camera/Anchor/Shop/ShopHome");
		goNGUIMenus[(int)NGUIMenus.ShopCostumes] = GameObject.Find("Camera/Anchor/Shop/ShopCostumes");
		goNGUIMenus[(int)NGUIMenus.ShopIAPs] = GameObject.Find("Camera/Anchor/Shop/ShopIAPs");
		goNGUIMenus[(int)NGUIMenus.ShopPowerups] = GameObject.Find("Camera/Anchor/Shop/ShopPowerups");
		goNGUIMenus[(int)NGUIMenus.ShopUtilities] = GameObject.Find("Camera/Anchor/Shop/ShopUtilities");
		
		for (int i=0; i<iMenuCount; i++)//disable all menu groups when game starts
			NGUITools.SetActive(goNGUIMenus[i], false);
		
		uilAchievementsText = (UILabel)goNGUIMenus[(int)NGUIMenus.AchievementsMenu].transform.
			Find("Text_Achievements").GetComponent(typeof(UILabel));
		uilPauseCounter = (UILabel)this.transform.Find("Camera/Anchor/Text_PauseCounter").GetComponent(typeof(UILabel));
		NGUITools.SetActive(uilPauseCounter.gameObject, false);
		
		uilMissionPauseMenuText = (UILabel)goNGUIMenus[(int)NGUIMenus.PauseMenu].transform.Find("Text_Missions").GetComponent(typeof(UILabel));
		uilMissionMissionMenuText = (UILabel)goNGUIMenus[(int)NGUIMenus.MissionsMenu].transform.Find("Text_Missions").GetComponent(typeof(UILabel));
		
		ShowMenu(NGUIMenus.MainMenu);//display main menu when game starts
		toggleHUDGroupState(false);
	}//end of Start function
	
	/*
	 * FUNCTION:	Return the reference of the NGUIHUDScript.cs script.
	 * 				This is used if the NGUI HUDGroup is disabled when its needed.
	 * CALLED BY:	MissionsControllerCS.Start()
	 * */
	public NGUIHUDScript getNGUIHUDScriptReference()
	{
		return hNGUIHUDScript;
	}
	
	void FixedUpdate()
	{		
		//display countdown timer on Resume
		if (iResumeGameState == 0)
			;
		else if (iResumeGameState == 1)//display the counter
		{
			NGUITools.SetActive(uilPauseCounter.gameObject, true);
			iResumeGameStartTime = (int)Time.time;
			iResumeGameState = 2;
		}
		else if (iResumeGameState == 2)//count down
		{
			uilPauseCounter.text = Mathf.Round(4 - (Time.time - iResumeGameStartTime)).ToString();
			
			if ( (Time.time - iResumeGameStartTime) >= 3)//resume the game when time expires
			{
				uilPauseCounter.text = string.Empty;
				NGUITools.SetActive(uilPauseCounter.gameObject, false);
				
				hInGameScriptCS.processClicksPauseMenu(MenuScriptCS.PauseMenuEvents.Resume);
				iResumeGameStartTime = 0;			
				iResumeGameState = 0;
			}
		}	
	}//end of fixed update
	
	/*
	 * FUNCTION:	Trigger the Fixed Update code to start resume game counter
	 * 				after which the InGameScript is signaled to resume gameplay
	 * CALLED BY:	ExitButtonHandler.OnClick()
	 * */
	public void startResumeGameCounter()
	{
		iResumeGameState = 1;//start the countdown timer
		CloseMenu(NGUIMenus.PauseMenu);
	}
	
	/*
	 * FUNCTION:	Display the list of achievemnts on the achievements menu.
	 * CALLED BY:	GlobalAchievemntControllerCS.updateMenuDescription()
	 * */
	public void updateAchievementsMenuDescription(string description)
	{
		uilAchievementsText.text = description;
	}
	
	/*
	*	FUNCTION:	Display description of the currently active
	*				missions on Pause Menu.
	*	CALLED BY:	MissionsControllerCS.updateMenuDescriptions()
	*/
	public void updatePauseMenuMissions(string description)
	{
		uilMissionPauseMenuText.text = description;
	}
	
	/*
	*	FUNCTION:	Display the currently active mission on the Mission Menu.
	*	CALLED BY:	MissionsControllerCS.updateMenuDescriptions()
	*/
	public void updateMissionsMenuMissions(string description)
	{
		uilMissionMissionMenuText.text = description;
	}
	
	/*
	 * FUNCTION:	Updates the value of the currency on the header. This
	 * 				header item is located at all shop menus.
	 * */
	public void updateCurrencyOnHeader(NGUIMenus menu)
	{
		( (UILabel)goNGUIMenus[(int)menu].transform.Find("Text_Currency").GetComponent(typeof(UILabel)) )
			.text = hInGameScriptCS.getCurrencyCount().ToString();
	}
	
	/*
	*	FUNCTION:	Enable the menu to show.
	*/
	public void ShowMenu(NGUIMenus menu)
	{	
		CurrentMenu = menu;//set the currently active menu		
		NGUITools.SetActive(goNGUIMenus[(int)menu], true);//enable the menu
		NGUITools.SetActive(goHUDGroup, false);//disable the HUD components
		
		//update the currency on the shop home menu header
		if (menu == NGUIMenus.ShopHome || menu == NGUIMenus.ShopCostumes
			|| menu == NGUIMenus.ShopIAPs || menu == NGUIMenus.ShopPowerups
			|| menu == NGUIMenus.ShopUtilities)
			updateCurrencyOnHeader(menu);
	}
	
	/*
	*	FUNCTION: Disable the menu to close
	*/
	public void CloseMenu(NGUIMenus menu)
	{
		NGUITools.SetActive(goNGUIMenus[(int)menu], false);//disable the menu
	}
	
	/*
	*	FUNCTION:	Enable/ disable the current script.
	*	ADDITIONA INFO:	The NGUIMenuScript.js is disabled during gameplay to improve
	*					performance.
	*/
	public void NGUIMenuScriptEnabled(bool state)
	{
		this.enabled = state;
	}
	
	/*
	*	FUNCTION:	Dispaly or hide HUD depeding upon the state the game is in;
	*				HUD is not displayed when menus are active.
	*/
	public void toggleHUDGroupState(bool state)
	{
		NGUITools.SetActive(goHUDGroup, state);
		hNGUIHUDScript.toggleHUDGroupState(state);
	}
	
	/*
	 * FUNCTION:	Get the currently active menu.
	 * RETURNS:		The current menu enum object.
	 * */
	public NGUIMenus getCurrentMenu()
	{
		return CurrentMenu;
	}
}