using UnityEngine;
using System.Collections;

public class NGUIShopIAPItemScript : MonoBehaviour {

	public float itemCost;//price of the in-app purchase
	public int itemReward;//amount of currency units user will get in return
	
	private UILabel uilCost;//cost label of the IAP element
	private UILabel uilReward;//reward label of the IAP element
	
	private NGUIMenuScript hNGUIMenuScript;
	private InGameScriptCS hInGameScriptCS;
	
	void Start () 
	{
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		
		if (itemCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the IAP shop element. Check the user documentation.");
		else if (itemReward <= 0)
			Debug.Log("EXCEPTION: No reward assigned to the IAP shop element. Check the user documentation.");
		
		uilCost = (UILabel)this.transform.Find("Text_Cost").GetComponent(typeof(UILabel));		
		uilReward = (UILabel)this.transform.Find("Text_Reward").GetComponent(typeof(UILabel));
		
		uilCost.text = "$ " + itemCost.ToString();//display the cost of the item
		uilReward.text = itemReward.ToString();//display the virtual currency reward
	}
		
	void OnClick () 
	{
		//give user the bought amount of in-game currency units
		hInGameScriptCS.alterCurrencyCount(itemReward);//award the purcahsed units
		//update the currency on the header bar
		hNGUIMenuScript.updateCurrencyOnHeader(hNGUIMenuScript.getCurrentMenu());
	}
}
