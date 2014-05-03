#pragma strict
/*
*	FUNCTION:
*	- Enables user to upgrade a powerup.
*/
var powerupUpgradeLevelMAX:int;
var upgradeCost:int;
var powerup : PowerUps;

private var iTapState:int = 0;//state of tap on screen
private var hit : RaycastHit;//used for detecting taps
private var HUDCamera : Camera;//the HUD/Menu orthographic camera

private var currentPowerupLevel:int = 1;

private var tBuyButton : Transform;
private var tmCost : TextMesh;

private var hShopScript : ShopScript;
private var hInGameScript : InGameScript;
private var hPowerupsMainController : PowerupsMainController;

function Start ()
{
	HUDCamera = GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(Camera) as Camera;
	hShopScript = GameObject.Find("MenuGroup/Shop").GetComponent(ShopScript) as ShopScript;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hPowerupsMainController = GameObject.Find("Player").GetComponent(PowerupsMainController) as PowerupsMainController;
	
	if (upgradeCost <= 0)
		Debug.Log("EXCEPTION: No cost assigned to the Power-up shop element. Check the user documentation.");
	else if (powerupUpgradeLevelMAX <= 0)
		Debug.Log("EXCEPTION: Power-up upgrade level cannot be zero. Check the user documentation.");
	
	tBuyButton = this.transform.Find("Buttons/Button_Buy").GetComponent(Transform) as Transform;
	tmCost = this.transform.Find("CostGroup/Text_Currency").GetComponent("TextMesh") as TextMesh;
	tmCost.text = upgradeCost.ToString();//set the cost of the item as specified by the user
	
	//Update the text on the power-up item in shop
	(this.transform.Find("Text_ItemLevel").GetComponent("TextMesh") as TextMesh).text = "Level "+currentPowerupLevel;
	
	setShopPowerupScriptEnabled(false);//turn off current script
}

function OnGUI () 
{
	listenerClicks();//listen for clicks on costume menu
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
	else if (iTapState == 1)//call relevent handler
	{
		if (Physics.Raycast(HUDCamera.ScreenPointToRay(Input.mousePosition),hit))//if a button has been tapped
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
private function handlerPowerupItem(buttonTransform:Transform)
{
	if (buttonTransform == tBuyButton)
	{
		//increase the powerup level
		if (currentPowerupLevel < powerupUpgradeLevelMAX //check if the max level has not been achieved
		&& hInGameScript.getCurrencyCount() >= upgradeCost)//check if user has enough currency
		{
			currentPowerupLevel++;//increase the power-up level
					
			hInGameScript.alterCurrencyCount(-upgradeCost);//deduct the cost of power-up upgrade
			hShopScript.updateCurrencyOnHeader();//update the currency on the header bar
			
			//tell the power-up script to increase the duration of the power-up
			hPowerupsMainController.upgradePowerup(powerup);
			
			//Update the text on the power-up item in shop
			(this.transform.Find("Text_ItemLevel").GetComponent("TextMesh") as TextMesh).text = "Level "+currentPowerupLevel;
		}
	}//end of if
}

/*
*	FUNCITON:	Enable or disable the current script.
*/
public function setShopPowerupScriptEnabled(state:boolean)
{	
	this.enabled = state;	
}