using UnityEngine;
using System.Collections;

public class SoundButtonHandler : MonoBehaviour {
	
	enum State { On, Off }
	
	private SoundManagerCS hSoundManagerCS;
	private UICheckbox uicSound;
	private State state;//if the music is ON of OFF
	
	void Start () 
	{
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
		uicSound = (UICheckbox)this.GetComponent(typeof(UICheckbox));
		
		//check if the script is a part of On or Off radio button		
		if (this.name == "On")
			state = State.On;
		else
			state = State.Off;
		
		//set the Music radio button to On or Off
		if (state == State.On)
		{
			if (hSoundManagerCS.isSoundEnabled())
				uicSound.isChecked = true;
			else
				uicSound.isChecked = false;
		}
		else if (state == State.Off)
		{
			if (!hSoundManagerCS.isSoundEnabled())
				uicSound.isChecked = true;
			else
				uicSound.isChecked = false;
		}
	}//end of Start()
	
	void OnActivate(bool isTrue)
	{
		if (isTrue && state == State.On)
			hSoundManagerCS.toggleSoundEnabled(true);
		else if (isTrue && state == State.Off)
			hSoundManagerCS.toggleSoundEnabled(false);
	}
		
}