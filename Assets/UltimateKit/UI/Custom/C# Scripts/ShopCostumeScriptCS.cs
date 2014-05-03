/*
*	FUNCTION:
*	- Enables user to buy or equip a skin.
*/

using UnityEngine;
using System.Collections;

public class ShopCostumeScriptCS : MonoBehaviour {

	//the characters material used by costumes menu
	//to change the skin
	public Material characterMaterial;
	public Texture characterCostume;//the character costumes
	public int costumeCost;
	
	private int iTapState = 0;//state of tap on screen
	private RaycastHit hit;//used for detecting taps
	private Camera HUDCamera;//the HUD/Menu orthographic camera
	
	//state of the costume (false = not owned; true = owned)
	private bool costumeOwned;
	
	private Transform tBuyButton;
	private Transform tEquipButton;
	private Material mThumb;
	private TextMesh tmCost;
	
	private ShopScriptCS hShopScriptCS;
	private InGameScriptCS hInGameScriptCS;
	
	void Start ()
	{
		HUDCamera = (Camera)GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(typeof(Camera));
		hShopScriptCS = (ShopScriptCS)GameObject.Find("MenuGroup/Shop").GetComponent(typeof(ShopScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		
		tBuyButton = (Transform)this.transform.Find("Buttons/Button_Buy").GetComponent(typeof(Transform));
		tEquipButton = (Transform)this.transform.Find("Buttons/Button_Equip").GetComponent(typeof(Transform));
		tmCost = (TextMesh)this.transform.Find("CostGroup/Text_Currency").GetComponent(typeof(TextMesh));
		tmCost.text = costumeCost.ToString();//display the costume cost in shop as specified by the user
			
		if (characterMaterial == null)
			Debug.Log("EXCEPTION: Character material not assigned to costume shop element. Check the user documentation.");
		else if (characterCostume == null)
			Debug.Log("EXCEPTION: Character texture not assigned to costume shop element. Check the user documentation.");
		else if (costumeCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the costume shop element. Check the user documentation.");
		
		if (characterMaterial.GetTexture("_MainTex") == characterCostume)//is the currently applied texture?
		{
			costumeOwned = true;
			tBuyButton.gameObject.SetActive(false);
			tEquipButton.gameObject.SetActive(true);
		}
		else
		{
			costumeOwned = false;
			tBuyButton.gameObject.SetActive(true);
			tEquipButton.gameObject.SetActive(false);
		}
		
		setShopCostumeScriptEnabled(false);//turn off current script
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
	private void handlerCostumeItem(Transform buttonTransform)
	{
		if (buttonTransform == tBuyButton)//buy button tapped
		{
			if (hInGameScriptCS.getCurrencyCount() >= costumeCost)//check if user has enough currency
			{
				//deduct the cost of costume
				hInGameScriptCS.alterCurrencyCount(-costumeCost);
				
				//change the texture of the character
				characterMaterial.SetTexture("_MainTex", characterCostume);
				
				//turn off buy and show the equip button
				tBuyButton.gameObject.SetActive(false);
				tEquipButton.gameObject.SetActive(true);
				
				//change the costumeOwned
				costumeOwned = true;
				
				//take the user to the main menu
				hShopScriptCS.displayEquippedCostume();
			}//end of if cost == cash	
		}
		else if (buttonTransform == tEquipButton)//equip button tapped
		{
			//change the texture of the character
			characterMaterial.SetTexture("_MainTex", characterCostume);
			
			//take the user to the main menu
			hShopScriptCS.displayEquippedCostume();
		}	
	}
	
	/*
	*	FUNCITON:	Enable or disable the current script.
	*/
	public void setShopCostumeScriptEnabled(bool state)
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
}
