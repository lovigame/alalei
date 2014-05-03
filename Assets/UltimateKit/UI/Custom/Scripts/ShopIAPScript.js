#pragma strict
/*
*	FUNCTION:
*	- Enables user to purchase a in-app purchase.
*/

var itemCost:float;//price of the in-app purchase
var itemReward:int;//amount of currency units user will get in return

private var iTapState:int = 0;//state of tap on screen
private var hit : RaycastHit;//used for detecting taps
private var HUDCamera : Camera;//the HUD/Menu orthographic camera

private var tBuyButton : Transform;
private var tmCost : TextMesh;//cost of the IAP item in real currency displayed on the prefab
private var tmReward : TextMesh;//virtual currency reward given; displayed on the prefab

//script references
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
		Debug.Log("EXCEPTION: No cost assigned to the IAP shop element. Check the user documentation.");
	else if (itemReward <= 0)
		Debug.Log("EXCEPTION: No reward assigned to the IAP shop element. Check the user documentation.");
	
	tBuyButton = this.transform.Find("Buttons/Button_Buy").GetComponent(Transform) as Transform;
	tmCost = this.transform.Find("Item_Cost").GetComponent("TextMesh") as TextMesh;
	tmCost.text = "$ "+itemCost.ToString();//set the cost as specified by user
	
	tmReward = this.transform.Find("ItemGroup/Text_Reward").GetComponent("TextMesh") as TextMesh;
	tmReward.text = itemReward.ToString();//set the virtual currency reward as specified by the user
		
	setShopIAPScriptEnabled(false);//turn off current script
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
			handlerIAPItem(hit.transform);//call the listner function			
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
private function handlerIAPItem(buttonTransform:Transform)
{
	if (buttonTransform == tBuyButton)//if buy button pressed
	{
		//give user the bought amount of in-game currency units
		hInGameScript.alterCurrencyCount(itemReward);//award the purcahsed units
		hShopScript.updateCurrencyOnHeader();//update the currency on the header bar			
	}//end of if
}

/*
*	FUNCITON:	Enable or disable the current script.
*/
public function setShopIAPScriptEnabled(state:boolean)
{	
	this.enabled = state;
}