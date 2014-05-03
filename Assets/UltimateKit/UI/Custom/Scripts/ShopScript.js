#pragma strict
/*
*	FUNCTION:
*	- Defines the implementation of the complete shop.
*	
*	INFO:
*	- 	The shop does not keep a permanent record of the purchased and 
*		equiped items. Everything will be reset when the scene is restarted.
*/

public enum ShopMenus
{
	ShopHome = 0,
	Costumes = 1,
	Powerups = 2,
	Utilities = 3,
	IAPs = 4
}

//names of the available skins
public enum CharacterConstumes
{
	Prisoner = 0,
	Casual = 1
}

private var hit : RaycastHit;//used for detecting taps
private var HUDCamera : Camera;//the HUD/Menu orthographic camera

//script references
private var hMenuScript : MenuScript;
private var hInGameScript : InGameScript;
private var hSoundManager : SoundManager;

private var iAndroidBackTapState : int = 0;//state of tap on back button on android devices
private var iTapState : int=0;//state of tap on screen
private var tTappedButtonTransform:Transform;
private var CurrentMenu : int = -1;//currently active shop sub-menu
private var tShopMenuTransforms : Transform[];//transforms of the shop sub-menus

//shop main screen
private var tShopHomeButtons : Transform[];
private var iShopHomeButtonCount:int = 5;
//shop costumes
private var tShopCostumesButtons : Transform[];//transforms of buttons in the costume sub-menu
private var iShopCostumesButtonCount : int = 1;//total number of buttons in the costume sub-menu
//shop powerups
private var tShopPowerupsButtons : Transform[];
private var iShopPowerupsButtonsCount : int = 1;
//shop utilities
private var tShopUtilitiesButtons : Transform[];
private var iShopUtilitiesButtonCount : int = 1;
//shop IAPs
private var tShopIAPButtons : Transform[];
private var iShopIAPButtonsCount : int = 1;

//elements list scroll
private var scrollSensitivity : float = 0.2;//how fast to scroll with swipe gesture
private var iScrollState : int = 0;
private var previousTapLocation : float;
private var currentTapLocation : float;
private var currentTab : Transform;//the list to scroll

private var scrollUpperLimit:float;
private var scrollLowerLimit:float;

function Start () 
{
	HUDCamera = GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(Camera) as Camera;
	hMenuScript = GameObject.Find("MenuGroup").GetComponent(MenuScript) as MenuScript;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hSoundManager = GameObject.Find("SoundManager").GetComponent(SoundManager) as SoundManager;
	
	tShopMenuTransforms = new Transform[ShopMenus.GetValues(ShopMenus).Length];
	tShopMenuTransforms[ShopMenus.ShopHome] = this.transform.Find("ShopHome").GetComponent(Transform) as Transform;
	
	//shop primary menu
	tShopHomeButtons = new Transform[iShopHomeButtonCount];
	tShopHomeButtons[0] = tShopMenuTransforms[ShopMenus.ShopHome].Find("Buttons/Button_Back").GetComponent(Transform) as Transform;
	tShopHomeButtons[1] = tShopMenuTransforms[ShopMenus.ShopHome].Find("Buttons/Button_Costumes").GetComponent(Transform) as Transform;
	tShopHomeButtons[2] = tShopMenuTransforms[ShopMenus.ShopHome].Find("Buttons/Button_Powerups").GetComponent(Transform) as Transform;
	tShopHomeButtons[3] = tShopMenuTransforms[ShopMenus.ShopHome].Find("Buttons/Button_Utilities").GetComponent(Transform) as Transform;
	tShopHomeButtons[4] = tShopMenuTransforms[ShopMenus.ShopHome].Find("Buttons/Button_MoreCoins").GetComponent(Transform) as Transform;
	
	//shop costumes menu
	tShopMenuTransforms[ShopMenus.Costumes] = this.transform.Find("CostumesShop").GetComponent(Transform) as Transform;
	tShopCostumesButtons = new Transform[iShopCostumesButtonCount];
	tShopCostumesButtons[0] = tShopMenuTransforms[ShopMenus.Costumes].Find("Button_Back").GetComponent(Transform) as Transform;
	
	//shop powerups menu
	tShopMenuTransforms[ShopMenus.Powerups] = this.transform.Find("PowerupsShop").GetComponent(Transform) as Transform;
	tShopPowerupsButtons = new Transform[iShopPowerupsButtonsCount];
	tShopPowerupsButtons[0] = tShopMenuTransforms[ShopMenus.Powerups].Find("Button_Back").GetComponent(Transform) as Transform;
	
	//shop utilities menu
	tShopMenuTransforms[ShopMenus.Utilities] = this.transform.Find("UtilitiesShop").GetComponent(Transform) as Transform;
	tShopUtilitiesButtons = new Transform[iShopUtilitiesButtonCount];
	tShopUtilitiesButtons[0] = tShopMenuTransforms[ShopMenus.Utilities].Find("Button_Back").GetComponent(Transform) as Transform;
	
	//shop IAP
	tShopMenuTransforms[ShopMenus.IAPs] = this.transform.Find("MoreCoinsShop").GetComponent(Transform) as Transform;
	tShopIAPButtons = new Transform[iShopIAPButtonsCount];
	tShopIAPButtons[0] = tShopMenuTransforms[ShopMenus.IAPs].Find("Button_Back").GetComponent(Transform) as Transform;
	
	//vertical scroll initilization
	iScrollState = 0;
	
	setShopScriptEnabled(false);
}

function OnGUI ()
{
	listenerClicks();//listen for clicks on the menu
	
	if (currentTab != null)
		scrollVertical();//make the current list scroll
		
	if (Application.platform == RuntimePlatform.Android || Application.isEditor)
		handlerAndroidBackButton();
}

/*
*	FUNCTION:	Listen for clicks on the menus and call the relevant handler function on click.
*	CALLED BY:	FixedUpdate()
*/
private function listenerClicks()
{	
	if (Input.GetMouseButtonDown(0) && iTapState == 0)//detect taps
	{	
		iTapState = 1;
		
	}//end of if get mouse button	
	else if (iTapState == 1)
	{
		if (Input.GetMouseButtonUp(0))
			iTapState = 2;
	}
	else if (iTapState == 2)//call relevent handler
	{
		if (Physics.Raycast(HUDCamera.ScreenPointToRay(Input.mousePosition),hit))//if a button has been tapped		
		{
			tTappedButtonTransform = hit.transform;
			//call the listner function of the active menu
			if (CurrentMenu == ShopMenus.ShopHome)
				handlerShopMenu(tTappedButtonTransform);
			else if (CurrentMenu == ShopMenus.Costumes)
				handlerCostumesShop(tTappedButtonTransform);
			else if (CurrentMenu == ShopMenus.Powerups)
				handlerPowerupShop(tTappedButtonTransform);
			else if (CurrentMenu == ShopMenus.Utilities)
				handlerUtilitiesShop(tTappedButtonTransform);
			else if (CurrentMenu == ShopMenus.IAPs)
				handlerIAPShop(tTappedButtonTransform);
		}//end of if raycast
			
		iTapState = 0;
	}//end of tap state 2
}//end of listener clicks function

/*
 * FUNCTION:	Handler back button execution on android devices.
 * 
 * CALLED BY:	OnGUI()
 * */
private function handlerAndroidBackButton():void
{
	if (Input.GetKeyDown(KeyCode.Escape) && iAndroidBackTapState == 0)
	{
		iAndroidBackTapState = 1;
	}//end of if input
	else if (iAndroidBackTapState == 1)
	{
		if (Input.GetKeyUp(KeyCode.Escape))
			iAndroidBackTapState = 2;
	}
	else if (iAndroidBackTapState == 2)
	{			
		if (CurrentMenu == ShopMenus.Costumes)
			handlerCostumesShop(tShopCostumesButtons[0]);
		else if (CurrentMenu == ShopMenus.IAPs)
			handlerIAPShop(tShopIAPButtons[0]);
		else if (CurrentMenu == ShopMenus.Powerups)
			handlerPowerupShop(tShopPowerupsButtons[0]);
		else if (CurrentMenu == ShopMenus.ShopHome)
			handlerShopMenu(tShopHomeButtons[0]);
		else if (CurrentMenu == ShopMenus.Utilities)
			handlerUtilitiesShop(tShopUtilitiesButtons[0]);
		
		iAndroidBackTapState = 0;
	}
}//end of handler android back button function

/*
*	FUNCTION:	Perform function according to the clicked button.
*	CALLED BY:	listenerClicks()
*/
private function handlerShopMenu(buttonTransform:Transform)
{
	if (buttonTransform == tShopHomeButtons[0])//back button
	{			
		hMenuScript.setMenuScriptStatus(true);
		setShopScriptEnabled(false);
	}
	else if (buttonTransform == tShopHomeButtons[1])//costumes button
	{
		CloseMenu(ShopMenus.ShopHome);
		ShowMenu(ShopMenus.Costumes);
	}
	else if (buttonTransform == tShopHomeButtons[2])//powerups button
	{
		CloseMenu(ShopMenus.ShopHome);
		ShowMenu(ShopMenus.Powerups);
	}
	else if (buttonTransform == tShopHomeButtons[3])//utilities button
	{
		CloseMenu(ShopMenus.ShopHome);
		ShowMenu(ShopMenus.Utilities);
	}
	else if (buttonTransform == tShopHomeButtons[4])//IAP button
	{
		CloseMenu(ShopMenus.ShopHome);
		ShowMenu(ShopMenus.IAPs);
	}
}//end of handler shop menu function

/*
*	FUNCTION:	Perform functions according to the button clicked.
*	CALLED BY:	listenerClicks()
*/
private function handlerCostumesShop(buttonTransform:Transform)
{
	if (buttonTransform == tShopCostumesButtons[0])
	{
		CloseMenu(ShopMenus.Costumes);
		ShowMenu(ShopMenus.ShopHome);
	}
}

/*
*	FUNCTION:	Perform functions according to the button clicked.
*	CALLED BY:	listenerClicks()
*/
private function handlerPowerupShop(buttonTransform:Transform)
{
	if (buttonTransform == tShopPowerupsButtons[0])
	{
		CloseMenu(ShopMenus.Powerups);
		ShowMenu(ShopMenus.ShopHome);
	}
}

/*
*	FUNCTION:	Perform functions according to the button clicked.
*	CALLED BY:	listenerClicks()
*/
private function handlerUtilitiesShop(buttonTransform:Transform)
{
	if (buttonTransform == tShopUtilitiesButtons[0])
	{
		CloseMenu(ShopMenus.Utilities);
		ShowMenu(ShopMenus.ShopHome);
	}
}

/*
*	FUNCTION:	Perform functions according to the button clicked.
*	CALLED BY:	listenerClicks()
*/
private function handlerIAPShop(buttonTransform:Transform)
{
	if (buttonTransform == tShopIAPButtons[0])
	{
		CloseMenu(ShopMenus.IAPs);
		ShowMenu(ShopMenus.ShopHome);
	}
}

/*
*	FUNCTION:	Get the currency the user has and display on the 
*				Shop's header.
*/
public function updateCurrencyOnHeader()
{
	if (CurrentMenu != -1)//check if a menu is currently active
		(tShopMenuTransforms[CurrentMenu].Find("CurrencyGroup/Text_Currency")
		.GetComponent("TextMesh") as TextMesh).text = hInGameScript.getCurrencyCount().ToString();
}

/*
*	FUNCTION:	Take the user to the main menu to display the costume.
*				This funciton is called if user clicks equip button in the costumes menu.
*/
public function displayEquippedCostume()
{
	CloseMenu(ShopMenus.Costumes);
	hMenuScript.setMenuScriptStatus(true);
	setShopScriptEnabled(false);
}

/*
*	FUNCTION: Set the menu to show in front of the HUD Camera
*/
private function ShowMenu(index:int)
{
	CurrentMenu = index;//set the active menu to the currently displayed
	hSoundManager.playSound(MenuSounds.ButtonTap);//play tap sound
	updateCurrencyOnHeader();//update currency on header
	
	//display the menu based on index
	if (index == ShopMenus.ShopHome)
		tShopMenuTransforms[ShopMenus.ShopHome].position.y = 0;
	else if (index == ShopMenus.Costumes)
	{
		var itemCount:int=0;
		
		//set the items's parent as the currentTab to make the vertical scroll code work
		currentTab = tShopMenuTransforms[ShopMenus.Costumes].Find("CostumeItemGroup").GetComponent(Transform) as Transform;		
				
		//enable the ShopCostumeScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("CostumeItem"))
			{
				(costumeItem.GetComponent(ShopCostumeScript) as ShopCostumeScript).setShopCostumeScriptEnabled(true);
				itemCount++;//count the number of costumes in the costume shop menu
			}
		
		currentTab.localPosition.y = 0;//reset the scrollable list location
		//set the upper and lower limit of the scroll
		scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
		//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
				
		tShopMenuTransforms[ShopMenus.Costumes].position.y = 0;
	}
	else if (index == ShopMenus.Powerups)
	{
		itemCount=0;
		//set the items's parent as the currentTab to make the vertical scroll code work
		currentTab = tShopMenuTransforms[ShopMenus.Powerups].Find("PowerupsItemGroup").GetComponent(Transform) as Transform;		
				
		//enable the ShopPowerupScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("PowerupItem"))
			{
				(costumeItem.GetComponent(ShopPowerupScript) as ShopPowerupScript).setShopPowerupScriptEnabled(true);
				itemCount++;//count the number of power-ups in power-up shop menu
			}
		
		currentTab.localPosition.y = 0;//reset the scrollable list location
		//set the upper and lower limit of the scroll
		scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
		//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		
		tShopMenuTransforms[ShopMenus.Powerups].position.y = 0;
	}
	else if (index == ShopMenus.Utilities)
	{
		itemCount=0;
		//set the items's parent as the currentTab to make the vertical scroll code work
		currentTab = tShopMenuTransforms[ShopMenus.Utilities].Find("UtilitiesItemGroup").GetComponent(Transform) as Transform;		
				
		//enable the ShopUtilityScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("UtilityItem"))
			{
				(costumeItem.GetComponent(ShopUtilityScript) as ShopUtilityScript).setShopUtilityScriptEnabled(true);
				itemCount++;//count the number of utilities in utility shop menu
			}
		
		currentTab.localPosition.y = 0;//reset the scrollable list location
		//set the upper and lower limit of the scroll
		scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
		//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		
		tShopMenuTransforms[ShopMenus.Utilities].position.y = 0;
	}
	else if (index == ShopMenus.IAPs)
	{
		itemCount=0;
		//set the items's parent as the currentTab to make the vertical scroll code work
		currentTab = tShopMenuTransforms[ShopMenus.IAPs].Find("IAPItemGroup").GetComponent(Transform) as Transform;		
				
		//enable the ShopIAPScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("IAPItem"))
			{
				(costumeItem.GetComponent(ShopIAPScript) as ShopIAPScript).setShopIAPScriptEnabled(true);//enable script
				itemCount++;//count the number of items in more coins shop menu
			}
		
		currentTab.localPosition.y = 0;//reset the scrollable list location
		//set the upper and lower limit of the scroll
		scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
		//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		
		tShopMenuTransforms[ShopMenus.IAPs].position.y = 0;//move the more coins menu in front of the HUD camera
	}//end of else if IAPs
}//end of Show Menu function

/*
*	FUNCTION: Send the menu away from the HUD Camera
*/
private function CloseMenu(index:int)
{
	if (index == ShopMenus.ShopHome)
		tShopMenuTransforms[ShopMenus.ShopHome].position.y = 1000;//hide the menu
	else if (index == ShopMenus.Costumes)
	{
		//disable the ShopCostumeScript of all the costume elements (to improve performance)
		currentTab = tShopMenuTransforms[ShopMenus.Costumes].Find("CostumeItemGroup").GetComponent(Transform) as Transform;
		
		//enable the ShopCostumeScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("CostumeItem"))
				(costumeItem.GetComponent(ShopCostumeScript) as ShopCostumeScript).setShopCostumeScriptEnabled(false);
				
		tShopMenuTransforms[ShopMenus.Costumes].position.y = 1000;//hide the menu
	}
	else if (index == ShopMenus.Powerups)
	{
		//disable the ShopPowerupScript of all the costume elements (to improve performance)
		currentTab = tShopMenuTransforms[ShopMenus.Powerups].Find("PowerupsItemGroup").GetComponent(Transform) as Transform;
		
		//enable the ShopPowerupScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("PowerupItem"))
				(costumeItem.GetComponent(ShopPowerupScript) as ShopPowerupScript).setShopPowerupScriptEnabled(false);
		
		tShopMenuTransforms[ShopMenus.Powerups].position.y = 1000;//hide the menu
	}
	else if (index == ShopMenus.Utilities)
	{
		//disable the ShopUtilityScript of all the costume elements (to improve performance)
		currentTab = tShopMenuTransforms[ShopMenus.Utilities].Find("UtilitiesItemGroup").GetComponent(Transform) as Transform;
				
		//enable the ShopUtilityScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("UtilityItem"))
				(costumeItem.GetComponent(ShopUtilityScript) as ShopUtilityScript).setShopUtilityScriptEnabled(false);
		
		tShopMenuTransforms[ShopMenus.Utilities].position.y = 1000;//hide the menu
	}
	else if (index == ShopMenus.IAPs)
	{
		//disable the ShopIAPScript of all the in-app purchase elements (to improve performance)
		currentTab = tShopMenuTransforms[ShopMenus.IAPs].Find("IAPItemGroup").GetComponent(Transform) as Transform;
		
		//enable the ShopIAPScript of all the costume elements
		for (var costumeItem:Transform in currentTab)
			if (costumeItem.name.Contains("IAPItem"))
				(costumeItem.GetComponent(ShopIAPScript) as ShopIAPScript).setShopIAPScriptEnabled(false);
				
		tShopMenuTransforms[ShopMenus.IAPs].position.y = 1000;//hide the menu
	}
		
	currentTab = null;//disable scroll function
	CurrentMenu = -1;
}//end of Close Menu function

/*
*	FUNCTION:	Make the menu items scroll vertically.
*	CALLED BY:	OnGUI()
*/
private function scrollVertical()
{	
	if (iScrollState == 0 && Input.GetMouseButtonDown(0))//if tapped for scroll
	{
		previousTapLocation = Input.mousePosition.y;
		currentTapLocation = previousTapLocation;
		
		iScrollState = 1;
	}	
	else if (iScrollState == 1)//check swipe direction and movement speed
	{	
		//exit scroll state 2 if finger is lifted
		if (Input.GetMouseButtonUp(0))
		{	
			iScrollState = 0;			
		}
		
		previousTapLocation = currentTapLocation;
		currentTapLocation = Input.mousePosition.y;
				
		var deltaY:float = previousTapLocation-currentTapLocation;
		
		if (Mathf.Abs(deltaY) > 1)//if the finger has been lifted off the screen
		{
			var movementFactor:float = Mathf.Abs(deltaY) * scrollSensitivity;
		
			if (deltaY < 0)//swipe up
				currentTab.localPosition.y = Mathf.Clamp(currentTab.localPosition.y+movementFactor, scrollLowerLimit, scrollUpperLimit);
			else if (deltaY > 0)//swipe down				
				currentTab.localPosition.y = Mathf.Clamp(currentTab.localPosition.y-movementFactor, scrollLowerLimit, scrollUpperLimit);			
		}//end of if
	}//end of if
}//end of scroll vertical function

/*
*	FUNCITON:	Activate or deactivate the ShopScript
*	CALLED BY:	handlerShopMenu()
*				MenuScript.handlerMainMenu(...)
*/
public function setShopScriptEnabled(status:boolean)
{
	if (status == true)
	{
		this.enabled = status;
		ShowMenu(ShopMenus.ShopHome);
	}
	else if (status == false)
	{
		this.enabled = status;
		CloseMenu(ShopMenus.ShopHome);
		hMenuScript.ShowMenu(MenuIDs.MainMenu);
	}
}//end of set shop script enabled function