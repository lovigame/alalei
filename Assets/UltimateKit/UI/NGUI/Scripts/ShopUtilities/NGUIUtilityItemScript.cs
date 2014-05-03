using UnityEngine;
using System.Collections;

public class NGUIUtilityItemScript : MonoBehaviour {

	public int itemCost;
	
	private InGameScriptCS hInGameScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
	
	private UILabel uilCost;
	
	void Start () 
	{
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
		
		if (itemCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the Utility shop element. Check the user documentation.");
		
		uilCost = (UILabel)this.transform.Find("Text_Cost").GetComponent(typeof(UILabel));
		uilCost.text = itemCost.ToString();//cost of the utility displayed in shop
	}
	
	void OnClick () 
	{
		//give the utility to user and deduct the item cost
		if (hInGameScriptCS.getCurrencyCount() >= itemCost)//check if user has enough currency
		{					
			hInGameScriptCS.alterCurrencyCount(-itemCost);//deduct the cost of utility
			//update the currency on the header bar
			hNGUIMenuScript.updateCurrencyOnHeader(hNGUIMenuScript.getCurrentMenu());
		}
	}//end of On Click function
}
