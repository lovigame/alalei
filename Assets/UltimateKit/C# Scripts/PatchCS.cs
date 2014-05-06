using UnityEngine;
using System.Collections;

public class PatchCS : MonoBehaviour {

	private ElementsGeneratorCS hElementsGeneratorCS;
	// Use this for initialization
	public Transform[] vPowerups;
	void Start () {
		hElementsGeneratorCS = (ElementsGeneratorCS)GameObject.Find("Player").GetComponent(typeof(ElementsGeneratorCS));
	 	Transform powerups = this.transform.Find ("Powerup");
		if (powerups == null) {
			return;
		}
		vPowerups = new Transform[powerups.childCount];
		int i = 0;
		foreach (Transform powerup in powerups) 
		{
			GameObject type = hElementsGeneratorCS.getRandomPowerup();
			if(type == null){
				Destroy(powerup.gameObject);
				continue;
			}
			vPowerups[i] = ((GameObject)Instantiate(type, powerup.position, powerup.rotation)).transform ;
			vPowerups[i].parent = this.transform;
			Destroy(powerup.gameObject);
			i++;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
