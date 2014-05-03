using UnityEngine;
using System.Collections;

public class NGUICostumeItemScript : MonoBehaviour {
	
	//the characters material used by costumes menu
	//to change the skin
	public Material characterMaterial;
	public Texture characterCostume;//the character costumes
	public int costumeCost;
		
	private GameObject goBuyEquipButton;
	private UILabel uilBuyEquipButton;
	private UILabel uilCostDescription;
		
	//state of the costume (false = not owned; true = owned)
	private bool costumeOwned;
	
	//script references
	private InGameScriptCS hInGameScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
		
	void Start () 
	{
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
				
		goBuyEquipButton = (GameObject)this.transform.Find("Button_BuyEquip").gameObject;
		uilBuyEquipButton = (UILabel)this.transform.Find("Button_BuyEquip/Label").GetComponent(typeof(UILabel));
		uilCostDescription = (UILabel)this.transform.Find("Text_Cost").GetComponent(typeof(UILabel));
						
		//check if a meterial, texture and cost has been assigned to exposed variables
		if (characterMaterial == null)
			Debug.Log("EXCEPTION: Character material not assigned to costume shop element. Check the user documentation.");
		else if (characterCostume == null)
			Debug.Log("EXCEPTION: Character texture not assigned to costume shop element. Check the user documentation.");
		else if (costumeCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the costume shop element. Check the user documentation.");
		
		//is this the currently applied texture?
		if (characterMaterial.GetTexture("_MainTex") == characterCostume)
		{
			costumeOwned = true;
			uilBuyEquipButton.text = "EQUIP";
		}
		else//not the currently equiped texture
		{
			costumeOwned = false;
			uilBuyEquipButton.text = "BUY";
		}
		
		//set the price on the label
		uilCostDescription.text = costumeCost.ToString();
	}//end of Start
		
	void OnClick()
	{
		if (costumeOwned == false)//buy button tapped
		{
			if (hInGameScriptCS.getCurrencyCount() >= costumeCost)//check if user has enough currency
			{
				//deduct the cost of costume
				hInGameScriptCS.alterCurrencyCount(-costumeCost);
				
				//change the texture of the character
				characterMaterial.SetTexture("_MainTex", characterCostume);
				
				//turn off buy and show equip button
				uilBuyEquipButton.text = "EQUIP";
				
				//change the costumeOwned
				costumeOwned = true;
				
				//take the user to the main menu
				hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.MainMenu);
				hNGUIMenuScript.CloseMenu(NGUIMenuScript.NGUIMenus.ShopCostumes);
			}//end of if cost == cash	
		}
		else if (costumeOwned == true)//equip button tapped
		{
			//change the texture of the character
			characterMaterial.SetTexture("_MainTex", characterCostume);
			
			//take the user to the main menu
			hNGUIMenuScript.ShowMenu(NGUIMenuScript.NGUIMenus.MainMenu);
			hNGUIMenuScript.CloseMenu(NGUIMenuScript.NGUIMenus.ShopCostumes);
		}
	}//end of On Click function
}
