using UnityEngine;
using System.Collections;

public class BossControllerCS : MonoBehaviour {

	public GameObject[] Bombs;

	private float fFrequence = 2;
	private float fTime = 0.0f;


	private CheckPointsMainCS hCheckPointsMainCS;
	// Use this for initialization
	void Start () {
		fTime = fFrequence;

		hCheckPointsMainCS = (CheckPointsMainCS)GameObject.Find("Player").GetComponent(typeof(CheckPointsMainCS));
	}

	void dropBomb(Vector3 pos){
		GameObject obj = (GameObject)Instantiate(Bombs[Random.Range(0,Bombs.Length)]);
		obj.transform.parent = hCheckPointsMainCS.getBossPatch ().transform;
		((BombCS)(obj.GetComponentInChildren<BombCS>())).move(transform.position,pos,1.0f);
	}

	public void Bomb(){

	}
	
	// Update is called once per frame
	void Update () {
		fTime -= Time.deltaTime;

		if (fTime > 0) {
			return;
		}
		fTime = fFrequence;	
		GameObject bosspatch = hCheckPointsMainCS.getBossPatch ();
		if (bosspatch == null) {
			return;
		}

		Matrix4x4 mat = new Matrix4x4 ();
		Vector3 angule = transform.eulerAngles;
		mat.SetTRS (new Vector3(0,0,0),Quaternion.Euler(new Vector3(angule.x,angule.y+90,angule.z)),new Vector3(1,1,1));


		RaycastHit hitInfo;
		if (Physics.Linecast (this.transform.position, this.transform.position + new Vector3 (0, -100, 0), out hitInfo, (1 << LayerMask.NameToLayer ("Terrain_lyr")))) {
			Vector3 center = hitInfo.point;
			Vector3 left = hitInfo.point + mat.MultiplyVector(new Vector3(0,0,-15));
			Vector3 right = hitInfo.point + mat.MultiplyVector(new Vector3(0,0,15));
			switch( Random.Range(0,6))
			{
			case 0:
			{
				dropBomb(left);
			}
				break;
			case 1:
			{
				dropBomb(center);
			}
				break;
			case 2:
			{
				dropBomb(right);
			}
				break;
			case 3:
			{
				dropBomb(left);
				dropBomb(center);
			}
				break;
			case 4:
			{
				dropBomb(right);
				dropBomb(center);
			}
				break;
			case 5:
			{
				dropBomb(left);
				dropBomb(right);
			}
				break;
			default:
				Debug.Log("!!!!!!!!!!!!");
				break;
			}
		}




	}
}
