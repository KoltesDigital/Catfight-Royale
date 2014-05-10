using UnityEngine;
using System.Collections;

public class StaminaBar : MonoBehaviour
{
	public float fadeDuration = 0.3f;

	private float opacity = 0.0f;
	
	private float stamina = 1.0f;
	private SpriteRenderer bar;
	private SpriteRenderer level;

	public void SetStamina(float s)
	{
		stamina = Mathf.Max (s, 0f);
	}

	void Start ()
	{
		bar = GetComponent<SpriteRenderer> ();
		level = transform.GetChild(0).GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		level.transform.localScale = new Vector3 (stamina, 1.0f, 1.0f);

		if (stamina > 0.999f)
			opacity = Mathf.Max (opacity - Time.deltaTime / fadeDuration, 0f);
		else
			opacity = Mathf.Min (opacity + Time.deltaTime / fadeDuration, 1f);
		
		Color color = bar.color;
		color.a = opacity;
		bar.color = color;
		
		color = level.color;
		color.a = opacity;
		level.color = color;
	}
}
