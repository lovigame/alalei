/*
*	FUNCTION:
*	- This script detects inputs (swipes and gyro) and controls the 
*	player accordingly.	
*	- It also defines the physics that defines
*	the user's actions.	
*	- It is responsible for handling all player animations.
*	- It is responsible for process all collisions.
*	- It is responsible for initiating the death scene.
*	
*	USED BY: This script is a part of the "Player" prefab.
*/

using UnityEngine;
using System.Collections;

public class ControllerScriptCS : MonoBehaviour {

	enum StrafeDirection {Strafe_Left = 0, Strafe_Right = 1}
	public enum ControlType { Swipe = 1, Gyro = 2 }

	private Transform tPlayer;	//the main character transform
	private Transform tPlayerRotation;	//Player child transform to rotate it in game
	private Animation aPlayer;				//character animation
	
	private Animator aPlayerMecAnim;
	private bool mecanimEnabled=false;
	private Vector3 v3DefaultPlayerAnimPosition;
	private Vector3 v3DefaultPlayerAnimRotation;
	
	private Transform tPlayerSidesCollider;	//sides collider transform (detects stumble)
	private Transform tFrontCollider;			//front collider transfrom (detects collisions)
	private Vector3 v3BNCDefaultScale;
	private Vector3 v3BFCDefaultScale;
	
	//Variables
	private float fCurrentWalkSpeed;
	
	private float tCurrentAngle = 0.0f;	//current rotation along Y axis
	private float fJumpForwardFactor = 0.0f;	//movement speed increase on jump
	private float fCurrentUpwardVelocity = 0.0f;	//speed during the duration of jump
	private float fCurrentHeight = 0.0f;
	private float fContactPointY = 0.0f;	//y-axis location of the path
	
	//player state during gameplay
	private bool bInAir = false;
	private bool bJumpFlag = false;
	private bool bInJump = false;
	private bool bDiveFlag = false;			//force character to dive during jump
	private bool bExecuteLand = false;
	private bool bInStrafe = false;	
	private bool bInDuck = false;				//true if the character is sliding
	
	private float fForwardAccleration = 0.0f;
	private Transform tBlobShadowPlane;	//the shadow under the player
	private Vector3 CurrentDirection;//set player rotation according to path
	
	//script references
	private PatchesRandomizerCS hPatchesRandomizerCS;
	private CheckPointsMainCS hCheckPointsMainCS;
	private InGameScriptCS hInGameScriptCS;
	private PitsMainControllerCS hPitsMainControllerCS;
	private SoundManagerCS hSoundManagerCS;
	private CameraControllerCS hCameraControllerCS;
	private PowerupsMainControllerCS hPowerupScriptCS;
	private EnemyControllerCS hEnemyControllerCS;
	private MenuScriptCS hMenuScriptCS;
	private MissionsControllerCS hMissionsControllerCS;
	private GlobalAchievementControllerCS hGlobalAchievementControllerCS;
	private PlayerFrontColliderScriptCS hPlayerFrontColliderScriptCS;
	private PlayerSidesColliderScriptCS hPlayerSidesColliderScriptCS;
	
	private RaycastHit hitInfo;	//whats under the player character
	private bool bGroundhit = false;	//is that an object under the player character
	private float fHorizontalDistance = 0.0f;	//calculate player's horizontal distance on path
	
	private float fCurrentForwardSpeed = .5f;	//sets movement based on spline
	private float fCurrentDistance = 0.0f;//distance between the start and current position during the run
	private float fCurrentMileage = 0.0f;//used to calculate the score based on distance covered
	
	//detect if there is a terrain_lyr under the player
	private float fPitFallLerpValue = 0.0f;
	private float fPitFallForwardSpeed = 0.0f;
	private float fPitPositionX = 0.0f;//check the position of the pit in x-axis
	private float fDeathAnimStartTime = 0;
	private int iDeathAnimEndTime = 3;	//duration wait for death scene
	
	private bool JumpAnimationFirstTime = true;	//play death animation once
	private Camera HUDCamera;
	
	private Transform tPauseButton;
	private Transform tHUDGroup;
	private SwipeControlsCS swipeLogic;
	private int iLanePosition;						//current lane number -- -1, 0 or 1
	private int iLastLanePosition; 					//stores the previous lane on lane change
	private bool bMouseReleased = true;
	private bool bControlsEnabled = true;
	
	//action queue
	private SwipeControlsCS.SwipeDirection directionQueue;
	private bool bDirectionQueueFlag = false;
	
	//Physics Constants
	//change these to adjust the initial and final movement speed
	private float fStartingWalkSpeed = 150.0f;//when player starts running
	private float fEndingWalkSpeed = 230.0f;	//final speed after acclerating
	private float fCurrentWalkAccleration = 0.5f;	//rate of accleartion
	
	//change these to adjust the jump height and displacement
	private float fJumpPush = 185;			//force with which player pushes the ground on jump
	private int getAccleration() { return 500; }	//accleration and deceleration on jump
	
	//the initial distance of the player character at launch
	//from the start of the path
	private float fCurrentDistanceOnPath = 0.0f;
	
	//switch between gyro and swipe controls
	private bool swipeControlsEnabled = true;
	public bool isSwipeControlEnabled() { return swipeControlsEnabled; }
	
	public void toggleSwipeControls(bool state)
	{
		swipeControlsEnabled = state;
		
		//permanently save user preference of controls
		PlayerPrefs.SetInt("ControlsType", (state == true ? 1 : 0));
		PlayerPrefs.Save();
	}
	
	void Start()
	{
		//script references
		hPatchesRandomizerCS = (PatchesRandomizerCS)this.GetComponent(typeof(PatchesRandomizerCS));
		hMissionsControllerCS = (MissionsControllerCS)this.GetComponent(typeof(MissionsControllerCS));
		hGlobalAchievementControllerCS = (GlobalAchievementControllerCS)this.GetComponent(typeof(GlobalAchievementControllerCS));
		hPlayerSidesColliderScriptCS = (PlayerSidesColliderScriptCS)GameObject.Find("PlayerSidesCollider").GetComponent(typeof(PlayerSidesColliderScriptCS));
		hPlayerFrontColliderScriptCS = (PlayerFrontColliderScriptCS)GameObject.Find("PlayerFrontCollider").GetComponent(typeof(PlayerFrontColliderScriptCS));
		hSoundManagerCS = (SoundManagerCS)GameObject.Find("SoundManager").GetComponent(typeof(SoundManagerCS));
		hInGameScriptCS = (InGameScriptCS)this.GetComponent(typeof(InGameScriptCS));
		hPitsMainControllerCS = (PitsMainControllerCS)this.GetComponent(typeof(PitsMainControllerCS));
		hCheckPointsMainCS = (CheckPointsMainCS)this.GetComponent(typeof(CheckPointsMainCS));
		hPowerupScriptCS = (PowerupsMainControllerCS)this.GetComponent(typeof(PowerupsMainControllerCS));
		hEnemyControllerCS = (EnemyControllerCS)GameObject.Find("Enemy").GetComponent(typeof(EnemyControllerCS));
		hPowerupScriptCS = (PowerupsMainControllerCS)this.GetComponent(typeof(PowerupsMainControllerCS));
		hCameraControllerCS = (CameraControllerCS)GameObject.Find("Main Camera").GetComponent(typeof(CameraControllerCS));
		swipeLogic = (SwipeControlsCS)transform.GetComponent(typeof(SwipeControlsCS));
		
		//check which type of menu (Custom or NGUI) to work with
		if (hInGameScriptCS.isCustomMenuEnabled())
		{
			hMenuScriptCS = (MenuScriptCS)GameObject.Find("MenuGroup").GetComponent(typeof(MenuScriptCS));
			HUDCamera = GameObject.Find("HUDCamera").camera;
			
			tHUDGroup = GameObject.Find("HUDMainGroup/HUDGroup").transform;
			tPauseButton = GameObject.Find("HUDMainGroup/HUDGroup/HUDPause").transform;
		}
		
		tPlayer = transform;
		tPlayerRotation = transform.Find("PlayerRotation");
		
		//get the animation component of the player character
		if (this.transform.Find("PlayerRotation/PlayerMesh/Prisoner"))
		{
			mecanimEnabled = false;
			aPlayer = (Animation)this.transform.Find("PlayerRotation/PlayerMesh/Prisoner").GetComponent(typeof(Animation));			
			StartCoroutine("playIdleAnimations");//start playing idle animations
		}
		else if (this.transform.Find("PlayerRotation/PlayerMesh/Prisoner(MecAnim)"))//check for mecanim animated character
		{
			mecanimEnabled = true;
			aPlayerMecAnim = (Animator)this.transform.Find("PlayerRotation/PlayerMesh/Prisoner(MecAnim)").GetComponent(typeof(Animator));
			
			v3DefaultPlayerAnimPosition = aPlayerMecAnim.transform.localPosition;//get the default player position
			v3DefaultPlayerAnimRotation = aPlayerMecAnim.transform.localEulerAngles;//get the default player rotation		
		}
			
		tBlobShadowPlane = transform.Find("BlobShadowPlane");//get the shadow
		
		tPlayerSidesCollider = GameObject.Find("PlayerSidesCollider").transform;//get the sides collider to detect stumbles
		tFrontCollider = GameObject.Find("PlayerFrontCollider").transform;//get the front collider to detect collisions
		
		v3BNCDefaultScale = tFrontCollider.localScale;	
		v3BFCDefaultScale = tPlayerSidesCollider.localScale;
		
		bInAir = false;
		fCurrentDistanceOnPath = 50.0f;	//inital distance with respect to spline
		fCurrentDistance = 0.0f;
		fCurrentMileage = 0.0f;
		tCurrentAngle = 0.0f;	
		fPitFallLerpValue = 0.0f;
		fPitFallForwardSpeed = 0.0f;
		fPitPositionX = 0.0f;
		fDeathAnimStartTime = 0;
		bGroundhit = false;	
		bJumpFlag = false;
		bInJump = false;
		fCurrentUpwardVelocity = 0;
		fCurrentHeight = 0;
		
		bDirectionQueueFlag = false;
		directionQueue = SwipeControlsCS.SwipeDirection.Null;
		iLanePosition = 0;	//set current lane to mid	
		fCurrentWalkSpeed = fStartingWalkSpeed;
		
		//get the type of controls (swipe or gyro) set by user
		if (PlayerPrefs.HasKey("ControlsType"))
			swipeControlsEnabled = PlayerPrefs.GetInt("ControlsType") == 1 ? true : false;
		else
			PlayerPrefs.SetInt("ControlsType", (swipeControlsEnabled == true ? 1 : 0));
			
		//stop footsteps sound if playing
		hSoundManagerCS.stopSound(SoundManagerCS.CharacterSounds.Footsteps);
	}//end of Start()
	
	/*
	 * FUNCTION:	Play and alternate between the two idle animations
	 * 				when the game is launched/ restarted.
	 * CALLED BY:	Start()
	 * */
	private IEnumerator playIdleAnimations()
	{
		while(true)
		{
			yield return new WaitForFixedUpdate();
			
			//check if idle animations are not being played
			if (!aPlayer.IsPlaying("Idle_1") && !aPlayer.IsPlaying("Idle_2"))
			{
				aPlayer.animation.Play("Idle_1");//play the idle animation
				//wait for the current animation to end and play the second idle animation
				aPlayer.PlayQueued("Idle_2", QueueMode.CompleteOthers);
			}
		}//end of while
	}
	
	/*
	*	FUNCTION: Enable controls, start player animation and movement
	*/
	public void launchGame()
	{
		StopCoroutine("playIdleAnimations");//stop idle animations
		hEnemyControllerCS.launchEnemy();
				
		if (!mecanimEnabled)//if legacy animations enabled
		{
			togglePlayerAnimation(true);
			aPlayer["run"].speed = Mathf.Clamp( (fCurrentWalkSpeed/fStartingWalkSpeed)/1.1f, 0.8f, 1.2f );
			aPlayer.Play("run");
		}
		else//if mecanim enabled
			aPlayerMecAnim.SetBool("RunAnim", true);	
		
		hSoundManagerCS.playSound(SoundManagerCS.CharacterSounds.Footsteps);//play the footsteps sound
	}
	
	void Update()
	{	
		if(hInGameScriptCS.isGamePaused()==true)
			return;
		
		if (hInGameScriptCS.isEnergyZero())
			if(DeathScene())
				return;
		
		if (hInGameScriptCS.isCustomMenuEnabled())
			getClicks();	//get taps/clicks for pause menu etc.
		
		if (mecanimEnabled)//reset parameters for next frame
		{
			aPlayerMecAnim.SetBool("StrafeLeftAnim", false);
			aPlayerMecAnim.SetBool("StrafeRightAnim", false);			
		}
		
		if (bControlsEnabled)
			SwipeMovementControl();
	}//end of update()
	
	void FixedUpdate()
	{
		if (mecanimEnabled)//set position and rotation of the mesh to its original values
		{
			aPlayerMecAnim.transform.localPosition = v3DefaultPlayerAnimPosition;
			aPlayerMecAnim.transform.localEulerAngles = v3DefaultPlayerAnimRotation;
		}
				
		if(hInGameScriptCS.isGamePaused() == true)
			return;
		
		setForwardSpeed();
		SetTransform();
		setShadow();
			
		if(!bInAir)
		{
			if(bExecuteLand)
			{
				hSoundManagerCS.playSound(SoundManagerCS.CharacterSounds.JumpLand);
				bExecuteLand = false;
				JumpAnimationFirstTime = true;
			}
		}//end of if not in air
		else
		{		
			if(JumpAnimationFirstTime&&bInJump==true)
			{
				if (!mecanimEnabled)
					aPlayer.Rewind("jump");					
					
				JumpAnimationFirstTime = false;
				bInDuck = false;
				
				if (!mecanimEnabled)
					aPlayer.CrossFade("jump", 0.1f);
				else
				{
					aPlayerMecAnim.SetBool("JumpAnim", true);				
				}
			}
		}//end of else !in air
	
		if(bJumpFlag==true)
		{		
			bJumpFlag = false;
			bExecuteLand = true;
			bInJump = true;
			bInAir = true;
			
			if (mecanimEnabled)//if mecanim animations are used
			{
				aPlayerMecAnim.SetBool("RunAnim", false);//disable run animation
				aPlayerMecAnim.SetBool("DuckAnim", false);//disable slide animation
			}		
			
			fCurrentUpwardVelocity = fJumpPush;
			fCurrentHeight = tPlayer.position.y;
			
			hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.Jump);//count jumps for mission script
			hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.Jump);//count jumps for global achievements script
		}
			
		//acclerate movement speed with time
		if(fCurrentWalkSpeed<fEndingWalkSpeed)
			fCurrentWalkSpeed += (fCurrentWalkAccleration * Time.fixedDeltaTime);
		
		if (!mecanimEnabled)
			aPlayer["run"].speed = Mathf.Clamp( (fCurrentWalkSpeed/fStartingWalkSpeed)/1.1f, 0.8f, 1.2f );	//set run animation speed according to current speed
	}//end of Fixed Update
	
	/*
	*	FUNCTION: Check if pause button is tapped in-game
	*	CALLED BY:	Update()
	*/
	private void getClicks()
	{
		if(Input.GetMouseButtonUp(0) && bMouseReleased==true)
		{
			Vector3 screenPoint;
			Vector2 buttonSize;
			Rect Orb_Rect;
			
			if (tHUDGroup.localPosition.z==0)
			{
				buttonSize = new Vector2(Screen.width/6,Screen.width/6);
				screenPoint = HUDCamera.WorldToScreenPoint( tPauseButton.position );
				
				Orb_Rect = new Rect (screenPoint.x - ( buttonSize.x * 0.5f ), screenPoint.y - ( buttonSize.y * 0.5f ), buttonSize.x, buttonSize.y);
				if(Orb_Rect.Contains(Input.mousePosition))
				{				
					hInGameScriptCS.pauseGame();
				}
			}
			
			//Orb_Rect = new Rect (screenPoint.x - ( buttonSize.x * 0.5f ), screenPoint.y - ( buttonSize.y * 0.5f ), buttonSize.x, buttonSize.y);
		}//end of mouserelease == true if
			
	}//end of get clicks function
	
	/*
	*	FUNCITON: Set the position of the shadow under the player and of the
	*				colliders to make them move with the character mesh.
	*	CALLED BY:	FixedUpdate()
	*/
	private void setShadow()
	{	
		tBlobShadowPlane.up = hitInfo.normal;
		//set shadow's position
		tBlobShadowPlane.position = new Vector3(tBlobShadowPlane.position.x, fContactPointY+0.7f, tBlobShadowPlane.position.z);
		//set shadow's rotation
		tBlobShadowPlane.localEulerAngles = new Vector3(tBlobShadowPlane.localEulerAngles.x,
			tPlayerRotation.localEulerAngles.y, tBlobShadowPlane.localEulerAngles.z);
		
		//set side collider's position and rotation
		tPlayerSidesCollider.position = tPlayer.position + new Vector3(0,5,0);
		tPlayerSidesCollider.localEulerAngles = tBlobShadowPlane.localEulerAngles;//set 
		
		//set front collider's position and rotation
		tFrontCollider.position = tPlayer.position + new Vector3(7,5,0);
		tFrontCollider.localEulerAngles = tBlobShadowPlane.localEulerAngles;
	}
	
	/*
	*	FUNCTION: Set the player's position the path with reference to the spline
	*	CALLED BY:	FixedUpdate()
	*/
	private void SetTransform()
	{
		int iStrafeDirection = (int)getLeftRightInput();	//get the current lane (-1, 0 or 1)
		
		fCurrentDistanceOnPath = hCheckPointsMainCS.SetNextMidPointandRotation(fCurrentDistanceOnPath, fCurrentForwardSpeed);//distance on current patch
		fCurrentDistance = fCurrentDistanceOnPath + hPatchesRandomizerCS.getCoveredDistance();//total distance since the begining of the run
		fCurrentMileage = fCurrentDistance/12.0f;//calculate milage to display score on HUD
		
		tCurrentAngle = hCheckPointsMainCS.getCurrentAngle();//get the angle according to the position on path
		//set player rotation according to the current player position on the path's curve (if any)
		tPlayerRotation.localEulerAngles = new Vector3(tPlayerRotation.localEulerAngles.x, -tCurrentAngle, tPlayerRotation.localEulerAngles.z);
		
		CurrentDirection = hCheckPointsMainCS.getCurrentDirection();
		Vector3 Desired_Horinzontal_Pos = calculateHorizontalPosition(iStrafeDirection);
		
		bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + new Vector3(0,20,0),Desired_Horinzontal_Pos + new Vector3(0,-100,0), out hitInfo,(1<<9));	
		
		if(bGroundhit && hPitsMainControllerCS.isFallingInPit()==false)//calculate player position in y-axis
			fContactPointY = hitInfo.point.y;
		else//call death if player in not on Terrain_lyr
		{
			fContactPointY = -10000.0f;
			if(!bInAir)
			{
				if(!bInJump)
				{
					if(reConfirmPitFalling(Desired_Horinzontal_Pos,iStrafeDirection)==true)
					{
						hPitsMainControllerCS.setPitValues();
					}
				}
				bInAir = true;
				fCurrentUpwardVelocity = 0;
				fCurrentHeight = tPlayer.position.y;
			}
		}
		
		if(!bInAir)//set player position when not in air
		{			
			tPlayer.position = new Vector3(tPlayer.position.x,
					fContactPointY+0.6f, tPlayer.position.z);
		}
		else//set player position if in air
		{
			if (bDiveFlag)	//dive during jump
			{
				setCurrentDiveHeight();
				tPlayer.position = new Vector3(tPlayer.position.x,
					fCurrentHeight, tPlayer.position.z);
			}
			else			//JUMP
			{
				setCurrentJumpHeight();
				tPlayer.position = new Vector3(tPlayer.position.x,
					fCurrentHeight, tPlayer.position.z);				
			}
		}
		
		tPlayer.position = new Vector3(Desired_Horinzontal_Pos.x,
			tPlayer.position.y,	Desired_Horinzontal_Pos.z);//set player position in x and z axis
		
	}//end of Set Transform()
	
	/*
	*	FUNCTION: Set the height of the player during jump
	*	CALLED BY:	SetTransform()
	*/
	private void setCurrentJumpHeight()		//set height during jump
	{
		fCurrentUpwardVelocity-=Time.fixedDeltaTime*getAccleration();
		fCurrentUpwardVelocity = Mathf.Clamp(fCurrentUpwardVelocity,-fJumpPush,fJumpPush);
		fCurrentHeight+=fCurrentUpwardVelocity*(Time.fixedDeltaTime/1.4f);
		
		if(fCurrentHeight<fContactPointY)
		{
			fCurrentHeight = fContactPointY;
			bInAir = false;
			bInJump = false;
			
			if (bDiveFlag)	//do not resume run animation on Dive
				return;
					
			if (!hInGameScriptCS.isEnergyZero())
			{	
				if (!mecanimEnabled)
					aPlayer.CrossFade("run", 0.1f);
				else
				{
					aPlayerMecAnim.SetBool("JumpAnim", false);
					aPlayerMecAnim.SetBool("RunAnim", true);
				}
			}//end of if current energy > 0
		}
	}
	
	/*
	*	FUNCITON: Pull the player down faster if user swipes down int the middle of jump
	*	CALLED BY:	SetTransform()
	*/
	private void setCurrentDiveHeight()	//set height after dive called
	{
		fCurrentUpwardVelocity-=Time.fixedDeltaTime*2000;
		fCurrentUpwardVelocity = Mathf.Clamp(fCurrentUpwardVelocity,-fJumpPush,fJumpPush);
		if(hPitsMainControllerCS.isFallingInPit() == false)
			fCurrentHeight+=fCurrentUpwardVelocity*Time.fixedDeltaTime;
		else
		{
			fCurrentHeight-=40.0f*Time.fixedDeltaTime;
			hMenuScriptCS.hideHUDElements();
		}	
		
		if(fCurrentHeight<=fContactPointY)
		{
			fCurrentHeight = fContactPointY;//bring character down completely
				
			bInAir = false;
			bInJump = false;
			
			if (mecanimEnabled)
				aPlayerMecAnim.SetBool("JumpAnim", false);
			
			duckPlayer();//make the character slide
			bDiveFlag = false;		//dive complete
		}//end of if
	}
	
	/*
	*	FUNCTION: 	Make sure that there is no terrain under the player
	*				before making it fall
	*	CALLED BY:	SetTransform()
	*/
	private bool reConfirmPitFalling(Vector3 Desired_Horinzontal_Pos, float iStrafeDirection)
	{
		bool bGroundhit = false;
		
		if(iStrafeDirection>=0)
			bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + new Vector3(1,20,5),Desired_Horinzontal_Pos + new Vector3(0,-100,5), out hitInfo,1<<9);
		else
			bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + new Vector3(1,20,-5),Desired_Horinzontal_Pos + new Vector3(0,-100,-5), out hitInfo,1<<9);
		
		if(!bGroundhit)
			return true;
		else
			return false;
	}
	
	/*
	*	FUNCTION: Called when user runs out of energy
	*	CALLED BY:	Update()
	*/
	private bool DeathScene()
	{
		bInAir = false;
		tPlayerRotation.localEulerAngles = new Vector3(0,0,0);
		
		if (fDeathAnimStartTime == 0)
		{
			hSoundManagerCS.stopSound(SoundManagerCS.CharacterSounds.Footsteps);
			bControlsEnabled = false;
					
			/*Vector3 v3EffectPosition = this.transform.position;
			v3EffectPosition.x += 15;
			v3EffectPosition.y += 5;*/
			
			if (!mecanimEnabled)//if legacy animations enabled
				aPlayer.CrossFade("death",0.1f);
			else//if mecanim enabled
			{
				aPlayerMecAnim.SetBool("DeathAnim", true);
				aPlayerMecAnim.SetBool("RunAnim", false);
			}
			hEnemyControllerCS.playDeathAnimation();
			
			if (hInGameScriptCS.isCustomMenuEnabled())
				hMenuScriptCS.hideHUDElements();
			
			fDeathAnimStartTime = Time.time;
		}	
		else if (fDeathAnimStartTime != 0 && (Time.time - fDeathAnimStartTime) >= iDeathAnimEndTime)
		{		
			hInGameScriptCS.setupDeathMenu();
			return true;
		}
		
		return false;
	}
	
	/*
	*	FUNCTION: Called when player hits an obstacle sideways
	*	CALLED BY: PlayerSidesColliderScript.OnCollisionEnter()
	*/
	public void processStumble()
	{
		hCameraControllerCS.setCameraShakeImpulseValue(1);
		iLanePosition = iLastLanePosition;	//stop strafe
			
		if (hEnemyControllerCS.processStumble())
		{	
			hInGameScriptCS.collidedWithObstacle();//call death if player stumbled twice in unit time
		}
		else
		{
			if (!mecanimEnabled)
				aPlayer.PlayQueued("run", QueueMode.CompleteOthers);
			
			//enable colliders if they were disabled
			hPlayerFrontColliderScriptCS.activateCollider();
			hPlayerSidesColliderScriptCS.activateCollider();
		}	
	}
	
	/*
	*	FUNCTION: Returns horizontal the position to move to
	*	CALLED BY: SetTransform()
	*/
	private float getLeftRightInput()	//change lane
	{
		if (swipeControlsEnabled == true)//swipe direction
			return iLanePosition;
		else//gyro direction
		{
			float fMovement = 0.0f;
			float fSign = 1.0f;
			
			if(Screen.orientation == ScreenOrientation.Portrait)
				fSign = 1.0f;
			else
				fSign = -1.0f;
			
			if(Application.isEditor)//map gyro controls on mouse in editor mode
			{
				fMovement = (Input.mousePosition.x - (Screen.height/2.0f))/(Screen.height/2.0f) * 4.5f;
			}
			else
			{
				fMovement = (fSign * Input.acceleration.x * 4.5f);
			}
			
			return fMovement;
		}
	}
	
	/*
	*	FUNCTION: Set the movement speed
	*	CALLED BY: FixedUpdate()
	*/
	private void setForwardSpeed()
	{
		//if the player is not on Terrain_lyr
		if(hPitsMainControllerCS.isFallingInPit() == true)
		{		
			if(transform.position.x>fPitPositionX)
				fCurrentForwardSpeed = 0.0f;
			else
				fCurrentForwardSpeed = Mathf.Lerp(fPitFallForwardSpeed,0.01f,(Time.time-fPitFallLerpValue)*3.5f);
			return;
		}
		
		if (hInGameScriptCS.isEnergyZero())//on death
		{
			fCurrentForwardSpeed = 0;
			return;
		}
		
		if(bInAir)
			fForwardAccleration = 1.0f;
		else
			fForwardAccleration = 2.0f;
			
		fJumpForwardFactor = 1 + ((1/fCurrentWalkSpeed)*50);
			
		if(bInJump==true)
			fCurrentForwardSpeed = Mathf.Lerp(fCurrentForwardSpeed,fCurrentWalkSpeed*Time.fixedDeltaTime*fJumpForwardFactor,Time.fixedDeltaTime*fForwardAccleration);
		else
			fCurrentForwardSpeed = Mathf.Lerp(fCurrentForwardSpeed,(fCurrentWalkSpeed)*Time.fixedDeltaTime,Time.fixedDeltaTime*fForwardAccleration);
	}
	
	/*
	*	FUNCTION: Make the player change lanes
	*	CALLED BY:	SetTransform()
	*/
	private float fCurrentStrafePosition = 0.0f;	//keeps track of strafe position at each frame
	private float fSpeedMultiplier = 5.0f;	//how fast to strafe/ change lane
	private Vector3 calculateHorizontalPosition(int iStrafeDirection)
	{
		Vector2 fHorizontalPoint;
		Vector2 SideDirection_Vector2;
			
		if (swipeControlsEnabled == true)
		{
			SideDirection_Vector2 = rotateAlongZAxis(new Vector2(CurrentDirection.x,CurrentDirection.z),90.0f);
			SideDirection_Vector2.Normalize();
				
			if(iStrafeDirection==-1)//strafe left from center
			{
				if(fCurrentStrafePosition>-1)
				{
					fCurrentStrafePosition-= fSpeedMultiplier*Time.fixedDeltaTime;
					if(fCurrentStrafePosition<=-1.0f)
					{
						fCurrentStrafePosition = -1.0f;
						switchStrafeToSprint();
					}
				}
			}
			else if(iStrafeDirection==1)//strafe right from center
			{
				if(fCurrentStrafePosition<1)
				{
					fCurrentStrafePosition+= fSpeedMultiplier*Time.fixedDeltaTime;
					if(fCurrentStrafePosition>=1.0f)
					{
						fCurrentStrafePosition = 1.0f;
						switchStrafeToSprint();
					}
				}
			}
			else if(iStrafeDirection==0&&fCurrentStrafePosition!=0.0f)//strafe from left or right lane to center
			{	
				if(fCurrentStrafePosition<0)
				{
					fCurrentStrafePosition+= fSpeedMultiplier*Time.fixedDeltaTime;
					if(fCurrentStrafePosition>=0.0f)
					{
						fCurrentStrafePosition = 0.0f;
						switchStrafeToSprint();
					}
				}
				else if(fCurrentStrafePosition>0)
				{
					fCurrentStrafePosition-= fSpeedMultiplier*Time.fixedDeltaTime;
					if(fCurrentStrafePosition<=0.0f)
					{
						fCurrentStrafePosition = 0.0f;
						switchStrafeToSprint();
					}
				}
			}//end of else
				
			fHorizontalDistance = -fCurrentStrafePosition*16.0f;	
			fHorizontalDistance = Mathf.Clamp(fHorizontalDistance,-20.0f,20.0f);
			
			fHorizontalPoint = hCheckPointsMainCS.getCurrentMidPoint() + SideDirection_Vector2*fHorizontalDistance;
				
			return new Vector3(fHorizontalPoint.x,tPlayerRotation.position.y,fHorizontalPoint.y);
		}
		else
		{
			SideDirection_Vector2 = rotateAlongZAxis(new Vector2(CurrentDirection.x,CurrentDirection.z),90.0f);
			SideDirection_Vector2.Normalize();
			
			fHorizontalDistance = Mathf.Lerp(fHorizontalDistance,-iStrafeDirection * 40.0f, 0.05f*fCurrentForwardSpeed);		
			fHorizontalDistance = Mathf.Clamp(fHorizontalDistance,-20.0f,20.0f);		
			fHorizontalPoint = hCheckPointsMainCS.getCurrentMidPoint() + SideDirection_Vector2*fHorizontalDistance;
					
			return new Vector3(fHorizontalPoint.x,tPlayerRotation.position.y,fHorizontalPoint.y);
		}//end of else
	}
	
	/*
	*	FUNCTION: Determine the rotation of the player character
	*/
	private Vector2 rotateAlongZAxis(Vector2 inputVector, float angletoRotate)
	{
		Vector2 FinalVector = Vector2.zero;
		angletoRotate = angletoRotate/57.3f;
		FinalVector.x = Mathf.Cos(angletoRotate) * inputVector.x - Mathf.Sin(angletoRotate) * inputVector.y;
		FinalVector.y = Mathf.Sin(angletoRotate) * inputVector.x + Mathf.Cos(angletoRotate) * inputVector.y;
		
		return FinalVector;
	}
	
	/*
	*	FUNCTION: Play the "run" animation
	*	CALLED BY:	calculateHorizontalPosition()
	*/
	private void switchStrafeToSprint()
	{
		if (!hInGameScriptCS.isEnergyZero() && !isInAir())
		{
			if (!mecanimEnabled)
				aPlayer.CrossFade("run", 0.1f);
			else
				aPlayerMecAnim.SetBool("RunAnim",true);
			bInStrafe = false;
		}	
	}
	
	/*
	*	FUNCITON: Detect swipes on screen
	*	CALLED BY: Update()
	*/
	void SwipeMovementControl()
	{	
		//check and execute two jump or duck commands simultaneously
		if (bDirectionQueueFlag)
		{
			if(!bInAir && directionQueue == SwipeControlsCS.SwipeDirection.Jump)		//queue JUMP
			{
				bJumpFlag = true;			
				bDirectionQueueFlag = false;
			}//end of jump queue
			if (directionQueue == SwipeControlsCS.SwipeDirection.Duck && !bInDuck)		//queue SLIDE
			{
				duckPlayer();			
				bDirectionQueueFlag = false;
			}//end of duck queue
			
		}//end of direction queue
	
		//restore the size of the collider after slide ends
		if ( (mecanimEnabled && aPlayerMecAnim.GetAnimatorTransitionInfo(0).nameHash == Animator.StringToHash("Base.Slide -> Base.Run"))
		|| (!mecanimEnabled && !isPlayingDuck() && bInDuck == true) )//is the slide animation playing?
		{
			hSoundManagerCS.playSound(SoundManagerCS.CharacterSounds.Footsteps);
			
			//rotation correction after DIVE
			tPlayerRotation.localEulerAngles = new Vector3(tPlayerRotation.localEulerAngles.x, tPlayerRotation.localEulerAngles.y,0);
			//translation correction after DIVE (to fix mysterious bug :S)
			tBlobShadowPlane.localPosition = new Vector3(0, tBlobShadowPlane.localPosition.y, tBlobShadowPlane.localPosition.z);
					
			bInDuck = false;
			tFrontCollider.localScale = v3BNCDefaultScale;
			tPlayerSidesCollider.localScale = v3BFCDefaultScale;//restore far collider
			
			if (bDiveFlag)	//do not resume run animation on Dive
				return;
			
			if (!mecanimEnabled)
				aPlayer.CrossFadeQueued("run", 0.5f, QueueMode.CompleteOthers);
			else
			{
				aPlayerMecAnim.SetBool("DuckAnim", false);
				aPlayerMecAnim.SetBool("RunAnim", true);
			}
		}//end of if end of duck animation
	
		//swipe controls
		var direction = swipeLogic.getSwipeDirection();	//get the swipe direction	
		if (direction != SwipeControlsCS.SwipeDirection.Null)
		{
			bMouseReleased = false;//disallow taps on swipe
			
			if (direction == SwipeControlsCS.SwipeDirection.Jump)	//JUMP
			{
				if(!bInAir)
				{					
					bJumpFlag = true;
				}
				if (bInAir)	//queue the second jump if player swipes up in the middle of a jump
				{
					bDirectionQueueFlag = true;
					directionQueue = SwipeControlsCS.SwipeDirection.Jump;
				}
			}//end of if direction is jump
			if (direction == SwipeControlsCS.SwipeDirection.Right && swipeControlsEnabled == true)	//RIGHT swipe
			{
				if (iLanePosition != 1) 
				{
					iLastLanePosition = iLanePosition;
					iLanePosition++;
					
					strafePlayer(StrafeDirection.Strafe_Right);
					
				}//end of lane check if
			}//end of swipe direction if
			if (direction == SwipeControlsCS.SwipeDirection.Left && swipeControlsEnabled == true)	//LEFT swipe
			{
				if (iLanePosition != -1) 
				{
					iLastLanePosition = iLanePosition;
					iLanePosition--;
					
					strafePlayer(StrafeDirection.Strafe_Left);
					
				}//end of lane check if
			}//end of swipe direction if
			if (direction == SwipeControlsCS.SwipeDirection.Duck && bInDuck)//SLIDE: queue the second duck command if player is in the middle of slide animation
			{
				bDirectionQueueFlag = true;
				directionQueue = SwipeControlsCS.SwipeDirection.Duck;
			}
			if (direction == SwipeControlsCS.SwipeDirection.Duck && !bInAir && !bInDuck)//SLIDE: on ground
			{
				duckPlayer();
			}
			if (direction == SwipeControlsCS.SwipeDirection.Duck && bInAir && !bInDuck)//SLIDE/ DIVE: in air
			{				
				bDiveFlag = true;	//used by Set Transform() to make the character dive
			}//end of slide in air if
			
			//swipeLogic.iTouchStateFlag = 2;
		}//end of if	
		if (Input.GetMouseButtonUp(0))	//allow taps on mouse/ tap release
		{
			bMouseReleased = true;
		}
			
		//keyboard controls (DEBUG)
		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))//Up/ jump
		{
			if(!bInAir)
			{					
				bJumpFlag = true;
			}
			if (bInAir)
			{
				bDirectionQueueFlag = true;
				directionQueue = SwipeControlsCS.SwipeDirection.Jump;
			}
		}
		else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))//Right
		{
			if (iLanePosition != 1) 
			{
				iLastLanePosition = iLanePosition;
				iLanePosition++;
				
				strafePlayer(StrafeDirection.Strafe_Right);
				
			}//end of lane check if
		}
		else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))//Left
		{
			if (iLanePosition != -1) 
			{
				iLastLanePosition = iLanePosition;
				iLanePosition--;
				
				strafePlayer(StrafeDirection.Strafe_Left);
				
			}//end of lane check if
		}
		else if ( (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && bInDuck)
		{
			bDirectionQueueFlag = true;
			directionQueue = SwipeControlsCS.SwipeDirection.Duck;
		}
		else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !bInAir && !bInDuck)
		{
			duckPlayer();
		}
		else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && bInAir && !bInDuck)
		{
			bDiveFlag = true;	//used by Set Transform() to make the character dive
		}
		
	}//end of Movement Control function
	
	/*
	*	FUNCTION: make the character slide
	*	CALLED BY: SwipeMovementControl()
	*/
	private void duckPlayer()
	{
		bInDuck = true;
		hSoundManagerCS.stopSound(SoundManagerCS.CharacterSounds.Footsteps);
		
		if (!mecanimEnabled)//if legacy animations are used
		{
			aPlayer["slide"].speed = 1.4f;
			aPlayer.CrossFade("slide", 0.1f);
		}
		else//if mecanim are used
		{		
			aPlayerMecAnim.SetBool("DuckAnim", true);
		}
		
		tFrontCollider.localScale = v3BNCDefaultScale/2;	//reduce the near collider size
		tPlayerSidesCollider.localScale = v3BFCDefaultScale/2;	//reduce the far collider size
		
		hMissionsControllerCS.incrementMissionCount(MissionsControllerCS.MissionTypes.Duck);//count ducks for mission script
		hGlobalAchievementControllerCS.incrementAchievementCount(GlobalAchievementControllerCS.GlobalAchievementTypes.Duck);//count ducks for global achievements script
	}
	
	/*
	*	FUNCTION: Check if the user is sliding
	*/
	private bool isPlayingDuck()
	{
		if (hInGameScriptCS.isEnergyZero())
			return false;
		
		if (!mecanimEnabled)
		{
			if (aPlayer.IsPlaying("slide"))
				return true;
			else
				return false;
		}
		else
			return aPlayerMecAnim.GetBool("DuckAnim");
	}
	
	/*
	*	FUNCTION: strafe charater right or left
	*	INPUT: "right" OR "left"
	*	OUTPUT: move the character left or right
	*/
	private void strafePlayer(StrafeDirection strafeDirection)
	{
		if (!mecanimEnabled)
		{
			string anim = strafeDirection == StrafeDirection.Strafe_Right ? "right" : "left";
			if (isInAir())
			{	
				aPlayer[anim].speed = 2;
				aPlayer.Play(anim);
				//aPlayer.CrossFade("glide", 0.5);
			}
			else if (aPlayer.IsPlaying(anim))	//if strafed while already strafing
			{
				aPlayer.Stop(anim);
				
				aPlayer[anim].speed = 1.75f;
				aPlayer.CrossFade(anim,0.01f);
				
				bInStrafe = true;
			}
			else
			{
				aPlayer[anim].speed = 1.75f;
				aPlayer.CrossFade(anim,0.01f);
				
				bInStrafe = true;
			}
		}//end of if is MechAnim Enabled
		else
		{
			string anim = strafeDirection == StrafeDirection.Strafe_Right ? "StrafeRightAnim" : "StrafeLeftAnim";
			if (isInAir())
			{	
				aPlayerMecAnim.SetBool(anim, true);
			}
			else if (aPlayerMecAnim.GetBool("StrafeRightAnim") || aPlayerMecAnim.GetBool("StrafeLeftAnim"))	//if strafed while already strafing
			{	
				aPlayerMecAnim.SetBool(anim, true);			
				bInStrafe = true;
			}
			else
			{			
				aPlayerMecAnim.SetBool(anim, true);
				bInStrafe = true;
			}
		}
	}//end of strafe player function
	
	public float getCurrentMileage() { return fCurrentMileage; }
	public float getCurrentForwardSpeed() { return fCurrentForwardSpeed; }
	public int getCurrentLane() { return iLanePosition; }
	public float getCurrentPlayerRotation() { return tCurrentAngle; }
	public float getCurrentWalkSpeed() { return fCurrentWalkSpeed; }
	public bool isInAir()
	{
		if (bInAir || bJumpFlag || bInJump || bDiveFlag)
			return true;
		else
			return false;
	}
	
	public void setCurrentDistanceOnPath(float iValue) { fCurrentDistanceOnPath = iValue; }
	public void setPitFallLerpValue(float iValue) { fPitFallLerpValue = iValue; }
	public void setPitFallForwardSpeed(float iValue) { fPitFallForwardSpeed = iValue; }
	
	/*
	*	FUNCTION: Turn player animations On or Off
	*/
	public void togglePlayerAnimation(bool bValue) { aPlayer.enabled = bValue; }
	
	public bool isMechAnimEnabled() { return mecanimEnabled; }
}
