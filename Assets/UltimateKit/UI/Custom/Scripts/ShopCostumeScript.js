#pragma strict
/*
*	FUNCTION:
*	- Enables user to buy or equip a skin.
*/

//the characters material used by costumes menu
//to change the skin
var characterMaterial : Material;	//the character material
var characterCostume : Texture;//the character costumes
var costumeCost : int;	//cost of the costume in virtual currency

private var iTapState:int = 0;//state of tap on screen
private var hit : RaycastHit;//used for detecting taps
private var HUDCamera : Camera;//the HUD/Menu orthographic camera

//state of the costume (false = not owned; true = owned)
private var costumeOwned : boolean;

private var tBuyButton : Transform;
private var tEquipButton : Transform;
private var tmCost : TextMesh;//cost of the costume displayed on the prefab

//script references
private var hShopScript : ShopScript;
private var hInGameScript : InGameScript;

function Start ()
{
	HUDCamera = GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(Camera) as Camera;
	hShopScript = GameObject.Find("MenuGroup/Shop").GetComponent(ShopScript) as ShopScript;
	hInGameScript = GameObject.Find("Player").GetComponent(InGameScript) as InGameScript;
	
	tBuyButton = this.transform.Find("Buttons/Button_Buy").GetComponent(Transform) as Transform;
	tEquipButton = this.transform.Find("Buttons/Button_Equip").GetComponent(Transform) as Transform;
	tmCost = this.transform.Find("CostGroup/Text_Currency").GetComponent("TextMesh") as TextMesh;
	tmCost.text = costumeCost.ToString();//set the user given cost on the prefab
		
	if (characterMaterial == null)
		Debug.Log("EXCEPTION: Character material not assigned to costume shop element. Check the user documentation.");
	else if (characterCostume == null)
		Debug.Log("EXCEPTION: Character texture not assigned to costume shop element. Check the user documentation.");
	else if (costumeCost <= 0)
		Debug.Log("EXCEPTION: No cost assigned to the costume shop element. Check the user documentation.");
	
	if (characterMaterial.GetTexture("_MainTex") == characterCostume)//is the currently applied texture?
	{
		costumeOwned = true;
		tBuyButton.gameObject.SetActive(false);//turn off buy button
		tEquipButton.gameObject.SetActive(true);//turn off equip button
	}
	else
	{
		costumeOwned = false;
		tBuyButton.gameObject.SetActive(true);
		tEquipButton.gameObject.SetActive(false);
	}
	
	setShopCostumeScriptEnabled(false);//turn off current script
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
			if (characterMaterial == null)
				Debug.Log("EXCEPTION: Character material not assigned to costume shop element. Check the user documentation.");
			else if (characterCostume == null)
				Debug.Log("EXCEPTION: Character texture not assigned to costume shop element. Check the user documentation.");
			else
				handlerCostumeItem(hit.transform);//call the listner function
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
private function handlerCostumeItem(buttonTransform:Transform)
{
	if (buttonTransform == tBuyButton)
	{
		if (hInGameScript.getCurrencyCount() >= costumeCost)//check if user has enough currency
		{
			//deduct the cost of costume
			hInGameScript.alterCurrencyCount(-costumeCost);
			
			//change the texture of the character
			characterMaterial.SetTexture("_MainTex", characterCostume);
			
			//turn off buy and equip button
			tBuyButton.gameObject.SetActive(false);
			tEquipButton.gameObject.SetActive(true);
			
			//change the costumeOwned
			costumeOwned = true;
			
			//take the user to the main menu
			hShopScript.displayEquippedCostume();
		}//end of if cost == cash	
	}
	else if (buttonTransform == tEquipButton)
	{
		//change the texture of the character
		characterMaterial.SetTexture("_MainTex", characterCostume);
		
		//take the user to the main menu
		hShopScript.displayEquippedCostume();
	}	
}

/*
*	FUNCITON:	Enable or disable the current script. The correct button
*				"Equip" or "Buy" is also activated for each costume item
*	
*	CALLED BY:	Start()
*/
public function setShopCostumeScriptEnabled(state:boolean)
{
	if (state == true)
	{		
		this.enabled = true;//enable the script		
		
		//enable or disable buy/ equip button
		if (costumeOwned == false)//if costume is not owned
		{
			tBuyButton.gameObject.SetActive(true);
			tEquipButton.gameObject.SetActive(false);
		}
		else if (costumeOwned == true)//if costume is owned
		{
			tBuyButton.gameObject.SetActive(false);
			tEquipButton.gameObject.SetActive(true);
		}		
	}
	else
		this.enabled = false;
	
}