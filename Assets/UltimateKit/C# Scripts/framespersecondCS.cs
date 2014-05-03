using UnityEngine;
using System.Collections;

public class framespersecondCS : MonoBehaviour {

	// Attach this to a GUIText to make a frames/second indicator.
	//
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	//
	// It is also fairly accurate at very low FPS counts (<10).
	// We do this not by simply counting frames per interval, but
	// by accumulating FPS for each frame. This way we end up with
	// correct overall FPS even if the interval renders something like
	// 5.5 frames.
	
	private float updateInterval = 0.5f;
	private float accum = 0.0f; // FPS accumulated over the interval
	private int frames = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	
	private float FPS = 0.0f;
	private GUIText FPS_Text_Ref;
	
	void Start()
	{
	    timeleft = updateInterval;
	    //FPS_Text_Ref = (GameObject.Find("FPS_Text").GetComponent(GUIText) as GUIText);
	    FPS_Text_Ref = (GUIText)GetComponent(typeof(GUIText));
	}
	
	void Update()
	{
	    timeleft -= Time.deltaTime;
	    accum += Time.timeScale/Time.deltaTime;
	    ++frames;
	    
	    // Interval ended - update GUI text and start new interval
	    if( timeleft <= 0.0 )
	    {
	        // display two fractional digits (f2 format)
	        FPS = (accum/frames);
	        timeleft = updateInterval;
	        accum = 0.0f;
	        frames = 0;
	        FPS_Text_Ref.text = System.String.Empty+FPS;
	        //FPS_Text_Ref.text = "";
	    }
	    
	}
}
