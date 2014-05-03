using UnityEngine;
using System.Collections;

public class NGUIPowerupItemScript : MonoBehaviour {

	public int powerupUpgradeLevelMAX;
	public int upgradeCost;
	public PowerupsMainControllerCS.PowerUps powerup;
	
	private InGameScriptCS hInGameScriptCS;
	private NGUIMenuScript hNGUIMenuScript;
	private PowerupsMainControllerCS hPowerupsMainControllerCS;
	
	private int currentPowerupLevel;
	private UILabel uilLevelText;
	private UILabel uilUpgradeCost;
	
	void Start ()
	{
		uilLevelText = (UILabel)this.transform.Find("Text_ItemLevel").GetComponent(typeof(UILabel));
		
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
		hNGUIMenuScript = (NGUIMenuScript)GameObject.Find("UI Root (2D)").GetComponent(typeof(NGUIMenuScript));
		hPowerupsMainControllerCS = (PowerupsMainControllerCS)GameObject.Find("Player").GetComponent(typeof(PowerupsMainControllerCS));
		
		if (upgradeCost <= 0)
			Debug.Log("EXCEPTION: No cost assigned to the Power-up shop element. Check the user documentation.");
		else if (powerupUpgradeLevelMAX <= 0)
			Debug.Log("EXCEPTION: Power-up upgrade level cannot be zero. Check the user documentation.");
		
		uilUpgradeCost = (UILabel)this.transform.Find("Text_Cost").GetComponent(typeof(UILabel));
		uilUpgradeCost.text = upgradeCost.ToString();//set the cost of the item as specified by the user
		
		currentPowerupLevel = 1;
	}
		
	void OnClick () 
	{
		//increase the powerup level
		if (currentPowerupLevel < powerupUpgradeLevelMAX //check if the max level has not been achieved
		&& hInGameScriptCS.getCurrencyCount() >= upgradeCost)//check if user has enough currency
		{
			currentPowerupLevel++;//increase the power-up level
					
			hInGameScriptCS.alterCurrencyCount(-upgradeCost);//deduct the cost of power-up upgrade
			hNGUIMenuScript.updateCurrencyOnHeader(hNGUIMenuScript.getCurrentMenu());//update the currency on the header bar
			
			//tell the power-up script to increase the duration of the power-up
			hPowerupsMainControllerCS.upgradePowerup(powerup);
			
			//Update the text on the power-up item in shop
			uilLevelText.text = "Level "+currentPowerupLevel;
		}		
	}//end of On Click
}
