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

		// 영상이 아닌 텍스트 먼저 노출
		StartCoroutine(ShowTextFirstCoroutine());
	}

	private IEnumerator ShowTextFirstCoroutine()
	{
		// 텍스트 캔버스 활성화
		_textCanvas.SetActive(true);
		yield return new WaitForSeconds(3f); // 3초 노출
		_textCanvas.SetActive(false);		

		// 텍스트 노출 후 스테이지 이동 시작
		if (_videoManager._bossStageController != null)
			_videoManager._bossStageController.StartMoving();
	}
}
