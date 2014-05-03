/*
*	FUNCTION:
*	- Enables user to purchase a utility item.
*/

using UnityEngine;
using System.Collections;

public class ShopUtilityScriptCS : MonoBehaviour {

	public int itemCost;//exposed variable to store the item cost

	private int iTapState = 0;//state of tap on screen
	private RaycastHit hit;//used for detecting taps
	private Camera HUDCamera;//the HUD/Menu orthographic camera
	
	private Transform tBuyButton;
	private TextMesh tmCost;//cost of the utility displayed in shop
	
	private ShopScriptCS hShopScriptCS;
	private InGameScriptCS hInGameScriptCS;
	
	void Start ()
	{
		HUDCamera = (Camera)GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(typeof(Camera));
		hShopScriptCS = (ShopScriptCS)GameObject.Find("MenuGroup/Shop").GetComponent(typeof(ShopScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		
		if (itemCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the Utility shop element. Check the user documentation.");
		
		tBuyButton = (Transform)this.transform.Find("Buttons/Button_Buy").GetComponent(typeof(Transform));
		tmCost = (TextMesh)this.transform.Find("CostGroup/Text_Currency").GetComponent(typeof(TextMesh));
		tmCost.text = itemCost.ToString();//set the cost of the item as specified by the user
			
		setShopUtilityScriptEnabled(false);//turn off current script
	}
	
	void OnGUI ()
	{
		listenerClicks();//listen for clicks on utility shop menu
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
	private void handlerUtilityItem(Transform buttonTransform)
	{
		if (buttonTransform == tBuyButton)
		{
			//give the utility to user and deduct the item cost
			if (hInGameScriptCS.getCurrencyCount() >= itemCost)//check if user has enough currency
			{					
				hInGameScriptCS.alterCurrencyCount(-itemCost);//deduct the cost of utility
				hShopScriptCS.updateCurrencyOnHeader();//update the currency on the header bar			
			}
		}//end of if
	}
	
	/*
	*	FUNCITON:	Enable or disable the current script.
	*/
	public void setShopUtilityScriptEnabled(bool state)
	{	
		this.enabled = state;
	}
}
