using UnityEngine;
using System.Collections;

public class ControlsButtonHandler : MonoBehaviour {
	
	private ControllerScriptCS hControllerScriptCS;
	private UICheckbox uicControls;
	private ControllerScriptCS.ControlType controlType;
	
	void Start ()
	{
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));
		uicControls = (UICheckbox)this.GetComponent(typeof(UICheckbox));
		
		//check which radio button is this
		if (this.name == "Swipe")
			controlType = ControllerScriptCS.ControlType.Swipe;
		else
			controlType = ControllerScriptCS.ControlType.Gyro;
		
		//check or uncheck according to type of controls currently enabled
		if (controlType == ControllerScriptCS.ControlType.Swipe)
		{
			if (hControllerScriptCS.isSwipeControlEnabled())
				uicControls.isChecked = true;
			else
				uicControls.isChecked = false;
		}
		else if (controlType == ControllerScriptCS.ControlType.Gyro)
		{
			if (!hControllerScriptCS.isSwipeControlEnabled())
				uicControls.isChecked = true;
			else
				uicControls.isChecked = false;
		}		
	}//end of start
	
	void OnActivate(bool state)
	{
		if (state && controlType == ControllerScriptCS.ControlType.Swipe)
			hControllerScriptCS.toggleSwipeControls(true);
		else if (state && controlType == ControllerScriptCS.ControlType.Gyro)
			hControllerScriptCS.toggleSwipeControls(false);		
	}//end of On Activate
}
