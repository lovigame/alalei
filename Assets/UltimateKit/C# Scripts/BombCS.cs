using UnityEngine;
using System.Collections;

public class BombCS : MonoBehaviour {

	private Vector3 vFrom;
	private Vector3 vTo;
	private bool moving = false;
	private float fTime = 1.0f;
	private float fElapse = 0.0f;

	private bool boomed = false;
	private bool bDead = false;
	private ControllerScriptCS hControllerScriptCS;
	private PlayerSidesColliderScriptCS hPlayerSidesColliderScriptCS;
	private InGameScriptCS hInGameScriptCS;
	public enum BombType
	{	
		BTBomb = 0,
		BTBullet,
		
	}

	public BombType eType;
	// Use this for initialization
	void Start () {
		hControllerScriptCS = (ControllerScriptCS)GameObject.Find("Player").GetComponent(typeof(ControllerScriptCS));

		hPlayerSidesColliderScriptCS = (PlayerSidesColliderScriptCS)GameObject.Find("PlayerSidesCollider").GetComponent(typeof(PlayerSidesColliderScriptCS));
		hInGameScriptCS = (InGameScriptCS)GameObject.Find("Player").GetComponent(typeof(InGameScriptCS));
	}

	public void move(Vector3 from,Vector3 to,float time){
		moving = true;
		transform.position = from;
		vFrom = from;
		vTo = to;
		fElapse = 0.0f;
		fTime = time;
	}
	
	// Update is called once per frame
	void Update () {
		if (moving) {
			if(bDead){
				if(hControllerScriptCS.getBoss() != null){
					vTo = hControllerScriptCS.getBoss().position;
				}

			}

			fElapse += Time.deltaTime;
			float r = fElapse/fTime;
			if(r > 1){
				r = 1;
				moving = false;
				transform.position = vTo;
				if(bDead){


					if(hControllerScriptCS.getBoss() != null){
						BossControllerCS bcs = (BossControllerCS)hControllerScriptCS.getBoss().GetComponent(typeof(BossControllerCS));
						bcs.Bomb();
					}

					Destroy(this.transform.parent.gameObject);
					Debug.Log("destroy");
				}
			}
			else{

					transform.position = new Vector3(Mathf.Lerp(vFrom.x,vTo.x, r), Mathf.Lerp(vFrom.y, vTo.y, r), Mathf.Lerp(vFrom.z, vTo.z, r));


			}
		}
	}

	public void boom(){
		if (boomed) {
			return;		
		}
		boomed = true;

		switch (eType) {
		case BombType.BTBomb:
		{
			Destroy(this.transform.parent.gameObject);

			hPlayerSidesColliderScriptCS.deactivateCollider();	//dont detect stumbles on death
			hInGameScriptCS.collidedWithObstacle();	//play the death scene
		}
			break;
		case BombType.BTBullet:
		{
			bDead = true;
			move (this.transform.position,hControllerScriptCS.getBoss().position,0.3f);
		}
			break;
		}
	}
}
