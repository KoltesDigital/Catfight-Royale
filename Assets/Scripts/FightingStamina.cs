using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
public class FightingStamina : MonoBehaviour
{

	public float Stamina = 100.0f;
	public float StaminaLossDuringFight = 20.0f;
	public float StaminaGainDuringStun = 20.0f;
	public float StaminaGainOutOfFight = 5.0f;
	public float FightDuration = 3.0f;
	public float rFightTimer = 0.0f;
	public float TimeToStartFightAnim = 0.0f;
	bool bAnimStarted = false;
	public int NumMash = 0;
	public List<Princess> Opponents = new List<Princess> ();

	public enum FightState
	{
		None,
		Fighting,
		Stun
	}

	public FightState State = FightState.None;
	private StaminaBar StaminaBar;

	// Use this for initialization
	void Start ()
	{
		StaminaBar = GetComponentInChildren<StaminaBar> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		switch (State) {
		case FightState.Fighting:
			{
				Princess thisPrincess = transform.GetComponent<Princess> ();
				rFightTimer += Time.deltaTime;

				if (bAnimStarted == false) {
					if (rFightTimer > TimeToStartFightAnim) {
						// TODO
						// thisPrincess.CourtisaneAnim.CrossFadeQueued( "attack", 0.3f, QueueMode.PlayNow );
						bAnimStarted = true;
					}
				}

				if (rFightTimer >= FightDuration) {
					int iMaxMashCount = NumMash;
					int iBestMasher = -1;
					int i = 0;
					
					foreach (Princess opponent in Opponents) {
						if (Stamina > 0.0f)
							opponent.FightStamina.Stamina = 0.0f;

						if (opponent.FightStamina.NumMash > iMaxMashCount) {
							iBestMasher = i;
							iMaxMashCount = opponent.FightStamina.NumMash;
						}
						
						i++;
					}

					if (iBestMasher < 0) {
						// we are the champions
						Stamina = 100.0f;

						//Princess winner = Opponents[iBestMasher];
						bool bNewOwner = false;
						
						foreach (Princess opponent in Opponents) {
							// give the item to the opponent
							if (opponent.GotItem) {
								// opponent.LostItem(Item);
								/*
								Debug.Log( "Give Item" );
								opponent.GotItem = false;
								thisPrincess.GotItem = true;
								opponent.Item.parent = thisPrincess.transform;
								thisPrincess.Item = opponent.Item;
								thisPrincess.ItemRef = opponent.ItemRef;
								
								if ( opponent.ItemRef.BoneName != "" )
									thisPrincess.Item.transform.parent = Extensions.Search( thisPrincess.transform, thisPrincess.ItemRef.BoneName );
								else
									thisPrincess.Item.transform.parent = transform;
								
								thisPrincess.Item.localPosition = thisPrincess.ItemRef.vPosOnPlayer;
								Vector3 vScale = thisPrincess.Item.localScale;
								vScale.x = Mathf.Abs( vScale.x );
								thisPrincess.Item.localScale = vScale;
								
								bNewOwner = true;
								
								opponent.Item = null;
								opponent.ItemRef = null;
								*
							}
							/*
							if ( opponent.GotItem2 )
							{
								Debug.Log( "Give Item 2" );
								opponent.GotItem2 = false;
								thisPrincess.GotItem2 = true;
								opponent.Item2.parent = thisPrincess.transform;
								thisPrincess.Item2 = opponent.Item;
								thisPrincess.ItemRef2 = opponent.ItemRef2;
								
								if ( thisPrincess.ItemRef2.BoneName != "" )
									thisPrincess.Item2.transform.parent = Extensions.Search( thisPrincess.transform, thisPrincess.ItemRef2.BoneName );
								else
									thisPrincess.Item2.transform.parent = transform;
								
								thisPrincess.Item2.localPosition = opponent.ItemRef2.vPosOnPlayer;
								Vector3 vScale = opponent.Item2.localScale;
								vScale.x = Mathf.Abs( vScale.x );
								thisPrincess.Item2.localScale = vScale;
								
								bNewOwner = true;
								
								opponent.Item2 = null;
								opponent.ItemRef2 = null;
							}
							*
							opponent.FightStamina.Stamina = 0.0f;
							opponent.FightStamina.State = FightState.Stun;
							/*
							opponent.CourtisaneAnim.CrossFadeQueued( "hit", 0.3f, QueueMode.PlayNow );
							opponent.CourtisaneAnim.CrossFadeQueued( "rest" );
							*
							opponent.FightStamina.Opponents.Clear ();
						}

						State = FightState.None;

						if (bNewOwner == false) {
							// TODO
							// thisPrincess.state = Princess.PlayerState.Moving;
							
							// thisPrincess.CourtisaneAnim.CrossFadeQueued( "idle", 0.3f, QueueMode.PlayNow );
							// thisPrincess.AnimMoving = false;
						} else {
							// TODO
							//	thisPrincess.GrabObject();
						}

						Opponents.Clear ();
						thisPrincess.Cloud.EndFight ();
					}
				}
			}
			break;

		case FightState.Stun:
			{
				Stamina += StaminaGainDuringStun * Time.deltaTime;
				StaminaBar.stamina = Stamina;

				if (Stamina >= 100.0f) {
					Stamina = 100.0f;
					State = FightState.None;

					Princess thisPrincess = transform.GetComponent<Princess> ();
					//thisPrincess.CourtisaneAnim.CrossFadeQueued( "stand", 0.3f, QueueMode.PlayNow );
					//thisPrincess.CourtisaneAnim.CrossFadeQueued( "idle" );
				}
			}
			break;

		case FightState.None:
			{
				bAnimStarted = false;
				StaminaBar.stamina = Stamina;
				Stamina += StaminaGainOutOfFight * Time.deltaTime;
				if (Stamina >= 100.0f) {
					Stamina = 100.0f;
				} else {
				}
			}
			break;
		}
	}

	// Start a fight (or reset timer)
	public void StartFight ()
	{
		// During a fight, stamina decrease at a constant rate
		State = FightState.Fighting;
		rFightTimer = 0.0f; //FightDuration;
		NumMash = 0;
		/*foreach ( Princess opponent in Opponents )
		{
			opponent.FightStamina.rFightTimer = 0.0f;
			opponent.FightStamina.NumMash = 0;
		}*
	}
}
*/