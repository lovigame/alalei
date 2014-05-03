/*
*	FUNCTION:
*	- Enables user to upgrade a powerup.
*/
using UnityEngine;
using System.Collections;

public class ShopPowerupScriptCS : MonoBehaviour {

	public int powerupUpgradeLevelMAX;
	public int upgradeCost;
	public PowerupsMainControllerCS.PowerUps powerup;
	
	private int iTapState = 0;//state of tap on screen
	private RaycastHit hit;//used for detecting taps
	private Camera HUDCamera;//the HUD/Menu orthographic camera
	
	private int currentPowerupLevel = 1;
	
	private Transform tBuyButton;
	private TextMesh tmCost;
	
	private ShopScriptCS hShopScriptCS;
	private InGameScriptCS hInGameScriptCS;
	private PowerupsMainControllerCS hPowerupsMainControllerCS;
	
	void Start ()
	{
		HUDCamera = (Camera)GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(typeof(Camera));
		hShopScriptCS = (ShopScriptCS)GameObject.Find("MenuGroup/Shop").GetComponent(typeof(ShopScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hPowerupsMainControllerCS = (PowerupsMainControllerCS)GameObject.Find("Player").GetComponent(typeof(PowerupsMainControllerCS));
		
		if (upgradeCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the Power-up shop element. Check the user documentation.");
		else if (powerupUpgradeLevelMAX <= 0)
			Debug.Log("EXCEPTION: Power-up upgrade level cannot be zero. Check the user documentation.");
		
		tBuyButton = (Transform)this.transform.Find("Buttons/Button_Buy").GetComponent(typeof(Transform));
		tmCost = (TextMesh)this.transform.Find("CostGroup/Text_Currency").GetComponent(typeof(TextMesh));
		tmCost.text = upgradeCost.ToString();//set the cost of the item as specified by the user
		
		//Update the text on the power-up item in shop
		(this.transform.Find("Text_ItemLevel").GetComponent("TextMesh") as TextMesh).text = "Level "+currentPowerupLevel;
		
		setShopPowerupScriptEnabled(false);//turn off current script
	}
	
	void OnGUI () 
	{
		listenerClicks();//listen for clicks on costume menu
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
			if (Physics.Raycast(HUDCamera.ScreenPointToRay(Input.mousePosition), out hit))//if a button has been tapped
			{			
				handlerPowerupItem(hit.transform);//call the listner function
			}//end of if raycast
			
			iTapState = 2;
		}
		else if (iTapState == 2)//wait for user to release before detcting next tap
		{
			if (Input.GetMouseButtonUp(0))
				iTapState = 0;
		}
	}//end of listener clicks function
	
	/*
	*	FUNCTION:	Perform function according to the clicked button.
	*	CALLED BY:	listenerClicks()
	*/
	private void handlerPowerupItem(Transform buttonTransform)
	{
		if (buttonTransform == tBuyButton)
		{
			//increase the powerup level
			if (currentPowerupLevel < powerupUpgradeLevelMAX //check if the max level has not been achieved
			&& hInGameScriptCS.getCurrencyCount() >= upgradeCost)//check if user has enough currency
			{
				currentPowerupLevel++;//increase the power-up level
						
				hInGameScriptCS.alterCurrencyCount(-upgradeCost);//deduct the cost of power-up upgrade
				hShopScriptCS.updateCurrencyOnHeader();//update the currency on the header bar
				
				//tell the power-up script to increase the duration of the power-up
				hPowerupsMainControllerCS.upgradePowerup(powerup);
				
				//Update the text on the power-up item in shop
				(this.transform.Find("Text_ItemLevel").GetComponent("TextMesh") as TextMesh).text = "Level "+currentPowerupLevel;
			}
		}//end of if
	}
	
	/*
	*	FUNCITON:	Enable or disable the current script.
	*/
	public void setShopPowerupScriptEnabled(bool state)
	{	
		this.enabled = state;	
	}
}
