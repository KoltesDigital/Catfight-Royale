using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainCamera : MonoBehaviour
{
	private enum Mode
	{
		Menu,
		King,
		Balloon,
		Player,
		Winner,
		Gameplay
	}

	private Mode mode = Mode.Menu;
	private GameObject playerTarget;
	private bool playerGrab;
	
	public Camera menuCameraTarget;
	public Camera kingCameraTarget;
	public Camera balloonCameraTarget;

	public float cameraZ;

	public float gameplayMarginX = 10.0f;
	public float gameplayMarginY = 10.0f;
	public float gameplaySizeMin = 15.0f;
	public Vector3 gameplayCamOffset;
	public float gameplayMultiplierX = 0.4f;
	public float gameplayMultiplierY = 0.85f;
	
	public float playerCameraSize = 8.0f;
	public float playerCameraOffsetY = 4.0f;
	public float playerGrabCameraOffsetY = 10.0f;
	public float playerHeight = 10.0f;
	
	public float winnerCameraSize = 10.0f;
	public float winnerGrabCameraOffsetY = 12.0f;

	public float reactivity = 2.5f;

	private List<Transform> players;

	public void SetPlayers(List<Princess> playerObjects)
	{
		players = playerObjects.Select (player => player.transform).ToList();
	}
	
	public void SetMenuMode ()
	{
		mode = Mode.Menu;
	}
	
	public void SetKingMode ()
	{
		mode = Mode.King;
	}
	
	public void SetBalloonMode ()
	{
		mode = Mode.Balloon;
	}
	
	public void SetPlayerMode (GameObject playerObject, bool grab = false)
	{
		mode = Mode.Player;
		playerTarget = playerObject;
		playerGrab = grab;
	}
	
	public void SetWinnerMode (GameObject playerObject)
	{
		mode = Mode.Winner;
		playerTarget = playerObject;
	}

	public void SetGameplayMode ()
	{
		mode = Mode.Gameplay;
	}

	void FixedUpdate ()
	{
		Vector3 targetPosition;
		float targetSize;

		switch (mode) {
		case Mode.Menu:
			targetPosition = menuCameraTarget.transform.position;
			targetSize = menuCameraTarget.orthographicSize;
			break;
			
		case Mode.King:
			targetPosition = kingCameraTarget.transform.position;
			targetSize = kingCameraTarget.orthographicSize;
			break;
			
		case Mode.Balloon:
			targetPosition = balloonCameraTarget.transform.position;
			targetSize = balloonCameraTarget.orthographicSize;
			break;
			
		case Mode.Player:
			targetPosition = playerTarget.transform.position;
			targetPosition.y += playerGrab ? playerGrabCameraOffsetY : playerCameraOffsetY;
			targetSize = playerCameraSize;
			break;
			
		case Mode.Winner:
			targetPosition = playerTarget.transform.position;
			targetPosition.y += winnerGrabCameraOffsetY;
			targetSize = winnerCameraSize;
			break;

		default: // case CameraState.Gameplay:
			
			// Compute player position range
			float[] xPositions = players.Select(player => player.position.x).ToArray();
			float[] yPositions = players.Select(player => player.position.y).ToArray();
			
			float xMin = Mathf.Min (xPositions);
			float xMax = Mathf.Max (xPositions);
			
			float yMin = Mathf.Min (yPositions);
			float yMax = Mathf.Max (yPositions) + playerHeight;
			
			float xCenter = Mathf.Lerp (xMin, xMax, 0.5f);
			float yCenter = Mathf.Lerp (yMin, yMax, 0.5f);

			float xDiff = Mathf.Abs (xMax - xMin) * gameplayMultiplierX;
			float yDiff = Mathf.Abs (yMax - yMin) * gameplayMultiplierY;

			float maxDiff = Mathf.Max (xDiff + gameplayMarginX, yDiff + gameplayMarginY);

			targetSize = Mathf.Max (maxDiff, gameplaySizeMin);
			targetPosition = new Vector3 (xCenter, yCenter, 0f) + gameplayCamOffset;
			break;
		}

		float blendRatio = 1 - Mathf.Exp (- Time.deltaTime * reactivity);
		camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, targetSize, blendRatio);

		Vector3 newPosition = Vector3.Lerp (camera.transform.position, targetPosition, blendRatio);
		newPosition.z = cameraZ;
		camera.transform.position = newPosition;
	}
}
