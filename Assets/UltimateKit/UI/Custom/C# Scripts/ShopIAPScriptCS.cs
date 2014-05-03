/*
*	FUNCTION:
*	- Enables user to purchase a in-app purchase.
*/
using UnityEngine;
using System.Collections;

public class ShopIAPScriptCS : MonoBehaviour {

	public float itemCost;//price of the in-app purchase
	public int itemReward;//amount of currency units user will get in return
	
	private int iTapState = 0;//state of tap on screen
	private RaycastHit hit;//used for detecting taps
	private Camera HUDCamera;//the HUD/Menu orthographic camera
	
	private Transform tBuyButton;
	private TextMesh tmItemCost;//cost of the IAP in real currency
	private TextMesh tmItemReward;//virtual currency that will be awarded on purchase
	
	private ShopScriptCS hShopScriptCS;
	private InGameScriptCS hInGameScriptCS;
	
	void Start ()
	{
		HUDCamera = (Camera)GameObject.Find("HUDMainGroup/HUDCamera").GetComponent(typeof(Camera));
		hShopScriptCS = (ShopScriptCS)GameObject.Find("MenuGroup/Shop").GetComponent(typeof(ShopScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		
		if (itemCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the IAP shop element. Check the user documentation.");
		else if (itemReward <= 0)
			Debug.Log("EXCEPTION: No reward assigned to the IAP shop element. Check the user documentation.");
		
		tBuyButton = (Transform)this.transform.Find("Buttons/Button_Buy").GetComponent(typeof(Transform));
		tmItemCost = (TextMesh)this.transform.Find("Item_Cost").GetComponent(typeof(TextMesh));
		tmItemCost.text = "$ " + itemCost.ToString();//set the cost as specified by user
		
		tmItemReward = (TextMesh)this.transform.Find("ItemGroup/Text_Reward").GetComponent(typeof(TextMesh));
		tmItemReward.text = itemReward.ToString();//set the virtual currency reward as specified by the user
			
		setShopIAPScriptEnabled(false);//turn off current script
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
	private void handlerIAPItem(Transform buttonTransform)
	{
		if (buttonTransform == tBuyButton)//if buy button pressed
		{
			//give user the bought amount of in-game currency units
			hInGameScriptCS.alterCurrencyCount(itemReward);//award the purcahsed units
			hShopScriptCS.updateCurrencyOnHeader();//update the currency on the header bar			
		}//end of if
	}
	
	/*
	*	FUNCITON:	Enable or disable the current script.
	*/
	public void setShopIAPScriptEnabled(bool state)
	{	
		this.enabled = state;
	}
}
