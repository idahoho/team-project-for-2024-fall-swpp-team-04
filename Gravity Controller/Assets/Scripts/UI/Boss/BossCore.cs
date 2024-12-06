using System.Collections;
using UnityEngine;

public class BossCore : MonoBehaviour
{
	[SerializeField] private Light _coreLight;
	[SerializeField] private GameObject _textCanvas;
	[SerializeField] private GameObject _videoCanvas;
	[SerializeField] private float _interactionRadius = 5f;

	private VideoManager _videoManager;
	private bool _hasInteracted = false;

	void Start()
	{
		_textCanvas.SetActive(false);
		_videoCanvas.SetActive(false);
		_coreLight.enabled = false;
		_videoManager = _videoCanvas.GetComponent<VideoManager>();
	}

	void Update()
	{
		if (!_hasInteracted && IsPlayerWithinRadius())
		{
			StartInteraction();
		}
	}

	private bool IsPlayerWithinRadius()
	{
		GameObject player = GameObject.FindWithTag("Player");
		if (player == null) return false;

		float distance = Vector3.Distance(transform.position, player.transform.position);
		Debug.Log($"Distance to player: {distance}");
		return distance <= _interactionRadius;
	}

	private void StartInteraction()
	{
		_hasInteracted = true;
		_coreLight.enabled = true;

		_videoCanvas.SetActive(true);
		_videoManager.PlayVideo();
	}

	public void ShowTextAfterVideoEnded()
	{
		StartCoroutine(ShowTextCoroutine());
	}

	private IEnumerator ShowTextCoroutine()
	{
		_textCanvas.SetActive(true);
		yield return new WaitForSeconds(3f);

		_textCanvas.SetActive(false);

		if (_videoManager._bossStageController != null)
			_videoManager._bossStageController.StartMoving();
	}
}
