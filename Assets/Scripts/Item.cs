using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
	// during grabbing animation
	public Vector3 grabbingPosition;
	public float grabbingRotation;

	// one grabbed during gameplay
	public Vector3 grabbedPosition;
	public float grabbedRotation;
	public string grabbedBoneName;

	// associated thinking balloon
	public Sprite balloon;

	// start position used for resetting
	private Transform startParent;
	private Vector3 startPosition;
	private Quaternion startRotation;

	void Start ()
	{
		startParent = transform.parent;
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

	public void Reset()
	{
		transform.parent = startParent;
		transform.position = startPosition;
		transform.rotation = startRotation;
	}
}
