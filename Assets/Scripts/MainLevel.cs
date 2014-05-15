using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainLevel : MonoBehaviour
{
	private class Fight
	{
		public List<Princess> fighters;
		public float timer;

		private float duration;
		private MainLevel level;
		
		public Fight(float duration, MainLevel level) {
			timer = 0.0f;
			this.duration = duration;
			this.level = level;
		}

		public void RestartFight() {
			timer = 0.0f;
			foreach (Princess fighter in fighters ) {
				fighter.numMeleeMashes = 0;
			}
		}

		public void Update() {
			timer += Time.deltaTime;

			if ( timer >= duration ) {
				// at the end of the timer, the one with the most stamina left wins
				Princess winner = fighters.OrderByDescending(fighter => fighter.numMeleeMashes).First();

				// winner gets the loot
				foreach (Princess loser in fighters ) {
					if ( loser != winner ) {
						foreach (Item item in loser.GetGrabbedItems())
							level.GrabItem(winner, item);
						loser.ResetItems();

						loser.LoseFight();
					}
				}

				winner.stamina = Mathf.Min( winner.stamina, 0.5f );
				winner.WinFight(); 
				level.cloud.EndFight();
			}
		}

		public bool IsOver() {
			return ( timer >= duration );
		}

		public float CenterX()
		{
			return fighters.Select (fighter => fighter.transform.position.x).Sum () / fighters.Count;
		}
	}

	private enum State
	{
		Menu,
		King,
		Balloon,
		Search,
		Grabbing,
		PutObject,
		BringBack,
		Congratulations,
		End
	}

	private State currentState;
	private float currentCountdown;
	private Princess currentPlayer;

	public float announceDuration = 4.0f;
	public float announceFadeDuration = 0.5f;
	public float scoreDisplayDuration = 2.0f;

	public float grabbingDuration = 1.5f;
	public float putObjectDuration = 1.0f;
	public float congratulationsDuration = 4.0f;
	public float endDuration = 4.0f;

	// Item managment
	private List<Item> items = new List<Item> ();
	private List<Item> currentItems = new List<Item> ();

	// References
	private List<Princess> players = new List<Princess> ();
	public Transform queenThrone;
	public SpriteRenderer balloon;

	// Fight
	public Cloud cloud;
	public float fightStartDistance = 2.0f;
	public float fightPosOffset = 1.0f;
	public float fightDuration = 4.0f;
	private List<Fight> fights = new List<Fight>();

	// Timer management
	public float gameDuration = 300.0f;
	private float gameCountdown;

	// Music
	public AudioClip announceJingle;
	public AudioClip congratulationsJingle;
	public AudioClip searchMusic;
	public AudioClip fightMusic;
	public AudioClip getItemSfx;

	// King sounds
	public AudioClip[] kingCongratsSounds;
	public AudioClip[] kingAnnounceSounds;
	public AudioClip[] kingVictorySounds;
	private AudioSource kingAudio;

	// camera
	public MainCamera mainCamera;
	
	// Score Display
	public MainGUI gui;

	public GameObject menu;
	public GameObject playerPrefab;
	

	void Start ()
	{
		for (int i = 0; i < 4; ++i) {
			GameObject playerObject = Instantiate (playerPrefab) as GameObject;

			Princess player = playerObject.GetComponent<Princess>();

			player.LevelManager = this;
			player.SetIndex(i + 1);
			
			players.Add (player);
			
			gui.SetScore (i, 0);
		}
		
		mainCamera.SetPlayers(players);

		kingAudio = transform.FindChild( "King" ).audio;

		ShowMenu ();
	}

	void FixedUpdate ()
	{
		if (CanMove ()) {
			gameCountdown -= Time.deltaTime;

			if (gameCountdown <= 0) {
				gameCountdown = 0;
				EndGame();
			}

			gui.SetTimeLeft(gameCountdown);

			// update fights
			foreach (Fight fight in fights ) {
				fight.Update();
			}
			fights.RemoveAll( fight => fight.IsOver() );
		}
		
		currentCountdown -= Time.deltaTime;
		switch (currentState) {
		case State.Menu:
			if (Input.anyKey)
				StartGame ();
			break;

		case State.King:
			if (currentCountdown <= scoreDisplayDuration ) {
				gui.ShowScores (false);
			}

			if (currentCountdown <= 0) {
				currentState = State.Balloon;
				currentCountdown = announceDuration;
				mainCamera.SetBalloonMode ();
				PlayAnnounceSound();
			}
			break;

		case State.Balloon:
			Color color = balloon.color;
			color.a = Mathf.Min(Mathf.Min(1, (announceDuration - currentCountdown) / announceFadeDuration), currentCountdown / announceFadeDuration);
			balloon.color = color;

			if (currentCountdown <= 0) {
				currentState = State.Search;
				mainCamera.SetGameplayMode ();

				gui.ShowTime (true);

				audio.clip = searchMusic;
				audio.loop = true;
				audio.Play();
			}
			break;

		case State.Grabbing:
			if (currentCountdown <= 0) {
				currentPlayer.SetFree ();
				currentState = State.PutObject;

				if (audio.clip != fightMusic) {
					audio.clip = fightMusic;
					audio.loop = true;
					audio.Play ();
				}

				currentCountdown = putObjectDuration;
			}

			break;

		case State.PutObject:
			if (currentCountdown <= 0) {

				currentState = State.BringBack;
				
				mainCamera.SetGameplayMode ();
				gui.ShowTime (true);
			}
			
			break;

		case State.Congratulations:
			if (currentCountdown <= 0) {
				if (items.Count == 0) {
					EndGame ();
				} else {
					// launch next round 
					StartRound ();
					PlayCongratsSound();
				}
			}
			break;

		case State.End:
			if (currentCountdown <= 0) {
				ShowMenu();
				gui.ShowWinner(false);
			}
			break;
		}
	}

	public bool CanMove ()
	{
		return currentState == State.Search || currentState == State.BringBack;
	}

	public List<Item> GetCurrentItems ()
	{
		return currentItems.ToList ();
	}

	Item GetUnusedItem ()
	{
		if (items.Count == 0)
			return null;

		int index = Random.Range (0, items.Count);
		Item item = items [index];
		items.RemoveAt (index);

		return item;
	}
	
	void ShowMenu()
	{
		foreach (Princess player in players) {
			player.transform.position = new Vector3(0, 0, -100);
		}

		currentState = State.Menu;
		menu.SetActive (true);
		mainCamera.SetMenuMode();
		
		gui.ShowCredits (true);

		audio.clip = searchMusic;
		audio.loop = true;
		audio.Play ();
	}
	
	void StartGame ()
	{
		gui.ShowCredits (false);

		items.Clear ();
		foreach (GameObject itemObject in GameObject.FindGameObjectsWithTag("Item")) {
			Item item = itemObject.GetComponent<Item> ();
			item.ResetPosition();
			items.Add (item);
		}

		List<GameObject> startPoints = GameObject.FindGameObjectsWithTag ("Start point").ToList();
		
		for (int i = 0; i < 4; ++i) {
			int startIndex = Random.Range (0, startPoints.Count);
			Transform startPoint = startPoints [startIndex].transform;
			startPoints.RemoveAt (startIndex);

			players[i].transform.position = startPoint.position;
			players[i].score = 0;
			players[i].ResetItems();

			gui.SetScore (i, 0);
		}

		gameCountdown = gameDuration;

		StartRound ();
	}
	
	void StartRound ()
	{
		menu.SetActive (false);
		gui.ShowScores (true);
		
		currentState = State.King;
		currentCountdown = announceDuration;
		
		mainCamera.SetKingMode ();

		audio.clip = announceJingle;
		audio.loop = false;
		audio.Play ();

		currentItems.Clear ();
		Item item = GetUnusedItem ();
		currentItems.Add (item);

		Color color = balloon.color;
		color.a = 0.0f;
		balloon.color = color;

		balloon.sprite = item.balloon;

		foreach (Princess player in players) {
			player.stamina = 1;
			player.SetFree();
		}
	}
	
	void EndGame ()
	{
		currentState = State.End;
		currentCountdown = endDuration;

		Princess winner = players.OrderByDescending (player => player.score).First ();
		winner.WinGame ();
		mainCamera.SetWinnerMode (winner.gameObject);

		gui.ShowScores (false);
		gui.ShowWinner (true, winner);

		PlayVictorySound();
	}

	public void GrabItem (Princess player, Item item)
	{
		if ( FightingMode() == false ) {
			audio.Stop();
		}

		currentItems.Remove (item);

		currentState = State.Grabbing;
		currentCountdown = grabbingDuration;
		currentPlayer = player;

		player.SetGrabbing (item);
		mainCamera.SetPlayerMode (player.gameObject, true);
		gui.ShowTime (false);
	}

	// The Item was brought back ! End of the round
	public void BringBackItems (Princess player, List<Item> playerItems)
	{
		// score = items²
		player.score += playerItems.Count * playerItems.Count;
		gui.SetScore (player.index - 1, player.score);

		foreach (Princess everyPlayer in players) {
			everyPlayer.ResetItems ();

			if (everyPlayer == player)
				everyPlayer.SetSuccess ();
			else
				everyPlayer.SetFail ();

		}

		mainCamera.SetPlayerMode (player.gameObject);
		gui.ShowScores (true);
		gui.ShowTime (false);

		currentState = State.Congratulations;
		currentCountdown = congratulationsDuration;
		
		audio.clip = congratulationsJingle;
		audio.loop = false;
		audio.Play ();
	}
	
	public Princess GetBestOpponent(Princess player)
	{
		float maxScore = 0f;
		Princess bestOpponent = null;
		
		foreach (Princess otherPlayer in players) {
			if (otherPlayer != player) {
				float score = otherPlayer.GetMatchScore();
				if (score > maxScore) {
					// Debug.Log (score.ToString() + " > " + maxScore.ToString());
					Vector3 opponentOffset = player.transform.position - otherPlayer.transform.position;
					opponentOffset.z = 0f;
					if (opponentOffset.magnitude < fightStartDistance) {
						maxScore = score;
						bestOpponent = otherPlayer;
					}
				}
			}
		}
		
		return bestOpponent;
	}

	// Return number of opponents near the princess
	public bool IsMeleeAvailable(Princess player, ref List<Princess> opponents)
	{
		foreach (Princess otherPlayer in players) {
			if ( (otherPlayer != player) && (otherPlayer.IsResting() == false) ) {
				Vector3 opponentOffset = player.transform.position - otherPlayer.transform.position; 
				opponentOffset.z = 0f;
				if ( opponentOffset.magnitude < fightStartDistance ) 
					opponents.Add( otherPlayer );
			}
		}
		
		return ( opponents.Count > 1 );
	}

	// Hit neighbor princesses
	public bool HitOpponents(Princess player, Collider2D hitbox)
	{
		int numHits = 0;
		
		foreach (Princess otherPlayer in players) {
			if (otherPlayer != player) {
				if ( ( otherPlayer.IsResting() == false ) && ( hitbox.OverlapPoint( otherPlayer.transform.position ) ) ) {
				    numHits++;
					otherPlayer.TakeHit( player );
				}
			}
		}
		
		return ( numHits > 0 );
	}
	
	private Princess GetBestOpponentInFight(Fight fight)
	{
		float maxScore = 0f;
		Princess bestOpponent = null;
		
		foreach (Princess otherPlayer in fight.fighters) {
			float score = otherPlayer.GetMatchScore();
			if (score > maxScore) {
				maxScore = score;
				bestOpponent = otherPlayer;
			}
		}
		
		return bestOpponent;
	}

	// Add b to fight is a is on this fight
	private bool AddFighterToFight(Fight fight, Princess a, Princess b)
	{
		if (fight.fighters.Any(fighter => fighter == a)) {
			// a is already in fight, adding b
			b.opponent = GetBestOpponentInFight(fight);
			fight.fighters.Add (b);
			b.StartFight(b.transform.position.x > fight.CenterX());
			return true;
		}
		return false;
	}

	public void StartFight(Princess a, Princess b)
	{
		foreach (Fight fight in fights) {
			if (AddFighterToFight(fight, a, b) || AddFighterToFight(fight, b, a)) {
				fight.RestartFight();
				cloud.BeginFight(fight.fighters);
				return;
			}
		}

		// no existing fight with a and b, creating a new one
		Fight newFight = new Fight(fightDuration, this);
		newFight.fighters = new List<Princess> ();
		newFight.fighters.Add(a);
		newFight.fighters.Add(b);
		fights.Add (newFight);

		b.opponent = a;

		Vector3 centerPosition = Vector3.Lerp (a.transform.position, b.transform.position, 0.5f);
		Vector3 centerOffset = new Vector3 (fightPosOffset, 0.0f, 0.0f);

		bool aOnRight = (a.transform.position.x > b.transform.position.x);
		if (aOnRight)
			centerOffset *= -1.0f;
		
		a.transform.position = centerPosition - centerOffset;
		b.transform.position = centerPosition + centerOffset;

		a.StartFight (aOnRight);
		b.StartFight (!aOnRight);

		cloud.BeginFight(newFight.fighters);
	}

	public void PlayCongratsSound() {
		kingAudio.clip = kingCongratsSounds[ Random.Range( 0, kingCongratsSounds.Length ) ];
		kingAudio.Play();
	}

	public void PlayAnnounceSound() {
		kingAudio.clip = kingAnnounceSounds[ Random.Range( 0, kingAnnounceSounds.Length ) ];
		kingAudio.Play();
	}

	public void PlayVictorySound() {
		kingAudio.clip = kingVictorySounds[ Random.Range( 0, kingVictorySounds.Length ) ];
		kingAudio.Play();
	}

	public bool FightingMode() { 
		return ( currentState == State.BringBack );
	}

	public bool GrabbingMode() { 
		return ( currentState == State.Grabbing );
	}

/*	public void LoseFight(Princess player)
	{
		foreach (Fight fight in fights.ToList()) {
			if (fight.fighters.Remove(player)) {
				// other players are free
				foreach (Princess fighter in fight.fighters)
					fighter.SetFree();

				// winner gets loots
				Princess winner = fight.fighters.OrderByDescending(fighter => fighter.stamina).First();
				foreach (Item item in player.GetGrabbedItems())
					GrabItem(winner, item);
				player.ResetItems();

				fights.Remove(fight);
				cloud.EndFight();
			}
		}
	}*/
}
