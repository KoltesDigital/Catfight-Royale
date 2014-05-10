using UnityEngine;
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
		Fighting,
		Resting
	}

	private PlayerState state = PlayerState.Free;

	// Move Parameters
	public float MoveHorizontalSpeed = 0.65f;
	public float MoveVerticalSpeed = 0.25f;

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
	public float
		stamina = 1.0f;
	public float staminaLoss = 0.05f;
	public float staminaRegen = 0.1f;
	private StaminaBar staminaBar;
	[HideInInspector]
	public Princess opponent;

	// Use this for initialization
	void Start ()
	{
		playerAnimation = GetComponentInChildren<Animation> ();
		attackAnimation = playerAnimation ["attack"];
		staminaBar = GetComponentInChildren<StaminaBar> ();

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
		playerAnimation.CrossFade ("take_loop", 0.3f);
		
		grabbingItem = item;
		
		item.transform.parent = Extensions.Search (transform, "Grab");
		
		item.transform.localPosition = item.grabbingPosition;
		
		Vector3 rotation = item.transform.localEulerAngles;
		rotation.z = item.grabbingRotation;
		item.transform.localEulerAngles = rotation;
		
		Vector3 scale = item.transform.localScale;
		scale.x = Mathf.Abs (scale.x);
		item.transform.localScale = scale;
	}
	
	public void SetSuccess ()
	{
		playerAnimation.CrossFade ("success", 0.3f);
	}
	
	public void SetFail ()
	{
		playerAnimation.CrossFade ("fail", 0.3f);
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

			UpdateFight ();
		}

		staminaBar.SetStamina (stamina);
	}
	
	void UpdateMotion ()
	{
		// Update motion velocity
		float xSpeed = Input.GetAxis ("L_XAxis_" + index.ToString ()) * MoveHorizontalSpeed;
		float ySpeed = Input.GetAxis ("L_YAxis_" + index.ToString ()) * MoveVerticalSpeed;

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
			itemOffset.z = 0.0f;

			if (itemOffset.magnitude < GrabItemDistance) {
				LevelManager.GrabItem (this, item);
			}
		}

		if (grabbedItems.Count > 0) {
			// Did we get to the throne ?
			Vector3 thronePosition = LevelManager.queenThrone.transform.position;
			Vector3 throneOffset = transform.position - thronePosition;
			throneOffset.z = 0.0f;
						
			if (throneOffset.magnitude < BringBackItemDistance) {
				LevelManager.BringBackItems (this, grabbedItems);
							
				transform.position = thronePosition;

				SetFree ();
			}
		}
	}

	// Fighting management
	public void UpdateFight ()
	{
		attackAnimation.speed -= Time.deltaTime * attackAnimationSpeedDecreaseRate;
		if (attackAnimation.speed < attackAnimationSpeedMin)
			attackAnimation.speed = attackAnimationSpeedMin;

		if (state != PlayerState.Fighting) {
			stamina += staminaRegen * Time.deltaTime;
			if (stamina >= 1f) {
				stamina = 1f;
				if (state != PlayerState.Free) {
					state = PlayerState.Free;
					playerAnimation.CrossFade ("stand", 0.3f);
					playerAnimation.CrossFadeQueued ("idle");
					isRunning = false;
				}
			}
		}

		if (GetAttackInput ()) {
			if (state == PlayerState.Free) {
				opponent = LevelManager.GetBestOpponent (this);
				if (opponent) {
					LevelManager.StartFight (this, opponent);
				}
			}
			
			if (state == PlayerState.Fighting) {
				attackAnimation.speed += attackAnimationSpeedIncrease;
				if (attackAnimation.speed > attackAnimationSpeedMax)
					attackAnimation.speed = attackAnimationSpeedMax;

				opponent.stamina -= staminaLoss;
			}
		}

		if (state == PlayerState.Fighting && stamina <= 0f) {
			stamina = 0f;
			state = PlayerState.Resting;
			playerAnimation.CrossFade ("hit", 0.3f);
			playerAnimation.PlayQueued ("rest");

			Vector3 scale = playerAnimation.transform.localScale;
			scale.x = -scale.x;
			playerAnimation.transform.localScale = scale;

			LevelManager.LoseFight(this);
		}
	}

	public bool IsResting()
	{
		return state == PlayerState.Resting;
	}

	// Detect AttackInput
	bool GetAttackInput ()
	{
		return Input.GetButtonDown ("A_1");
		//return Input.GetButtonDown ("A_" + index.ToString ());
	}

	// Start a fight
	public void StartFight (bool onRight)
	{
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
}
