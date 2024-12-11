using UnityEngine;
using System.Collections;

public class CoreInteraction : MonoBehaviour, IInteractable
{

	[Header("Global Light Off")]
	[SerializeField] private float _initialSunLightIntensity;
	[SerializeField] private Color _initialEnvironmentLightColor;
	[SerializeField] private float _initialEnvironmentLightIntensity;
	[SerializeField] private Color _initialFogColor;
	[SerializeField] private float _initialFogDensity;
	[SerializeField] private EmergencyLightController[] _beacons;
	[Header("Global Light On")]
	[SerializeField] private Light _sunLight;
	[SerializeField] private float _sunLightIntensity;
	[SerializeField] private Color _environmentLightColor;
	[SerializeField] private float _environmentLightIntensity;
	[SerializeField] private float _fogDensity;
	[SerializeField] private float _lightOnDamping;
	private float _epsilon = 1e-5f;

   [SerializeField] private Light _coreLight;
   [SerializeField] private GameObject _door;
   [SerializeField] private GameObject _wall;
   [SerializeField] private GameObject _spawner;
   [SerializeField] private Renderer _forceFieldRenderer;
   [SerializeField] private CoreController _coreController;
   [SerializeField] private float _delay;
   private GameManager _gameManager;

   private bool _isInteractable = true;
   private bool _hasEnemiesCleared = false; // 적 처치 완료 여부

	[Header("Audio")]
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _timeOutSound;

	void Start()
   {
      _gameManager = FindObjectOfType<GameManager>();
      _wall.SetActive(false);
      _spawner.SetActive(false);

      if (_coreLight != null)
      {
         _coreLight.enabled = false;
      }
      if (_forceFieldRenderer != null)
      {
         Material forceFieldMaterial = _forceFieldRenderer.material;
         forceFieldMaterial.DisableKeyword("_EMISSION");
         forceFieldMaterial.SetColor("_EmissionColor", Color.black);
      }
   }

   public void Interactive()
   {
      if (!_isInteractable) return;

      if (!_hasEnemiesCleared)
      {
         // 첫 번째 상호작용: 조명(Light)만 활성화
         _isInteractable = false;
         if (_coreLight != null)
         {
            _coreLight.enabled = true;
         }

		 StartCoroutine(GlobalLightOff());

		 _wall.SetActive(true);
         UIManager.Instance.TriggerCoreInteractionUi();
         _spawner.SetActive(true);

         StartCoroutine(ClearEnemiesAndActivateEmission());
      }
      else
      {
         if (_forceFieldRenderer != null)
         {
            Material forceFieldMaterial = _forceFieldRenderer.material;
            forceFieldMaterial.EnableKeyword("_EMISSION");
            forceFieldMaterial.SetColor("_EmissionColor", Color.white); 
         }

		 _isInteractable = false; 
		 StartCoroutine(GlobalLightOn());

         if (_door.TryGetComponent<IDoor>(out IDoor door))
         {
            door.Open();
         }
         _coreController.ResetCoreController();
      }
   }

   private System.Collections.IEnumerator ClearEnemiesAndActivateEmission()
   {
      yield return new WaitForSeconds(_delay);

		_audioSource.PlayOneShot(_timeOutSound);

		SendOnDeathSignalToEnemies();	  

		// 상호작용 가능 상태 복구
	  _hasEnemiesCleared = true;
      _isInteractable = true;
   }

   private void SendOnDeathSignalToEnemies()
   {
      var activeEnemies = _gameManager.GetActiveEnemies().ToArray(); 
      foreach (var enemy in activeEnemies)
      {
         if (enemy.TryGetComponent<IEnemy>(out IEnemy enemyScript))
         {
            enemyScript.OnDeath();
         }
      }
   }

   public bool IsInteractable() => _isInteractable;

	private IEnumerator GlobalLightOff()
	{
		RenderSettings.fogColor = _initialFogColor;

		foreach (var beacon in _beacons)
		{
			beacon.TurnOn();
		}

		while (_sunLight.intensity < _sunLightIntensity - _epsilon)
		{
			_sunLight.intensity = Mathf.Lerp(_sunLight.intensity, _initialSunLightIntensity, Time.deltaTime * _lightOnDamping);
			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, _initialEnvironmentLightColor, Time.deltaTime * _lightOnDamping);
			RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, _initialEnvironmentLightIntensity, Time.deltaTime * _lightOnDamping);
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, _initialFogDensity, Time.deltaTime * _lightOnDamping);
			yield return null;
		}
		_sunLight.intensity = _initialSunLightIntensity;
		RenderSettings.ambientLight = _initialEnvironmentLightColor;
		RenderSettings.ambientIntensity = _initialEnvironmentLightIntensity;
		RenderSettings.fogDensity = _initialFogDensity;
		RenderSettings.fogColor = _initialFogColor;
	}

	private IEnumerator GlobalLightOn()
	{
		foreach (var beacon in _beacons)
		{
			beacon.TurnOff();
		}

		while (_sunLight.intensity < _sunLightIntensity - _epsilon)
		{
			_sunLight.intensity = Mathf.Lerp(_sunLight.intensity, _sunLightIntensity, Time.deltaTime * _lightOnDamping);
			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, _environmentLightColor, Time.deltaTime * _lightOnDamping);
			RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, _environmentLightIntensity, Time.deltaTime * _lightOnDamping);
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, _fogDensity, Time.deltaTime * _lightOnDamping);
			yield return null;
		}
		_sunLight.intensity = _sunLightIntensity;
		RenderSettings.ambientLight = _environmentLightColor;
		RenderSettings.ambientIntensity = _environmentLightIntensity;
		RenderSettings.fogDensity = _fogDensity;
	}
}
