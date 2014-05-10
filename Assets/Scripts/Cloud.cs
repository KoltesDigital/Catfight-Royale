using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Cloud : MonoBehaviour
{
	private List<Transform> clouds = new List<Transform> ();
	private List<Transform> parts = new List<Transform> ();
	private List<GameObject> vfxs = new List<GameObject> ();
	private int nextVfx = 0;
	private float nextVfxTime;
	private float clamp = 0, clampSpeed = 0;
	
	public float cloudOffsetY = 2.0f;
	public float cloudAngularSpeed = 400.0f;
	public float cloudPulsatance = 1.0f;
	public float cloudRadiusAmplitude = 2.0f;
	public float cloudRadiusMean = 5.0f;
	public float cloudRadiusPulsatance = 6.0f;
	public float partRadiusAmplitude = 7.0f;
	public float partRadiusBase = 3.0f;
	public float partRadiusPulsatance = 1.5f;
	public float partPulsatance = 5.0f;
	public float vfxTime = 0.1f;
	public float growth = 0.3f;

	// Use this for initialization
	void Start ()
	{
		foreach (Transform child in transform) {
			GameObject go = child.gameObject;
			string name = go.name;
			if (name.StartsWith ("cloud_"))
				clouds.Add (child);
			else if (name.StartsWith ("part_"))
				parts.Add (child);
			else if (name.StartsWith ("vfx_")) {
				vfxs.Add (go);
				go.AddComponent<Jitter> ();
			}
		}

		for (int i = 0; i < parts.Count; ++i) {
			Transform part = parts [i];
			part.Rotate (new Vector3 (0, 0, -360 * (float)(i - 1) / parts.Count));
		}
		
		for (int i = 0; i < vfxs.Count; ++i) {
			GameObject vfx = vfxs [i];
			if (i < vfxs.Count / 2)
				vfx.GetComponent<MeshRenderer> ().enabled = false;
		}
		nextVfxTime = vfxTime;
	}
	
	public void BeginFight (List<Princess> fighters)
	{
		clampSpeed = 1;
		
		float xPosition = fighters.Select (fighter => fighter.transform.position.x).Sum () / fighters.Count ();
		float yPosition = fighters.Select (fighter => fighter.transform.position.x).Sum () / fighters.Count () + cloudOffsetY;
		transform.position = new Vector3 (xPosition, yPosition, -40);
	}
	
	public void EndFight ()
	{
		clampSpeed = -1;
	}

	void Update ()
	{
		float t = Time.time;
		float dt = Time.deltaTime;

		for (int i = 0; i < clouds.Count; ++i) {
			Transform cloud = clouds [i];
			float index = 2 * Mathf.PI * (float)i / clouds.Count;
			float radius = cloudRadiusMean + Mathf.Cos (t * cloudRadiusPulsatance + index) * cloudRadiusAmplitude;
			
			cloud.localPosition = new Vector3 (Mathf.Cos (t * cloudPulsatance + index) * radius, Mathf.Sin (t * cloudPulsatance + index) * radius, Mathf.Cos (t + (i * 5 * Mathf.PI) / clouds.Count) * 0.1f);
			cloud.Rotate (new Vector3 (0, 0, - dt * cloudAngularSpeed));
		}
		
		for (int i = 0; i < parts.Count; ++i) {
			Transform part = parts [i];
			float index = 2 * Mathf.PI * (float)i / parts.Count;
			float angle = t * partPulsatance + index;
			float radius = Mathf.Abs (Mathf.Cos (t * partRadiusPulsatance + index) * partRadiusAmplitude) + partRadiusBase;
			
			part.localPosition = new Vector3 (Mathf.Cos (angle) * radius, Mathf.Sin (angle) * radius, Mathf.Cos (t * 4 + (i * 5 * Mathf.PI) / parts.Count) * 2f + 1);
			part.Rotate (new Vector3 (0, 0, - dt * partPulsatance * 180 / Mathf.PI));
		}

		nextVfxTime -= dt;
		if (nextVfxTime <= 0) {
			nextVfxTime += vfxTime;
			GameObject vfx = vfxs [nextVfx];
			vfx.GetComponent<MeshRenderer> ().enabled = true;
			vfx.GetComponent<Jitter> ().Start ();
			vfxs [(nextVfx + vfxs.Count / 2) % vfxs.Count].GetComponent<MeshRenderer> ().enabled = false;
			nextVfx = (nextVfx + 1) % vfxs.Count;
		}

		clamp = Mathf.Clamp (clamp + clampSpeed * dt / growth, 0, 1);
		float s = (Mathf.Sin (t * 3) * 0.1f + 1.0f) * clamp;
		transform.localScale = new Vector3 (s, s, s);
	}
}
