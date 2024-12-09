using UnityEngine;

public class PlatformController : MonoBehaviour
{
	[SerializeField] private float _descentSpeed = 1f; // 초당 하강 속도
	private float _initialY; // 초기 Y좌표
	private float _targetY; // 7만큼 내려간 목표 Y좌표
	private bool _isPlayerOnPlatform = false; // 플레이어가 발판 위에 있는지 여부
	private bool _isMoving = false; // 발판 이동 상태
	private Rigidbody _rb;

	private ParticleSystem[] _dust;
	[SerializeField] private float _unitParticleRate = 100;
	[SerializeField] private float _emissionPower;
	[SerializeField] private float _unitSpeed = 1;

	// 플랫폼의 이전 위치를 추적하기 위한 변수
	private Vector3 _previousPosition;

	void Start()
	{
		_initialY = transform.position.y; // 초기 Y좌표 저장
		_targetY = _initialY - 7f; // 목표 Y좌표 계산

		// Rigidbody 컴포넌트 가져오기 또는 추가
		_rb = GetComponent<Rigidbody>();
		if (_rb == null)
		{
			_rb = gameObject.AddComponent<Rigidbody>();
			_rb.isKinematic = true; // Kinematic으로 설정
		}

		// Rigidbody 보간 활성화 (더 부드러운 움직임을 위해)
		_rb.interpolation = RigidbodyInterpolation.Interpolate;

		_previousPosition = _rb.position;

		_dust = transform.Find("Dust").GetComponentsInChildren<ParticleSystem>();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			_isPlayerOnPlatform = true;
			_isMoving = true;
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			_isPlayerOnPlatform = false;
			_isMoving = false;
		}
	}

	void Update()
	{
		if (_isPlayerOnPlatform && _isMoving)
		{
			foreach(ParticleSystem dustParticle in _dust)
			{
				dustParticle.Play();
			}
			MovePlatform();
		}
		else
		{
			foreach (ParticleSystem dustParticle in _dust)
			{
				dustParticle.Stop();
			}
		}

		_previousPosition = _rb.position;
	}

	private void MovePlatform()
	{
		float step = _descentSpeed * Time.deltaTime; 
		Vector3 targetPosition = new Vector3(transform.position.x, _targetY, transform.position.z);
		Vector3 newPosition = Vector3.MoveTowards(_rb.position, targetPosition, step);
		_rb.MovePosition(newPosition);

		foreach (ParticleSystem dustParticle in _dust)
		{
			var mult = dustParticle.main.startSpeedMultiplier;
			var em = dustParticle.emission;
			mult = _unitSpeed * (7 + _targetY - transform.position.y);
			em.rateOverTime = _unitParticleRate * Mathf.Pow((7 + _targetY - transform.position.y), _emissionPower);
		}

		if (newPosition.y <= _targetY)
		{
			_rb.position = targetPosition;
			_isMoving = false;
			
			var sparkle = transform.parent.Find("SparkleDust");
			sparkle.position = newPosition;
			sparkle.gameObject.SetActive(true);

			gameObject.SetActive(false);
		}
	}
}
