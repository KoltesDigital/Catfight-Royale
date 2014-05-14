using UnityEngine;
using System.Collections;

public class MainGUI : MonoBehaviour
{
	public Transform scores;
	public GUIText[] scoreTexts;
	public GUIText timeText;
	public GUIText winnerText;

	public float reactivity = 1f;

	public float showedTimeY = 1f;
	public float hiddenTimeY = 1.2f;
	private float timeY;

	public float showedScoresY = 0.1f;
	public float hiddenScoresY = -0.2f;
	private float scoresY;

	public float showedWinnerY = 0.3f;
	public float hiddenWinnerY = -0.2f;
	private float winnerY;

	void Start ()
	{
		timeY = hiddenTimeY;
		scoresY = hiddenScoresY;
		winnerY = hiddenWinnerY;
	}

	void Update ()
	{
		float ratio = 1 - Mathf.Exp (- reactivity * Time.deltaTime);

		Vector3 position;
		
		position = scores.localPosition;
		position.y += (scoresY - position.y) * ratio;
		scores.localPosition = position;
		
		position = timeText.transform.localPosition;
		position.y += (timeY - position.y) * ratio;
		timeText.transform.localPosition = position;
		
		position = winnerText.transform.localPosition;
		position.y += (winnerY - position.y) * ratio;
		winnerText.transform.localPosition = position;
	}

	public void ShowTime (bool show)
	{
		timeY = show ? showedTimeY : hiddenTimeY;
	}

	public void ShowScores (bool show)
	{
		scoresY = show ? showedScoresY : hiddenScoresY;
	}

	public void ShowWinner (bool show, Princess winner = null)
	{
		winnerY = show ? showedWinnerY : hiddenWinnerY;
		if (winner)
			winnerText.text = "Player " + winner.index.ToString () + " wins!";
	}

	public void SetTimeLeft (float timeLeft)
	{
		int minutes = Mathf.FloorToInt (timeLeft / 60.0f);
		int seconds = Mathf.FloorToInt (timeLeft - minutes * 60);
		
		string niceTime = string.Format ("{0:00}:{1:00}", minutes, seconds);
		timeText.text = niceTime;
	}

	public void SetScore(int index, int score)
	{
		scoreTexts [index].text = score.ToString ();
	}
}
