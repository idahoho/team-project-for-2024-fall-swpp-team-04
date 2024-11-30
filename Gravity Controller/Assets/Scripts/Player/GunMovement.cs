using System.Collections;
using UnityEngine;

public class GunMovement : MonoBehaviour
{
	// **Recoil Parameters**
	[Header("Recoil Settings")]
	[SerializeField] private Vector3 _recoilOffset = new Vector3(0.03f, -0.03f, -0.01f); // Recoil position offset
	[SerializeField] private float _recoilDuration = 0.3f; // Recoil duration
	[SerializeField] private float _gunCoolDown = 0.5f;

	// **Hide Gun Parameters**
	[Header("Hide Gun Settings")]
	[SerializeField] private Vector3 _hiddenOffset = new Vector3(0, -4.5f, -0.5f); // Position to hide the gun
	[SerializeField] private float _hiddenDurationSkill = 0.25f; // Duration to hide gun during skill usage
	[SerializeField] private float _hiddenDurationReload = 1f; // Duration to hide gun during reload

	// **Movement Parameters**
	[Header("Movement Settings")]
	[SerializeField] private float _movementSwayAmountX = 1f; // Movement sway range on X
	[SerializeField] private float _movementSwayAmountZ = 0.2f; // Movement sway range on Z
	[SerializeField] private float _movementSwaySpeed = 3f; // Sway speed
	[SerializeField] private float _movementAngleLimit = 30f; // Maximum sway angle in degrees 
	[SerializeField] private float _maxMoveSpeed = 20f; // Maximum movement speed for normalization

	// **Player Settings**
	[Header("Player Settings")]
	[SerializeField] private PlayerMovement _playerMovement; // Assign the PlayerMovement script in the Inspector

	// **Gun Emission Settings**
	[Header("Gun Emission Settings")]
	[SerializeField] private Material _gunMaterial; // Material to control Emission
	[SerializeField] private Color _emissionColor = Color.white; // Default Emission color
	[SerializeField] private float _emissionFadeDuration = 1f; // Emission fade duration

	// **Rotation Parameters**
	[Header("Rotation Settings")]
	[SerializeField] private Vector3 _reloadRotationOffset = new Vector3(5f, 5f, -3f); // Rotation offset during reload
	[SerializeField] private float _reloadRotationDuration = 0.5f; // Duration to rotate and return

	// **Recoil Rotation Parameters**
	[Header("Recoil Rotation Settings")]
	[SerializeField] private Vector3 _recoilRotationOffset = new Vector3(3f, 5f, -3f); // Rotation offset during recoil
	[SerializeField] private float _recoilRotationDuration = 0.3f; // Total duration for recoil rotation (forward and back)

	// **Internal State**
	private Vector3 _initialPosition; // Initial gun position
	private Quaternion _initialRotation; // Initial gun rotation
	private bool _isAnimating = false; // Is an animation currently playing
	private Coroutine _movementSwayCoroutine; // Reference to the sway coroutine

	// **Flags for Parallel Coroutines**
	private bool _fadeEmissionDone = false;
	private bool _rotationDone = false;

	// **Flags for Recoil Coroutines**
	private bool _recoilMovementDone = false;
	private bool _recoilRotationDone = false;

	void Start()
	{
		_initialPosition = transform.localPosition;
		_initialRotation = transform.localRotation;

		if (_playerMovement == null)
		{
			Debug.LogError("PlayerMovement is not assigned in the GunMovement script.");
		}

		// Initialize Emission color
		if (_gunMaterial != null)
		{
			_gunMaterial.SetColor("_EmissionColor", _emissionColor);
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
			transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * _movementSwaySpeed/5);
			yield return null;
		}

		transform.localPosition = _initialPosition;
	}

	/// <summary>
	/// Handles the recoil animation when the gun is fired.
	/// Rotates the gun to a target rotation during recoil and returns it back.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>
	/// <summary>
	/// Handles the recoil animation when the gun is fired.
	/// Rotates the gun to a target rotation during recoil and returns it back.
	/// Additionally, stops sway for 0.55 seconds regardless of recoil duration.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator HandleRecoil()
	{
		_isAnimating = true;

		// Stop movement sway during recoil
		StopMovementSway();

		float startTime = Time.time;

		// Reset recoil flags
		_recoilMovementDone = false;
		_recoilRotationDone = false;

		// Start moving the gun for recoil
		StartCoroutine(MoveGun(_initialPosition + _recoilOffset, _recoilDuration / 2f, () => { _recoilMovementDone = true; }));

		// Start rotating the gun for recoil
		StartCoroutine(RotateGunDuringRecoil());

		// Wait until both movement and rotation are done
		yield return new WaitUntil(() => _recoilMovementDone && _recoilRotationDone);

		// Start moving the gun back to initial position
		_recoilMovementDone = false;
		StartCoroutine(MoveGun(_initialPosition, _recoilDuration / 2f, () => { _recoilMovementDone = true; }));

		// Start rotating the gun back to initial rotation
		_recoilRotationDone = false;
		StartCoroutine(RotateGunBackFromRecoil());

		// Wait until both movement and rotation back are done
		yield return new WaitUntil(() => _recoilMovementDone && _recoilRotationDone);

		// Calculate elapsed time since recoil started
		float elapsedTime = Time.time - startTime;

		if (elapsedTime < _gunCoolDown)
		{
			yield return new WaitForSeconds(_gunCoolDown - elapsedTime);
		}

		_isAnimating = false;
	}


	/// <summary>
	/// Rotates the gun slightly during recoil.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator RotateGunDuringRecoil()
	{
		Quaternion targetRotation = _initialRotation * Quaternion.Euler(_recoilRotationOffset);
		yield return StartCoroutine(RotateGun(targetRotation, _recoilRotationDuration / 2f));
		_recoilRotationDone = true;
	}

	/// <summary>
	/// Rotates the gun back to its original rotation after recoil.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator RotateGunBackFromRecoil()
	{
		yield return StartCoroutine(RotateGun(_initialRotation, _recoilRotationDuration / 2f));
		_recoilRotationDone = true;
	}

	private void HideGunOnReload()
	{
		StartCoroutine(HideGun(true));
	}

	private void HideGunOnSkill()
	{
		StartCoroutine(HideGun(false));
	}

	/// <summary>
	/// Hide guns when reload or cast skills
	/// </summary>
	/// <param name="isReload">True if hiding due to reload, false for skill usage.</param>
	IEnumerator HideGun(bool isReload)
	{
		_isAnimating = true;

		StopMovementSway();

		if (isReload)
		{
			// Reset flags
			_fadeEmissionDone = false;
			_rotationDone = false;

			// Start Emission fade and Rotation simultaneously
			StartCoroutine(FadeEmissionSequence());
			StartCoroutine(RotateGunDuringReload());

			// Wait until both Emission fade and Rotation are done
			yield return new WaitUntil(() => _fadeEmissionDone && _rotationDone);
		}
		else
		{
			yield return StartCoroutine(MoveGun(_hiddenOffset, _hiddenDurationSkill));
			yield return StartCoroutine(MoveGun(_initialPosition, _hiddenDurationSkill));
		}

		_isAnimating = false;
	}

	/// <summary>
	/// Handles the sequence of fading Emission out and then back in.
	/// </summary>
	IEnumerator FadeEmissionSequence()
	{
		yield return StartCoroutine(FadeEmission(1f, 0f, _emissionFadeDuration));
		yield return StartCoroutine(FadeEmission(0f, 1f, _emissionFadeDuration));

		// Set the flag indicating Emission fade sequence is done
		_fadeEmissionDone = true;
	}

	/// <summary>
	/// Rotates the gun slightly during reload and returns to original rotation after a delay.
	/// </summary>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator RotateGunDuringReload()
	{
		Quaternion targetRotation = _initialRotation * Quaternion.Euler(_reloadRotationOffset);

		yield return StartCoroutine(RotateGun(targetRotation, _reloadRotationDuration / 2f));

		yield return new WaitForSeconds(_reloadRotationDuration);

		yield return StartCoroutine(RotateGun(_initialRotation, _reloadRotationDuration / 2f));

		_rotationDone = true;
	}

	/// <summary>
	/// Smoothly rotates the gun to a target rotation over a specified duration.
	/// </summary>
	/// <param name="targetRotation">The target rotation.</param>
	/// <param name="duration">Duration of the rotation.</param>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator RotateGun(Quaternion targetRotation, float duration)
	{
		Quaternion startRotation = transform.localRotation;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}

		transform.localRotation = targetRotation;
	}

	/// <summary>
	/// Fades the emission intensity from startIntensity to endIntensity over the specified duration.
	/// </summary>
	/// <param name="startIntensity">Starting emission intensity.</param>
	/// <param name="endIntensity">Ending emission intensity.</param>
	/// <param name="duration">Duration of the fade.</param>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator FadeEmission(float startIntensity, float endIntensity, float duration)
	{
		if (_gunMaterial == null) yield break;

		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float intensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);

			_gunMaterial.SetColor("_EmissionColor", _emissionColor * intensity);

			yield return null;
		}

		_gunMaterial.SetColor("_EmissionColor", _emissionColor * endIntensity);
	}

	/// <summary>
	/// Smoothly moves the gun to a target position over a specified duration.
	/// </summary>
	/// <param name="targetPosition">The target local position.</param>
	/// <param name="duration">Duration of the movement.</param>
	/// <param name="onComplete">Callback to invoke when movement is complete.</param>
	/// <returns>An IEnumerator for the coroutine.</returns>
	IEnumerator MoveGun(Vector3 targetPosition, float duration, System.Action onComplete = null)
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

		onComplete?.Invoke();
	}
}
