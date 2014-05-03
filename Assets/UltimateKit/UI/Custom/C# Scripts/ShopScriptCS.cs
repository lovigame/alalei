/*
*	FUNCTION:
*	- Defines the implementation of the complete shop.
*	
*	INFO:
*	- 	The shop does not keep a permanent record of the purchased and 
*		equiped items. Everything will be reset when the scene is restarted.
*/
using UnityEngine;
using System.Collections;

public enum ShopMenus
{
	ShopHome = 0,
	Costumes = 1,
	Powerups = 2,
	Utilities = 3,
	IAPs = 4
}

public class ShopScriptCS : MonoBehaviour {

	//names of the available skins
	public enum CharacterConstumes
	{
		Prisoner = 0,
		Casual = 1
	}
	
	private RaycastHit hit;//used for detecting taps
	private Camera HUDCamera;//the HUD/Menu orthographic camera
	
	//script references
	private MenuScriptCS hMenuScriptCS;
	private InGameScriptCS hInGameScriptCS;
	private SoundManagerCS hSoundManagerCS;
	
	private int iAndroidBackTapState = 0;//state of tap on back button on android devices
	private int iTapState=0;//state of tap on screen
	private Transform tTappedButtonTransform;
	private int CurrentMenu = -1;//currently active shop sub-menu
	private Transform[] tShopMenuTransforms;//transforms of the shop sub-menus
	
	//shop main screen
	private Transform[] tShopHomeButtons;
	private int iShopHomeButtonCount = 5;
	//shop costumes
	private Transform[] tShopCostumesButtons;//transforms of buttons in the costume sub-menu
	private int iShopCostumesButtonCount = 1;//total number of buttons in the costume sub-menu
	//shop powerups
	private Transform[] tShopPowerupsButtons;
	private int iShopPowerupsButtonsCount = 1;
	//shop utilities
	private Transform[] tShopUtilitiesButtons;
	private int iShopUtilitiesButtonCount = 1;
	//shop IAPs
	private Transform[] tShopIAPButtons;
	private int iShopIAPButtonsCount = 1;
	
	//elements list scroll
	private float scrollSensitivity = 0.2f;//how fast to scroll with swipe gesture
	private int iScrollState = 0;
	private float previousTapLocation;
	private float currentTapLocation;
	private Transform currentTab;//the list to scroll
	
	private float scrollUpperLimit;
	private float scrollLowerLimit;
	
	void Start () 
	{
		HUDCamera = (Camera)GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(typeof(Camera));
		hMenuScriptCS = (MenuScriptCS)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
		
		tShopMenuTransforms = new Transform[ShopMenus.GetValues(typeof(ShopMenus)).Length];
		tShopMenuTransforms[(int)ShopMenus.ShopHome] = (Transform)this.transform.Find("ShopHome").GetComponent(typeof(Transform));
		
		//shop primary menu
		tShopHomeButtons = new Transform[iShopHomeButtonCount];
		tShopHomeButtons[0] = (Transform)tShopMenuTransforms[(int)ShopMenus.ShopHome].Find("Buttons/Button_Back").GetComponent(typeof(Transform));
		tShopHomeButtons[1] = (Transform)tShopMenuTransforms[(int)ShopMenus.ShopHome].Find("Buttons/Button_Costumes").GetComponent(typeof(Transform));
		tShopHomeButtons[2] = (Transform)tShopMenuTransforms[(int)ShopMenus.ShopHome].Find("Buttons/Button_Powerups").GetComponent(typeof(Transform));
		tShopHomeButtons[3] = (Transform)tShopMenuTransforms[(int)ShopMenus.ShopHome].Find("Buttons/Button_Utilities").GetComponent(typeof(Transform));
		tShopHomeButtons[4] = (Transform)tShopMenuTransforms[(int)ShopMenus.ShopHome].Find("Buttons/Button_MoreCoins").GetComponent(typeof(Transform));
		
		//shop costumes menu
		tShopMenuTransforms[(int)ShopMenus.Costumes] = (Transform)this.transform.Find("CostumesShop").GetComponent(typeof(Transform));
		tShopCostumesButtons = new Transform[iShopCostumesButtonCount];
		tShopCostumesButtons[0] = (Transform)tShopMenuTransforms[(int)ShopMenus.Costumes].Find("Button_Back").GetComponent(typeof(Transform));
		
		//shop powerups menu
		tShopMenuTransforms[(int)ShopMenus.Powerups] = (Transform)this.transform.Find("PowerupsShop").GetComponent(typeof(Transform));
		tShopPowerupsButtons = new Transform[iShopPowerupsButtonsCount];
		tShopPowerupsButtons[0] = (Transform)tShopMenuTransforms[(int)ShopMenus.Powerups].Find("Button_Back").GetComponent(typeof(Transform));
		
		//shop utilities menu
		tShopMenuTransforms[(int)ShopMenus.Utilities] = (Transform)this.transform.Find("UtilitiesShop").GetComponent(typeof(Transform));
		tShopUtilitiesButtons = new Transform[iShopUtilitiesButtonCount];
		tShopUtilitiesButtons[0] = (Transform)tShopMenuTransforms[(int)ShopMenus.Utilities].Find("Button_Back").GetComponent(typeof(Transform));
		
		//shop IAP
		tShopMenuTransforms[(int)ShopMenus.IAPs] = (Transform)this.transform.Find("MoreCoinsShop").GetComponent(typeof(Transform));
		tShopIAPButtons = new Transform[iShopIAPButtonsCount];
		tShopIAPButtons[0] = (Transform)tShopMenuTransforms[(int)ShopMenus.IAPs].Find("Button_Back").GetComponent(typeof(Transform));
		
		//vertical scroll initilization
		iScrollState = 0;
		
		setShopScriptEnabled(false);
	}
	
	void OnGUI ()
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
	private void listenerClicks()
	{	
		if (Input.GetMouseButtonDown(0) && iTapState == 0)//detect taps
		{	
			iTapState = 1;			
		}//end of if get mouse button
		else if (iTapState == 1)//call relevent handler
		{
			if (Input.GetMouseButtonUp(0))			
				iTapState = 2;
		}
		else if (iTapState == 2)//wait for user to release before detcting next tap
		{
			if (Physics.Raycast(HUDCamera.ScreenPointToRay(Input.mousePosition), out hit))//if a button has been tapped		
			{
				tTappedButtonTransform = hit.transform;
				//call the listner function of the active menu
				if (CurrentMenu == (int)ShopMenus.ShopHome)
					handlerShopMenu(tTappedButtonTransform);
				else if (CurrentMenu == (int)ShopMenus.Costumes)
					handlerCostumesShop(tTappedButtonTransform);
				else if (CurrentMenu == (int)ShopMenus.Powerups)
					handlerPowerupShop(tTappedButtonTransform);
				else if (CurrentMenu == (int)ShopMenus.Utilities)
					handlerUtilitiesShop(tTappedButtonTransform);
				else if (CurrentMenu == (int)ShopMenus.IAPs)
					handlerIAPShop(tTappedButtonTransform);
			}//end of if raycast
			
			iTapState = 0;
		}
	}//end of listener clicks function
	
	/*
	 * FUNCTION:	Handler back button execution on android devices.
	 * 
	 * CALLED BY:	OnGUI()
	 * */
	private void handlerAndroidBackButton()
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
			if (CurrentMenu == (int)ShopMenus.Costumes)
				handlerCostumesShop(tShopCostumesButtons[0]);
			else if (CurrentMenu == (int)ShopMenus.IAPs)
				handlerIAPShop(tShopIAPButtons[0]);
			else if (CurrentMenu == (int)ShopMenus.Powerups)
				handlerPowerupShop(tShopPowerupsButtons[0]);
			else if (CurrentMenu == (int)ShopMenus.ShopHome)
				handlerShopMenu(tShopHomeButtons[0]);
			else if (CurrentMenu == (int)ShopMenus.Utilities)
				handlerUtilitiesShop(tShopUtilitiesButtons[0]);
			
			iAndroidBackTapState = 0;
		}
	}//end of handler android back button function
	
	/*
	*	FUNCTION:	Perform function according to the clicked button.
	*	CALLED BY:	listenerClicks()
	*/
	private void handlerShopMenu(Transform buttonTransform)
	{
		if (buttonTransform == tShopHomeButtons[0])//back button
		{			
			hMenuScriptCS.setMenuScriptStatus(true);
			setShopScriptEnabled(false);
			CloseMenu((int)ShopMenus.ShopHome);
			hMenuScriptCS.ShowMenu((int)MenuScriptCS.MenuIDs.MainMenu);
		}
		else if (buttonTransform == tShopHomeButtons[1])//costumes button
		{
			CloseMenu((int)ShopMenus.ShopHome);
			ShowMenu((int)ShopMenus.Costumes);
		}
		else if (buttonTransform == tShopHomeButtons[2])//powerups button
		{
			CloseMenu((int)ShopMenus.ShopHome);
			ShowMenu((int)ShopMenus.Powerups);
		}
		else if (buttonTransform == tShopHomeButtons[3])//utilities button
		{
			CloseMenu((int)ShopMenus.ShopHome);
			ShowMenu((int)ShopMenus.Utilities);
		}
		else if (buttonTransform == tShopHomeButtons[4])//IAP button
		{
			CloseMenu((int)ShopMenus.ShopHome);
			ShowMenu((int)ShopMenus.IAPs);
		}
	}//end of handler shop menu function
	
	/*
	*	FUNCTION:	Perform functions according to the button clicked.
	*	CALLED BY:	listenerClicks()
	*/
	private void handlerCostumesShop(Transform buttonTransform)
	{
		if (buttonTransform == tShopCostumesButtons[0])
		{
			CloseMenu((int)ShopMenus.Costumes);
			ShowMenu((int)ShopMenus.ShopHome);
		}
	}
	
	/*
	*	FUNCTION:	Perform functions according to the button clicked.
	*	CALLED BY:	listenerClicks()
	*/
	private void handlerPowerupShop(Transform buttonTransform)
	{
		if (buttonTransform == tShopPowerupsButtons[0])
		{
			CloseMenu((int)ShopMenus.Powerups);
			ShowMenu((int)ShopMenus.ShopHome);
		}
	}
	
	/*
	*	FUNCTION:	Perform functions according to the button clicked.
	*	CALLED BY:	listenerClicks()
	*/
	private void handlerUtilitiesShop(Transform buttonTransform)
	{
		if (buttonTransform == tShopUtilitiesButtons[0])
		{
			CloseMenu((int)ShopMenus.Utilities);
			ShowMenu((int)ShopMenus.ShopHome);
		}
	}
	
	/*
	*	FUNCTION:	Perform functions according to the button clicked.
	*	CALLED BY:	listenerClicks()
	*/
	private void handlerIAPShop(Transform buttonTransform)
	{
		if (buttonTransform == tShopIAPButtons[0])
		{
			CloseMenu((int)ShopMenus.IAPs);
			ShowMenu((int)ShopMenus.ShopHome);
		}
	}
	
	/*
	*	FUNCTION:	Get the currency the user has and display on the 
	*				Shop's header.
	*/
	public void updateCurrencyOnHeader()
	{
		if (CurrentMenu != -1)//check if a menu is currently active
			((TextMesh)tShopMenuTransforms[CurrentMenu].Find("CurrencyGroup/Text_Currency")
			.GetComponent(typeof(TextMesh))).text = hInGameScriptCS.getCurrencyCount().ToString();
		
	}
	
	/*
	*	FUNCTION:	Take the user to the main menu to display the costume.
	*				This funciton is called if user clicks equip button in the costumes menu.
	*/
	public void displayEquippedCostume()
	{
		CloseMenu((int)ShopMenus.Costumes);
		hMenuScriptCS.setMenuScriptStatus(true);
		setShopScriptEnabled(false);
		CloseMenu((int)ShopMenus.ShopHome);
		hMenuScriptCS.ShowMenu((int)MenuScriptCS.MenuIDs.MainMenu);
	}
	
	/*
	*	FUNCTION: Set the menu to show in front of the HUD Camera
	*/
	public void ShowMenu(int index)
	{
		CurrentMenu = index;//set the active menu to the currently displayed
		hSoundManagerCS.playSound(SoundManagerCS.MenuSounds.ButtonTap);//play tap sound
		updateCurrencyOnHeader();//update currency on header
		
		if (index == (int)ShopMenus.Costumes)
		{
			int itemCount=0;
			
			//set the items's parent as the currentTab to make the vertical scroll code work
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.Costumes].Find("CostumeItemGroup").GetComponent(typeof(Transform));
					
			//enable the ShopCostumeScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("CostumeItem"))
				{
					((ShopCostumeScriptCS)costumeItem.GetComponent(typeof(ShopCostumeScriptCS))).setShopCostumeScriptEnabled(true);
					itemCount++;//count the number of costumes in the costume shop menu
				}
			
			currentTab.localPosition = new Vector3(currentTab.localPosition.x, 0, currentTab.localPosition.z);
			//set the upper and lower limit of the scroll
			scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
			//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		}
		else if (index == (int)ShopMenus.Powerups)
		{
			int itemCount=0;
			//set the items's parent as the currentTab to make the vertical scroll code work
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.Powerups].Find("PowerupsItemGroup").GetComponent(typeof(Transform));
					
			//enable the ShopPowerupScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("PowerupItem"))
				{
					((ShopPowerupScriptCS)costumeItem.GetComponent(typeof(ShopPowerupScriptCS))).setShopPowerupScriptEnabled(true);
					itemCount++;//count the number of power-ups in power-up shop menu
				}
			
			currentTab.localPosition = new Vector3(currentTab.localPosition.x, 0, currentTab.localPosition.z);
			//set the upper and lower limit of the scroll
			scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
			//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		}
		else if (index == (int)ShopMenus.Utilities)
		{
			int itemCount=0;
			//set the items's parent as the currentTab to make the vertical scroll code work
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.Utilities].Find("UtilitiesItemGroup").GetComponent(typeof(Transform));
					
			//enable the ShopUtilityScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("UtilityItem"))
				{
					((ShopUtilityScriptCS)costumeItem.GetComponent(typeof(ShopUtilityScriptCS))).setShopUtilityScriptEnabled(true);
					itemCount++;//count the number of utilities in utility shop menu
				}
			
			currentTab.localPosition = new Vector3(currentTab.localPosition.x, 0, currentTab.localPosition.z);
			//set the upper and lower limit of the scroll
			scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
			//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		}
		else if (index == (int)ShopMenus.IAPs)
		{
			int itemCount=0;
			//set the items's parent as the currentTab to make the vertical scroll code work
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.IAPs].Find("IAPItemGroup").GetComponent(typeof(Transform));
					
			//enable the ShopIAPScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("IAPItem"))
				{
					((ShopIAPScriptCS)costumeItem.GetComponent(typeof(ShopIAPScriptCS))).setShopIAPScriptEnabled(true);//enable script
					itemCount++;//count the number of items in more coins shop menu
				}
			
			currentTab.localPosition = new Vector3(currentTab.localPosition.x, 0, currentTab.localPosition.z);
			//set the upper and lower limit of the scroll
			scrollUpperLimit = currentTab.localPosition.y+(itemCount*15);
			//scrollLowerLimit = currentTab.localPosition.y-(itemCount*10);
		}//end of else if IAPs
		
		//move the more menu in front of the HUD camera
		tShopMenuTransforms[index].position = new Vector3(tShopMenuTransforms[index].position.x, 0, tShopMenuTransforms[index].position.z);
	}//end of Show Menu function
	
	/*
	*	FUNCTION: Send the menu away from the HUD Camera
	*/
	private void CloseMenu(int index)
	{		
		if (index == (int)ShopMenus.Costumes)
		{
			//disable the ShopCostumeScript of all the costume elements (to improve performance)
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.Costumes].Find("CostumeItemGroup").GetComponent(typeof(Transform));
			
			//enable the ShopCostumeScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("CostumeItem"))
					((ShopCostumeScriptCS)costumeItem.GetComponent(typeof(ShopCostumeScriptCS))).setShopCostumeScriptEnabled(false);
		}
		else if (index == (int)ShopMenus.Powerups)
		{
			//disable the ShopPowerupScript of all the costume elements (to improve performance)
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.Powerups].Find("PowerupsItemGroup").GetComponent(typeof(Transform));
			
			//enable the ShopPowerupScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("PowerupItem"))
					((ShopPowerupScriptCS)costumeItem.GetComponent(typeof(ShopPowerupScriptCS))).setShopPowerupScriptEnabled(false);
		}
		else if (index == (int)ShopMenus.Utilities)
		{
			//disable the ShopUtilityScript of all the costume elements (to improve performance)
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.Utilities].Find("UtilitiesItemGroup").GetComponent(typeof(Transform));
			
			//enable the ShopUtilityScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("UtilityItem"))
					((ShopUtilityScriptCS)costumeItem.GetComponent(typeof(ShopUtilityScriptCS))).setShopUtilityScriptEnabled(false);
		}
		else if (index == (int)ShopMenus.IAPs)
		{
			//disable the ShopIAPScript of all the in-app purchase elements (to improve performance)
			currentTab = (Transform)tShopMenuTransforms[(int)ShopMenus.IAPs].Find("IAPItemGroup").GetComponent(typeof(Transform));
			
			//enable the ShopIAPScript of all the costume elements
			foreach (Transform costumeItem in currentTab)
				if (costumeItem.name.Contains("IAPItem"))
					((ShopIAPScriptCS)costumeItem.GetComponent(typeof(ShopIAPScriptCS))).setShopIAPScriptEnabled(false);
		}
		
		tShopMenuTransforms[index].position = new Vector3(tShopMenuTransforms[index].position.x, 1000,
			tShopMenuTransforms[index].position.z);//hide the menu
		
		currentTab = null;//disable scroll function
		CurrentMenu = -1;
	}//end of Close Menu function
	
	/*
	*	FUNCTION:	Make the menu items scroll vertically.
	*	CALLED BY:	OnGUI()
	*/
	private void scrollVertical()
	{	
		if (iScrollState == 0 && Input.GetMouseButtonDown(0))//if tapped for scroll
		{
			previousTapLocation = Input.mousePosition.y;
			currentTapLocation = previousTapLocation;
			
			iScrollState = 1;
		}	
		else if (iScrollState == 1)//check swipe direction and movement speed
		{	
			if (Input.GetMouseButtonUp(0))
			{			
				iScrollState = 0;
			}
			
			previousTapLocation = currentTapLocation;
			currentTapLocation = Input.mousePosition.y;
					
			float deltaY = previousTapLocation-currentTapLocation;
			
			if (Mathf.Abs(deltaY) > 1)//if the finger has been lifted off the screen
			{			
				float movementFactor = Mathf.Abs(deltaY) * scrollSensitivity;
			
				if (deltaY < 0)//swipe up
					currentTab.localPosition = new Vector3(currentTab.localPosition.x, 
						Mathf.Clamp(currentTab.localPosition.y+movementFactor, scrollLowerLimit, scrollUpperLimit), currentTab.localPosition.z);					
				else if (deltaY > 0)//swipe down
					currentTab.localPosition = new Vector3(currentTab.localPosition.x,
						Mathf.Clamp(currentTab.localPosition.y-movementFactor, scrollLowerLimit, scrollUpperLimit), currentTab.localPosition.z);					
			}
		}//end of if
	}
	
	/*
	*	FUNCITON:	Activate or deactivate the ShopScript
	*	CALLED BY:	handlerShopMenu()
	*				MenuScript.handlerMainMenu(...)
	*/
	public void setShopScriptEnabled(bool status)
	{
		if (status == true)
		{
			this.enabled = status;			
		}
		else if (status == false)
		{
			this.enabled = status;			
		}
	}//end of set shop script enabled function
}