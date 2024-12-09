using System.Collections;
using UnityEngine;

public class BossStageController : MonoBehaviour
{
	[SerializeField] private Vector3 _moveDirection = Vector3.up;
	[SerializeField] private float _moveDistance = 1000f;
	[SerializeField] private float _moveDuration = 20f;

	[SerializeField] private GameObject _videoCanvas;
	[SerializeField] public GameObject _finalCanvas; // finalCanvas 접근을 위해 public으로 변경

	private Vector3 _startPosition;
	private Vector3 _targetPosition;
	private bool _isMoving = false;

	void Start()
	{
		_startPosition = transform.position;
		_targetPosition = _startPosition + _moveDirection.normalized * _moveDistance;

		if (_finalCanvas != null)
			_finalCanvas.SetActive(false);
	}

	public void StartMoving()
	{
		if (!_isMoving)
			StartCoroutine(MoveStage());
	}

	private IEnumerator MoveStage()
	{
		_isMoving = true;
		float elapsedTime = 0f;

		while (elapsedTime < _moveDuration)
		{
			transform.position = Vector3.Lerp(_startPosition, _targetPosition, elapsedTime / _moveDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.position = _targetPosition;
		_isMoving = false;

		_videoCanvas.SetActive(true); _videoCanvas.SetActive(true);

		// 스테이지 이동 완료 후 영상 재생
		VideoManager vm = FindObjectOfType<VideoManager>();
		if (vm != null)
			vm.PlayVideo();
	}
}
