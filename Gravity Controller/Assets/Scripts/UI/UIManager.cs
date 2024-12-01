using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;
    [Header("HP")]
	[SerializeField] private List<Image> _hpSegments;
	[SerializeField] private float _hpBarDamping;
    [Header("Energy")]
    [SerializeField] private Image _energyGauge;
    [SerializeField] private TextMeshProUGUI _energyText;
    [SerializeField] private float _energyGaugeDamping;
    [Header("Bullet")]
    [SerializeField] private TextMeshProUGUI _bulletText;
    [Header("Crosshair")]
    [SerializeField] private RectTransform _crossHair;
    [SerializeField] private float _defaultSize = 100f;
	[SerializeField] private float _fireSize = 200f;
	[Header("Core Interaction")]
	[SerializeField] private GameObject _warningTriangle;
	[SerializeField] private Slider _progressBar;
	[SerializeField] private GameObject _coreText;

	[SerializeField] private GameObject _doorText;

	public static UIManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

    private void Start() {
	    _warningTriangle.SetActive(false);
	    _progressBar.gameObject.SetActive(false);
	}

    private void Update() {
        UpdateEnergy();
    }

	// issue: 어떤 건 인자를 플레이어에게 요청하고, 어떤 건 플레이어가 알아서 넘겨주는 방식이 통일성이 없어보임
	// 차라리 전부 public으로 선언하고, PlayerController에서 이를 호출하도록 해야 하나?
	// HP
	public void UpdateHP(int currentHP, int maxHP)
	{
		for (int i = 0; i < _hpSegments.Count; i++)
		{
			_hpSegments[i].enabled = i < currentHP; 
		}
	}

	// Energy
	private void UpdateEnergy()
	{
		float energyRatio = _playerController.GetEnergyRatio();

		// 게이지 값 업데이트 (Lerp로 부드럽게 전환)
		_energyGauge.fillAmount = Mathf.Lerp(_energyGauge.fillAmount, energyRatio, Time.deltaTime * _energyGaugeDamping);
		_energyText.text = (energyRatio * 100f).ToString("0.0") + "%";

		// 색상 변경 로직
		if (energyRatio >= 1f)
		{
			// 완충 시 진한 보라색으로 변경 및 추가 효과
			_energyGauge.color = new Color(75/255f, 0, 130/255f);
			if (!_isFullyCharged)
			{ // 이미 효과가 실행된 상태가 아니라면
				StartCoroutine(PlayFullEnergyEffect());
				_isFullyCharged = true; // 중복 실행 방지
			}
		}
		else if (energyRatio > 1f / 3f)
		{
			// 1/3 이상 시 연한 보라색으로 변경
			_energyGauge.color = new Color(0.8f, 0.6f, 1f); // 연한 보라색
			_isFullyCharged = false; // 다시 완충 효과 가능하도록 초기화
		}
		else
		{
			// 1/3 이하일 경우 기본 색상 (흰색)
			_energyGauge.color = Color.white;
			_isFullyCharged = false; // 다시 완충 효과 가능하도록 초기화
		}
	}

	// 완충 시 발생하는 추가 효과
	private bool _isFullyCharged = false;

	private IEnumerator PlayFullEnergyEffect()
	{
		// 예: 에너지 게이지가 번쩍이는 효과
		float effectDuration = 1f;
		float elapsedTime = 0f;

		while (elapsedTime < effectDuration)
		{
			elapsedTime += Time.deltaTime;

			// 색상을 깜빡이듯 변경
			_energyGauge.color = Color.Lerp(new Color(75 / 255f, 0, 130 / 255f), Color.white, Mathf.PingPong(elapsedTime * 2f, 1f));

			yield return null;
		}

		// 최종적으로 진한 보라색으로 유지
		_energyGauge.color = new Color(75 / 255f, 0, 130 / 255f);
	}

	// Bullet
	public void UpdateBullet(int currentBullet, int maxBullet) {
        _bulletText.text = currentBullet + " / " + maxBullet;
    }

    // Crosshair
    private void SetCrosshairSize(float size) {
		_crossHair.sizeDelta = new Vector2(size, size);
	}

    public void CrossHairFire() {
        SetCrosshairSize(_fireSize);
        StartCoroutine(ResetCrossHair());
    }

    private IEnumerator ResetCrossHair() {
        yield return new WaitForSeconds(0.1f);

        SetCrosshairSize(_defaultSize);
    }

    public void CrossHairHit() {
        // 총에 적이 맞으면 붉은색으로 변한다든가 하는 효과를 주면 좋을 것 같음
    }
    public void TriggerCoreInteractionUi()
    {
	    StartCoroutine(ShowCoreInteractionUi());
    }

    public void ECoreInteractionUi()
    {
	    _coreText.SetActive(true);
    }

    public void EDoorInteractionUi()
    {
	    _doorText.SetActive(true);
    }

    public void HideInteractionUi()
    {
	    _coreText.SetActive(false);
	    _doorText.SetActive(false);
    }
	private IEnumerator ShowCoreInteractionUi()
    {
	    // 경고 삼각형을 1초 동안 표시
	    _warningTriangle.SetActive(true);
	    yield return new WaitForSeconds(1f);
	    _warningTriangle.SetActive(false);

	    // 프로그레스 바를 1분 동안 표시
	    _progressBar.value = 0;
	    _progressBar.gameObject.SetActive(true);

	    float duration = 60f;
	    float elapsed = 0f;

	    while (elapsed < duration)
	    {
		    elapsed += Time.deltaTime;
		    _progressBar.value = Mathf.Clamp01(elapsed / duration);
		    yield return null;
	    }

	    _progressBar.gameObject.SetActive(false);
    }
}
