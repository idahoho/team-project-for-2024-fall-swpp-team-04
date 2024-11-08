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
    [SerializeField] private Image _hpBar;
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

    }

    private void Update() {
        UpdateEnergy();
    }

    // issue: 어떤 건 인자를 플레이어에게 요청하고, 어떤 건 플레이어가 알아서 넘겨주는 방식이 통일성이 없어보임
    // 차라리 전부 public으로 선언하고, PlayerController에서 이를 호출하도록 해야 하나?
    // HP
    public void UpdateHP(int currentHP, int maxHP) {
        float hpRatio = currentHP * 1f / maxHP;
        _hpBar.fillAmount = hpRatio;
    }

    // Energy
    private void UpdateEnergy() {
        float energyRatio = _playerController.GetEnergyRatio();
        
        _energyGauge.fillAmount = Mathf.Lerp(_energyGauge.fillAmount, energyRatio, Time.deltaTime * _energyGaugeDamping);
        _energyText.text = (energyRatio * 100f).ToString("0.0") + "%";
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
}
