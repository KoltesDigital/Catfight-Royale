using UnityEngine;
using System.Collections;

public class MainGUI : MonoBehaviour
{
	public Transform scores;
	public GUIText[] scoreTexts;
	public GUIText timeText;
	public GUIText winnerText;
	public Transform creditsContainer;
	public GameObject creditsTextPrefab;
	private GUIText[] creditsTexts;
	private int mainCreditTextIndex = 1;

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
	
	public float showedCreditsY = 0.8f;
	public float hiddenCreditsY = 1.2f;
	private float creditsY;
	public string[] credits;
	private int creditsIndex = -1;
	private float creditsCountdown = 0;
	public float creditsPeriod = 1f;
	public float creditsTransition = 0.3f;
	public float creditsOffset = 0.05f;

	void Start ()
	{
		timeY = hiddenTimeY;
		scoresY = hiddenScoresY;
		winnerY = hiddenWinnerY;
		creditsY = showedCreditsY;

		creditsTexts = new GUIText[2];
		for (int i = 0; i < 2; ++i) {
			GameObject creditsTextObject = Instantiate (creditsTextPrefab) as GameObject;
			creditsTextObject.transform.parent = creditsContainer;

			GUIText creditsText = creditsTextObject.GetComponentInChildren<GUIText>();
			creditsTexts[i] = creditsText;
		}
	}

	void Update ()
	{
		float ratio = 1 - Mathf.Exp (- reactivity * Time.deltaTime);

		Vector3 position;
		Color color;
		
		position = scores.localPosition;
		position.y += (scoresY - position.y) * ratio;
		scores.localPosition = position;
		
		position = timeText.transform.localPosition;
		position.y += (timeY - position.y) * ratio;
		timeText.transform.localPosition = position;
		
		position = winnerText.transform.localPosition;
		position.y += (winnerY - position.y) * ratio;
		winnerText.transform.localPosition = position;
		
		position = creditsContainer.localPosition;
		position.y += (creditsY - position.y) * ratio;
		creditsContainer.localPosition = position;

		float transition = Mathf.SmoothStep(0f, 1f, (creditsPeriod - creditsCountdown) / creditsTransition);

		position = creditsTexts [mainCreditTextIndex].transform.localPosition;
		position.y = - (1 - transition) * creditsOffset;
		creditsTexts [mainCreditTextIndex].transform.localPosition = position;

		color = creditsTexts [mainCreditTextIndex].color;
		color.a = transition;
		creditsTexts [mainCreditTextIndex].color = color;
		
		position = creditsTexts [1 - mainCreditTextIndex].transform.localPosition;
		position.y = transition * creditsOffset;
		creditsTexts [1 - mainCreditTextIndex].transform.localPosition = position;

		color = creditsTexts [1 - mainCreditTextIndex].color;
		color.a = 1f - transition;
		creditsTexts [1 - mainCreditTextIndex].color = color;

		creditsCountdown -= Time.deltaTime;
		if (creditsCountdown <= 0) {
			creditsCountdown = creditsPeriod;
			++creditsIndex;
			if (creditsIndex >= credits.Length)
				creditsIndex = 0;

			mainCreditTextIndex = 1 - mainCreditTextIndex;
			creditsTexts[mainCreditTextIndex].text = credits[creditsIndex];
		}
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
	
	public void ShowCredits (bool show)
	{
		creditsY = show ? showedCreditsY : hiddenCreditsY;
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
