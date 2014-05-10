using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour
{
	public float period;
	private float timer;

	void Start ()
	{
		timer = period;
	}

	void Update ()
	{
		timer -= Time.deltaTime;
		if (timer <= 0) {
			timer += period;
			renderer.enabled = !renderer.enabled;
		}
	}
}
