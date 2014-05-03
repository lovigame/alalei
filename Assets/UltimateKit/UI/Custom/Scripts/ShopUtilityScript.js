#pragma strict
/*
*	FUNCTION:
*	- Enables user to purchase a utility item.
*/

var itemCost:int;//cost of the utility

private var iTapState:int = 0;//state of tap on screen
private var hit : RaycastHit;//used for detecting taps
private var HUDCamera : Camera;//the HUD/Menu orthographic camera

private var tBuyButton : Transform;
private var tmCost : TextMesh;//cost of the utility displayed in shop

private var hShopScript : ShopScript;
private var hInGameScript : InGameScript;
private var hPowerupsMainController : PowerupsMainController;

function Start ()
{
	HUDCamera = GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(Camera) as Camera;
	hShopScript = GameObject.Find("MenuGroup/Shop").GetComponent(ShopScript) as ShopScript;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	hPowerupsMainController = GameObject.Find("Player").GetComponent(PowerupsMainController) as PowerupsMainController;
	
	if (itemCost <= 0)
		Debug.Log("EXCEPTION: No cost assigned to the Utility shop element. Check the user documentation.");
	
	tBuyButton = this.transform.Find("Buttons/Button_Buy").GetComponent(Transform) as Transform;
	tmCost = this.transform.Find("CostGroup/Text_Currency").GetComponent("TextMesh") as TextMesh;
	tmCost.text = itemCost.ToString();//set the cost of the item as specified by the user
		
	setShopUtilityScriptEnabled(false);//turn off current script
}

function OnGUI () 
{
	listenerClicks();//listen for clicks on utility shop menu
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
			handlerUtilityItem(hit.transform);//call the listner function
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
private function handlerUtilityItem(buttonTransform:Transform)
{
	if (buttonTransform == tBuyButton)
	{
		//give the utility to user and deduct the item cost
		if (hInGameScript.getCurrencyCount() >= itemCost)//check if user has enough currency
		{					
			hInGameScript.alterCurrencyCount(-itemCost);//deduct the cost of utility
			hShopScript.updateCurrencyOnHeader();//update the currency on the header bar			
		}
	}//end of if
}

/*
*	FUNCITON:	Enable or disable the current script.
*/
public function setShopUtilityScriptEnabled(state:boolean)
{	
	this.enabled = state;
}