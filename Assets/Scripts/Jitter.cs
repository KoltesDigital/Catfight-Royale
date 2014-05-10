using UnityEngine;
using System.Collections;

// Shake your body
public class Jitter : MonoBehaviour {

	private float dx, dy;
	private Vector2 position;

	// Use this for initialization
	public void Start () {
		position = Random.insideUnitCircle * 15.0f;

		float angle = Random.Range (0, 2 * Mathf.PI);
		dx = Mathf.Cos (angle);
		dy = Mathf.Sin (angle);
	}
	
	// Update is called once per frame
	void Update () {
		float t = Time.time;
		float radius = Mathf.Sin (t * 600);
		transform.localPosition = new Vector3 (position.x + dx * radius, position.y + dy * radius, -0.5f);
	}
}
