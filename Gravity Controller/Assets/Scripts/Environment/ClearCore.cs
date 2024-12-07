using UnityEngine;

/// <summary>
/// Class to manage the Core object. 
/// Activates the light and enables interaction when all enemies are eliminated.
/// On interaction, it enables emission, opens the door, and activates Stage2.
/// </summary>
public class ClearCore : MonoBehaviour, IInteractable
{
	[Header("Core Settings")]
	[SerializeField] private Light _coreLight; // Core light
	[SerializeField] private Renderer _forceFieldRenderer; // Renderer for emission effect
	[SerializeField] private GameObject _door; // Door object (implements IDoor interface)
	private GameManager _gameManager; // Reference to GameManager
	[SerializeField] private CoreController _coreController;
	private bool _isInteractable = false; // Current interactable state

	void Start()
	{
		_gameManager = FindObjectOfType<GameManager>();
		if (_gameManager == null)
		{
			Debug.LogError("GameManager is not present in the scene.");
			return;
		}

		InitializeCore();
	}

	/// <summary>
	/// Performs initial setup for the core.
	/// Checks if all enemies are eliminated and updates the state.
	/// </summary>
	private void InitializeCore()
	{
		if (_coreLight != null)
		{
			_coreLight.enabled = false;
		}
		else
		{
			Debug.LogWarning("coreLight is not assigned.");
		}

		if (_forceFieldRenderer != null)
		{
			Material forceFieldMaterial = _forceFieldRenderer.material;
			forceFieldMaterial.DisableKeyword("_EMISSION");
			forceFieldMaterial.SetColor("_EmissionColor", Color.black);
		}
		else
		{
			Debug.LogWarning("forceFieldRenderer is not assigned.");
		}


		if (_door == null)
		{
			Debug.LogError("door GameObject is not assigned.");
		}
	}
	public bool IsInteractable()
	{
		return _isInteractable;
	}

	private void FixedUpdate()
	{
		CheckEnemiesCleared();
	}

	/// <summary>
	/// Checks if all enemies are eliminated and updates the state.
	/// </summary>
	private void CheckEnemiesCleared()
	{
		var activeEnemies = _gameManager.GetActiveEnemies();
		int enemyCount = activeEnemies != null ? activeEnemies.Count : 0;
		Debug.Log($"Enemy Count: {enemyCount}");

		if (enemyCount == 0)
		{
			Debug.Log("All enemies have been eliminated. Core interaction is now enabled.");

			if (_coreLight != null)
			{
				_coreLight.enabled = true;
				Debug.Log("Core light is now on.");
			}
			_isInteractable = true;
		}
		else
		{
			Debug.Log("There are still enemies remaining. Core interaction is disabled.");

			// Disable interaction
			_isInteractable = false;

			// Hide interaction UI
			UIManager.Instance.HideInteractionUi();
		}
	}

	/// <summary>
	/// Called when the player attempts to interact with the core.
	/// Enables emission, opens the door, and activates Stage2.
	/// </summary>
	public void Interactive()
	{
		if (!_isInteractable)
		{
			Debug.Log("ClearCore is not currently interactable.");
			return;
		}

		Debug.Log("Interacted with ClearCore.");

		if (_forceFieldRenderer != null)
		{
			Material forceFieldMaterial = _forceFieldRenderer.material;
			forceFieldMaterial.EnableKeyword("_EMISSION");
			forceFieldMaterial.SetColor("_EmissionColor", Color.white);
			Debug.Log("Force field emission is now enabled.");
		}

		if (_door != null && _door.TryGetComponent<IDoor>(out IDoor doorComponent))
		{
			doorComponent.Open();
			Debug.Log("The door is now open.");
		}
		else
		{
			Debug.LogWarning("The door object does not have an IDoor component.");
		}

		_coreController.ResetCoreController();

		// Disable interaction and hide UI
		_isInteractable = false;
		UIManager.Instance.HideInteractionUi();
	}


}