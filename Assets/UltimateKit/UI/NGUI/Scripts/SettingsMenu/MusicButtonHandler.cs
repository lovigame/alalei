using UnityEngine;
using System.Collections;

public class MusicButtonHandler : MonoBehaviour {
	
	enum State { On, Off }
	
	private SoundManagerCS hSoundManagerCS;
	private UICheckbox uicMusic;
	private State state;//if the music is ON of OFF
	
	void Start () 
	{
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
		uicMusic = (UICheckbox)this.GetComponent(typeof(UICheckbox));
		
		//check if the script is a part of On or Off radio button		
		if (this.name == "On")
			state = State.On;
		else
			state = State.Off;
		
		//set the Music radio button to On or Off
		if (state == State.On)
		{
			if (hSoundManagerCS.isMusicEnabled())
				uicMusic.isChecked = true;
			else
				uicMusic.isChecked = false;
		}
		else if (state == State.Off)
		{
			if (!hSoundManagerCS.isMusicEnabled())
				uicMusic.isChecked = true;
			else
				uicMusic.isChecked = false;
		}
	}//end of Start()
	
	void OnActivate(bool isTrue)
	{
		if (isTrue && state == State.On)
			hSoundManagerCS.toggleMusicEnabled(true);
		else if (isTrue && state == State.Off)
			hSoundManagerCS.toggleMusicEnabled(false);
	}
		
}
