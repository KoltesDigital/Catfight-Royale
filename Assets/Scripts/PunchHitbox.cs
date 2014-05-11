using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PunchHitbox : MonoBehaviour {

	private bool boxActive = false;
	private float timer = -1.0f;
	private Princess thisPrincess;

	public float activeTimer = 0.3f;
	public float deactiveTimer = 1.0f;

	// Use this for initialization
	void Start () {
		thisPrincess = transform.parent.parent.GetComponent<Princess>();
	}
	
	// Update is called once per frame
	void Update () {
		if ( timer >= 0.0f ) {
			timer += Time.deltaTime;

			boxActive = ( ( timer >= activeTimer ) && ( timer < deactiveTimer ) );

			if ( boxActive ) {
				bool hitOpponents = thisPrincess.LevelManager.HitOpponents( thisPrincess, collider2D );

				if ( hitOpponents ) {
					boxActive = false;
					timer = -1.0f;
				}
			}
		}
	}
	
	public void StartPunch() {
		timer = 0.0f;
		boxActive = false;
	}

	public bool IsPunchOver() {
		return ( ( timer < 0.0f ) || ( timer > deactiveTimer ) );
	}
}
