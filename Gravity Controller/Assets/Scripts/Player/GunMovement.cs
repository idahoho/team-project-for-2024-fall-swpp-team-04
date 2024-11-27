using System.Collections;
using UnityEngine;

public class GunMovement : MonoBehaviour
{
	// **Recoil Parameters**
	[Header("Recoil Settings")]
	[SerializeField] private Vector3 _recoilOffset = new Vector3(0.2f, -0.2f, -0.03f); // Recoil position offset
	[SerializeField] private float _recoilDuration = 0.12f;                     // Recoil duration

	// **Hide Gun Parameters**
	[Header("Hide Gun Settings")]
	[SerializeField] private Vector3 _hiddenOffset = new Vector3(0, -4.5f, -0.5f);    // Position to hide the gun
	[SerializeField] private float _hiddenDurationSkill = 0.25f;                // Duration to hide gun during skill usage
	[SerializeField] private float _hiddenDurationReload = 1.5f;                  // Duration to hide gun during reload

	// **Movement Parameters**
	[Header("Movement Settings")]
	[SerializeField] private float _movementSwayAmountX = 1f;   // Movement sway range on X
	[SerializeField] private float _movementSwayAmountZ = 0.2f;  // Movement sway range on Z
	[SerializeField] private float _movementSwaySpeed = 3f;       // Sway speed
	[SerializeField] private float _movementAngleLimit = 30f;     // Maximum sway angle in degrees 
	[SerializeField] private float _maxMoveSpeed = 20f;           // Maximum movement speed for normalization

	// **Player Settings**
	[Header("Player Settings")]
	[SerializeField] private PlayerMovement _playerMovement; // Assign the PlayerMovement script in the Inspector

	// **Internal State**
	private Vector3 _initialPosition; // Initial gun position
	private bool _isAnimating = false; // Is an animation currently playing
	private Coroutine _movementSwayCoroutine; // Reference to the sway coroutine

	void Start()
	{
		_initialPosition = transform.localPosition;

		if (_playerMovement == null)
		{
			Debug.LogError("PlayerMovement is not assigned in the GunMovement script.");
		}
	}

	void Update()
	{
		if (_playerMovement == null)
			return;

		// Get the current movement speed
		float speed = _playerMovement._moveSpeedGun;

		// Start movement sway only if not animating
		if (speed > 0.1f && !_isAnimating)
		{
			if (_movementSwayCoroutine == null)
			{
				_movementSwayCoroutine = StartCoroutine(HandleMovementSway(speed));
			}
		}
		else
		{
			StopMovementSway();
		}

		// Handle recoil on left mouse button click
		if (Input.GetMouseButtonDown(0))
		{
			if (!_isAnimating)
			{
				StartCoroutine(HandleRecoil());
			}
		}

		// Handle hiding gun on 'R' key press (reload)
		if (Input.GetKeyDown(KeyCode.R))
		{
			if (!_isAnimating)
			{
				StartCoroutine(HideGun(true));
			}
		}

		// Handle hiding gun on right mouse button or 'V' key press (skill usage)
		if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.V))
		{
			if (!_isAnimating)
			{
				StartCoroutine(HideGun(false));
			}
		}
	}

	/// <summary>
	/// Handles the movement sway of the gun based on the player's current speed.
	/// Implements a sector-shaped (fan-shaped) sway within a limited angle.
	/// </summary>
	/// <param name="speed">The current movement speed of the player.</param>
	IEnumerator HandleMovementSway(float speed)
	{
		float swayIntensity = Mathf.Clamp01(speed / _maxMoveSpeed);

		float maxSwayAngle = _movementAngleLimit * swayIntensity;

		float swayPhase = 0f;

		while (_playerMovement != null && _playerMovement._moveSpeedGun > 0.1f && !_isAnimating)
		{
			swayPhase += Time.deltaTime * _movementSwaySpeed;

			float currentSwayAngle = Mathf.Sin(swayPhase) * maxSwayAngle;

			float angleRad = currentSwayAngle * Mathf.Deg2Rad;

			float swayOffsetX = Mathf.Sin(angleRad) * _movementSwayAmountX;
			float swayOffsetZ = Mathf.Cos(angleRad) * _movementSwayAmountZ;

			Vector3 newPosition = _initialPosition + new Vector3(swayOffsetX, 0f, -swayOffsetZ);
			transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime * _movementSwaySpeed);

			yield return null;
		}

		while (Vector3.Distance(transform.localPosition, _initialPosition) > 0.001f)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * _movementSwaySpeed);
			yield return null;
		}

		_movementSwayCoroutine = null;
	}
	/// <summary>
	/// Stops the movement sway coroutine and initiates returning the gun to its initial position.
	/// </summary>
	private void StopMovementSway()
	{
		if (_movementSwayCoroutine != null)
		{
			StopCoroutine(_movementSwayCoroutine);
			_movementSwayCoroutine = null;
		}
		if (!_isAnimating) StartCoroutine(ReturnGunToInitialPosition());
	}
	/// <summary>
	/// Smoothly returns the gun to its initial position over time.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>

	IEnumerator ReturnGunToInitialPosition()
	{
		while (Vector3.Distance(transform.localPosition, _initialPosition) > 0.001f)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * _movementSwaySpeed);
			yield return null;
		}


		transform.localPosition = _initialPosition;

	}
	/// <summary>
	/// Handles the recoil animation when the gun is fired.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>

	IEnumerator HandleRecoil()
	{
		_isAnimating = true;
		Vector3 targetRecoil = _initialPosition + _recoilOffset;
		yield return StartCoroutine(MoveGun(targetRecoil, _recoilDuration));
		yield return StartCoroutine(MoveGun(_initialPosition, _recoilDuration));

		_isAnimating = false;
	}

	/// <summary>
	/// Hide guns when reload or cast skills
	/// </summary>
	IEnumerator HideGun(bool isReload)
	{
		_isAnimating = true;

		StopMovementSway();

		yield return StartCoroutine(MoveGun(_hiddenOffset, _hiddenDurationSkill));

		if (isReload) yield return StartCoroutine(MoveGun(_hiddenOffset, _hiddenDurationReload));
		yield return StartCoroutine(MoveGun(_initialPosition, _hiddenDurationSkill));

		_isAnimating = false;
	}

	/// <summary>
	/// Smoothly moves the gun to a target position over a specified duration.
	/// </summary>
	/// <param name="targetPosition">The target local position.</param>
	/// <param name="duration">Duration of the movement.</param>
	IEnumerator MoveGun(Vector3 targetPosition, float duration)
	{
		Vector3 startPosition = transform.localPosition;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}

		transform.localPosition = targetPosition;
	}
}
