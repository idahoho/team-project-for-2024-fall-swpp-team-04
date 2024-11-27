using UnityEngine;

public class CoreInteraction : MonoBehaviour, IInteractable
{
	[SerializeField] private Light _coreLight; 
	[SerializeField] private GameObject _door;
	[SerializeField] private GameObject _wall;
	private GameManager _gameManager;

	private bool _isCheckingEnemies = false; 

	void Start()
	{
		_gameManager = FindObjectOfType<GameManager>();
		_wall.SetActive(false);
	}

	public void Interactive()
	{
		if (_coreLight != null)
		{
			_coreLight.enabled = false;
		}
		_wall.SetActive(true);

		StartCoroutine(StartCheckingEnemiesAfterDelay(62f));
	}

	private System.Collections.IEnumerator StartCheckingEnemiesAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		_isCheckingEnemies = true;

		while (_isCheckingEnemies)
		{
			if (_gameManager.GetActiveEnemies().Count == 0)
			{
				if (_door.TryGetComponent<IDoor>(out IDoor door))
				{
					door.Open();
				}
				_isCheckingEnemies = false; 
			}

			yield return new WaitForSeconds(1f); 
		}
	}
}