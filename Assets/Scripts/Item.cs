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

	public Sprite balloon;
}
