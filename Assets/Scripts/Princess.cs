﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Princess : MonoBehaviour
{
	// References
	public MainLevel LevelManager;

	// Player number
	public int index = 0;
	public int score = 0;

	// Player state
	private enum PlayerState
	{
		Free,
		Punching,
		Fighting,
		Resting,
		Blocked
	}

	private PlayerState state = PlayerState.Free;

	// Move Parameters
	public float MoveHorizontalSpeed = 0.65f;
	public float MoveVerticalSpeed = 0.25f;
	public float MoveSpeedFactorSearch = 0.75f;
	public float MoveSpeedFactorItem = 0.5f;

	// Grab Parameters
	public float GrabItemDistance = 1.0f;

	// Bring back parameters
	public float BringBackItemDistance = 1.0f;

	// mesh animation
	private bool isRunning = false;
	private Animation playerAnimation;
	private AnimationState attackAnimation;
	public float attackAnimationSpeedMin = 0.5f;
	public float attackAnimationSpeedMax = 2.5f;
	public float attackAnimationSpeedIncrease = 0.5f;
	public float attackAnimationSpeedDecreaseRate = 1.0f;
	private Item grabbingItem;
	private List<Item> grabbedItems = new List<Item> ();

	// Fight
	[HideInInspector]
	public float stamina = 1.0f;
	public float staminaPunchLoss = 0.2f;
	public int numMeleeMashes = 0;
	public float staminaLoss = 0.05f;
	public float staminaRegen = 0.1f;
	public float staminaMashRegen = 0.15f;
	private StaminaBar staminaBar;
	[HideInInspector]
	public Princess opponent;
	private PunchHitbox punchHitbox;

	private float attackButtonCooldown = 0.0f;

	// sound
	public AudioClip[] attackSounds;
	public AudioClip[] attackSoundsHysteria;
	public float attackPitchMin = 0.8f;
	public float attackPitchMax = 1.3f;

	public AudioClip[] koSounds;
	public AudioClip[] koSoundsHysteria;

	public AudioClip[] grabItemSounds;
	public AudioClip[] grabItemSoundsHysteria;

	public AudioClip[] winnerSounds;

	public float fightSoundDelayMin = 0.3f;
	public float fightSoundDelayMax = 1.0f;
	private float fightSoundCountdown = 0.0f;

	// Use this for initialization
	void Start ()
	{
		playerAnimation = GetComponentInChildren<Animation> ();
		attackAnimation = playerAnimation ["attack"];
		staminaBar = GetComponentInChildren<StaminaBar> ();
		punchHitbox = GetComponentInChildren<PunchHitbox>();

		SetFree ();
	}

	public void SetIndex (int i)
	{
		index = i;

		foreach (Transform child in GetComponentsInChildren<Transform>()) {
			if (child.tag == "Colored material") {
				int underscore = child.name.IndexOf ("_");
				string baseName = (underscore < 0) ? child.name : child.name.Substring (0, underscore);
				child.renderer.material = Resources.Load (baseName + "_" + index.ToString (), typeof(Material)) as Material;
			}
		}
	}

	public void ResetItems ()
	{
		grabbedItems.Clear ();
	}

	public void SetFree ()
	{
		state = PlayerState.Free;
		playerAnimation.CrossFade ("idle", 0.3f);
		isRunning = false;

		if (grabbingItem) {
			if (grabbingItem.grabbedBoneName != "")
				grabbingItem.transform.parent = Extensions.Search (transform, grabbingItem.grabbedBoneName);
			else
				grabbingItem.transform.parent = transform;
			
			grabbingItem.transform.localPosition = grabbingItem.grabbedPosition;
			
			Vector3 rotation = grabbingItem.transform.localEulerAngles;
			rotation.z = grabbingItem.grabbedRotation;
			grabbingItem.transform.localEulerAngles = rotation;

			grabbedItems.Add (grabbingItem);
			grabbingItem = null;
		}
	}

	public void SetGrabbing (Item item)
	{
		playerAnimation.CrossFade ("take", 0.3f);
		
		grabbingItem = item;
		
		item.transform.parent = Extensions.Search (transform, "Grab");
		
		item.transform.localPosition = item.grabbingPosition;
		
		Vector3 rotation = item.transform.localEulerAngles;
		rotation.z = item.grabbingRotation;
		item.transform.localEulerAngles = rotation;
		
		Vector3 scale = item.transform.localScale;
		scale.x = Mathf.Abs (scale.x);
		item.transform.localScale = scale;

		PlayGrabItemSound();
	}
	
	public void SetSuccess ()
	{
		playerAnimation.CrossFade ("success", 0.3f);
		state = PlayerState.Blocked;
	}
	
	public void SetFail ()
	{
		playerAnimation.CrossFade ("fail", 0.3f);
		state = PlayerState.Blocked;
	}

	public void LostItem (Item item)
	{
		grabbedItems.Remove (item);
	}

	void FixedUpdate ()
	{
		if (LevelManager.CanMove ()) {
			if (state == PlayerState.Free)
				UpdateMotion ();

			if ( state == PlayerState.Punching )
				UpdatePunch();

			UpdateFight ();
		}

		staminaBar.SetStamina (stamina);
	}
	
	void UpdateMotion ()
	{
		// Update motion velocity
		float xSpeed = Input.GetAxis ("XAxis_" + index.ToString ()) * MoveHorizontalSpeed;
		float ySpeed = Input.GetAxis ("YAxis_" + index.ToString ()) * MoveVerticalSpeed;

		// add speed multipliers depending on situation
		if ( LevelManager.FightingMode() == false ) {
			// Search mode
			xSpeed *= MoveSpeedFactorSearch;
			ySpeed *= MoveSpeedFactorSearch;
		} else if (grabbedItems.Count > 0) {
			// item mode
			xSpeed *= MoveSpeedFactorItem;
			ySpeed *= MoveSpeedFactorItem;
		}

		// recompute depth based on height
		float depth = transform.position.y;

		transform.position = new Vector3 (transform.position.x + xSpeed, transform.position.y + ySpeed, depth);

		// select animation
		if ((xSpeed != 0.0f) || (ySpeed != 0.0f)) {
			if (!isRunning) {
				playerAnimation.CrossFade ("run", 0.3f);
				isRunning = true;
			}

			// set direction
			Vector3 scale = playerAnimation.transform.localScale;
			if (xSpeed < 0.0f)
				scale.x = -Mathf.Abs (scale.x);
			else if (xSpeed > 0.0f)
				scale.x = Mathf.Abs (scale.x);
			playerAnimation.transform.localScale = scale;
		} else {
			if (isRunning) {
				playerAnimation.CrossFade ("idle", 0.3f);
				isRunning = false;
			}
		}
				
		foreach (Item item in LevelManager.GetCurrentItems()) {
			Vector3 itemOffset = transform.position - item.transform.position;
			itemOffset.y = 0.0f;

			if (itemOffset.magnitude < GrabItemDistance) {
				LevelManager.GrabItem (this, item);
			}
		}

		if (grabbedItems.Count > 0) {
			// Did we get to the throne ?
			Vector3 thronePosition = LevelManager.queenThrone.transform.position;
			Vector3 throneOffset = transform.position - thronePosition;
			throneOffset.y = 0.0f;
						
			if (throneOffset.magnitude < BringBackItemDistance) {
				LevelManager.BringBackItems (this, grabbedItems);
							
				//transform.position = thronePosition;
				PlayWinnerSound();
			}
		}
	}

	// update punch
	public void UpdatePunch()
	{
		if ( playerAnimation.isPlaying == false )
			SetFree();
	}

	// Fighting management
	public void UpdateFight ()
	{
		/*attackAnimation.speed -= Time.deltaTime * attackAnimationSpeedDecreaseRate;
		if (attackAnimation.speed < attackAnimationSpeedMin)
			attackAnimation.speed = attackAnimationSpeedMin;*/

		if (state != PlayerState.Fighting) {
			if (state == PlayerState.Resting) {
				stamina += staminaRegen * Time.deltaTime;
				if (stamina >= 1f) {
					stamina = 1f;
					if (state == PlayerState.Resting) {
						state = PlayerState.Free;
						playerAnimation.CrossFade ("stand", 0.3f);
						playerAnimation.CrossFadeQueued ("idle");
						isRunning = false;
					}
				}
			}
		} else {
			// hide princess during fight
			Vector3 hiddenPos = transform.position;
			hiddenPos.z = -1000f;
			transform.position = hiddenPos;

			// play a random sound between shout and clap
			fightSoundCountdown -= Time.deltaTime;
			if ( fightSoundCountdown < 0.0f ) {
				if ( Random.value > 0.5f )
					PlayAttackSound();
				else
					punchHitbox.PlayClapSound();

				fightSoundCountdown = Random.Range( fightSoundDelayMin, fightSoundDelayMax );
			}
		}

		if (GetAttackInput ()) {
			if ( (state == PlayerState.Free) || 
			    ( (state == PlayerState.Punching ) && ( punchHitbox.IsPunchOver() ) ) ) {
				// check if we have only 0-1 opponent around
				List<Princess> opponents = new List<Princess>();
				bool startMelee = LevelManager.IsMeleeAvailable(this, ref opponents);
				if ( startMelee == false ) {
					// only throw a punch
					Punch();
				} else {
					// we have enough opponents to start a melee !
					foreach ( Princess opponent in opponents ) {
						LevelManager.StartFight(this, opponent);
					}
				}
			} else if ( state == PlayerState.Resting ) {
				stamina += staminaMashRegen;
			}
			
			if (state == PlayerState.Fighting) {
				// resist stamina loss using mash
				//stamina += staminaMashResist;
				numMeleeMashes++;
			}
		}

		/*if (stamina <= 0f) {
			stamina = 0f;
			state = PlayerState.Resting;
			playerAnimation.CrossFade ("hit", 0.3f);
			playerAnimation.PlayQueued ("rest");

			Vector3 scale = playerAnimation.transform.localScale;
			scale.x = -scale.x;
			playerAnimation.transform.localScale = scale;

			// un-hide the character
			Vector3 unhiddenPos = transform.position;
			unhiddenPos.z = transform.position.y;
			transform.position = unhiddenPos;

			PlayerKOSound();

			//LevelManager.LoseFight(this);
		}*/
	}

	// melee victory
	public void WinFight() 
	{
		// un-hide the character
		Vector3 unhiddenPos = transform.position;
		unhiddenPos.z = transform.position.y;
		transform.position = unhiddenPos;

		// only get free in case we are not playing grab animatio
		if (LevelManager.GrabbingMode() == false )
			SetFree();

		// todo : play some sound/animation ?
	}
	
	// melee defeat
	public void LoseFight() 
	{
		stamina = 0f;
		state = PlayerState.Resting;
		playerAnimation.CrossFade ("hit", 0.3f);
		playerAnimation.PlayQueued ("rest");
		
		Vector3 scale = playerAnimation.transform.localScale;
		scale.x = -scale.x;
		playerAnimation.transform.localScale = scale;
		
		// un-hide the character
		Vector3 unhiddenPos = transform.position;
		unhiddenPos.z = transform.position.y;
		transform.position = unhiddenPos;
		
		PlayerKOSound();
	}

	public void WinGame() 
	{
		playerAnimation.CrossFade ("victory", 0.3f);
	}

	public bool IsResting()
	{
		return state == PlayerState.Resting;
	}

	// Detect AttackInput
	bool GetAttackInput ()
	{
		// TODO : SHOULD BE REMOVED...but there's a weird bug where the input is still active for a few frames ?
		attackButtonCooldown -= Time.deltaTime;
		if ( attackButtonCooldown <= 0.0f ) {
			bool bAttackButton = Input.GetButtonDown ("A_" + index.ToString ());

			if ( bAttackButton ) {
				attackButtonCooldown = 0.05f;
			}

			return bAttackButton;
		}

		return false;
	}

	// Start a fight
	public void StartFight (bool onRight)
	{
	//	return;
		Vector3 vScale = playerAnimation.transform.localScale;
		if (onRight) {
			vScale.x = -Mathf.Abs (vScale.x);
		} else {
			vScale.x = Mathf.Abs (vScale.x);
		}
		playerAnimation.transform.localScale = vScale;

		state = PlayerState.Fighting;
		playerAnimation.CrossFade ("attack", 0.3f);
		attackAnimation.speed = attackAnimationSpeedMin;

		fightSoundCountdown = Random.Range( 0.0f, fightSoundDelayMax );

		numMeleeMashes = 0;
	}

	// throw a punch
	public void Punch()
	{
		playerAnimation.CrossFade("attack", 0.3f);
		state = PlayerState.Punching;
		attackAnimation.speed = attackAnimationSpeedMin;
		punchHitbox.StartPunch();
		PlayAttackSound();
	}

	// receive a hit
	public void TakeHit(Princess puncher) {
		stamina -= staminaPunchLoss;

		// if we get killed, give all items to the puncher
		if ( stamina <= 0.0f ) {
			foreach (Item item in GetGrabbedItems())
				LevelManager.GrabItem(puncher, item);
			ResetItems();

			LoseFight();
		}
	}
		
	public float GetMatchScore ()
	{
		if (state == PlayerState.Resting)
			return -1f;

		return stamina + grabbedItems.Count;
	}

	public List<Item> GetGrabbedItems()
	{
		return grabbedItems;
	}

	public void PlayAttackSound() {
		if ( LevelManager.FightingMode() == false ) {
			audio.clip = attackSounds[ Random.Range( 0, attackSounds.Length ) ];
			audio.pitch = Random.Range( attackPitchMin, attackPitchMax );
		} else {
			audio.clip = attackSoundsHysteria[ Random.Range( 0, attackSoundsHysteria.Length ) ];
		}

		audio.Play();
	}

	public void PlayerKOSound() {
		if ( LevelManager.FightingMode() == false ) {
			audio.clip = koSounds[ Random.Range( 0, koSounds.Length ) ];
		} else {
			audio.clip = koSoundsHysteria[ Random.Range( 0, koSoundsHysteria.Length ) ];
		}

		audio.Play();
	}

	public void PlayGrabItemSound() {
		if ( LevelManager.FightingMode() == false ) {
			audio.clip = grabItemSounds[ Random.Range( 0, grabItemSounds.Length ) ];
		} else {
			audio.clip = grabItemSoundsHysteria[ Random.Range( 0, grabItemSoundsHysteria.Length ) ];
		}

		audio.Play();
	}

	public void PlayWinnerSound() {
		audio.clip = winnerSounds[ Random.Range( 0, winnerSounds.Length ) ];
		audio.Play();
	}
}
