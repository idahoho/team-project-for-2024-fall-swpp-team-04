using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreController : MonoBehaviour, IInteractable
{
    [Header("Initial Global Light")]
    [SerializeField] private float _initialSunLightIntensity;
    [SerializeField] private Color _initialEnvironmentLightColor;
    [SerializeField] private float _initialEnvironmentLightIntensity;
    [SerializeField] private Color _initialFogColor;
    [SerializeField] private float _initialFogDensity;
    [Header("Global Light On")]
    [SerializeField] private Light _sunLight;
    [SerializeField] private float _sunLightIntensity;
    [SerializeField] private Color _environmentLightColor;
    [SerializeField] private float _environmentLightIntensity;
    [SerializeField] private float _fogDensity;
    [SerializeField] private float _lightOnDamping;
    private bool _isLightOn = false;
    private float _epsilon = 1e-5f;

    [Header("Core Restored")]
    [SerializeField] private Renderer[] _batteries;
    [SerializeField] private Material _blueEmission;
    [SerializeField] private Light[] _batteryLights;
    [SerializeField] private float _batteryLightIntensity;
    private bool _fullCharged = false;

    // Start is called before the first frame update
    void Start()
    {
        InitializeGlobalLight();
        // StartCoroutine(GlobalLightOn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interactive() {
        if(!_isLightOn) {
            _isLightOn = true;
            StartCoroutine(GlobalLightOn());
        }
    }
    
    private void InitializeGlobalLight() {
        _isLightOn = false;
        _sunLight.intensity = _initialSunLightIntensity;
        RenderSettings.ambientLight = _initialEnvironmentLightColor;
        RenderSettings.ambientIntensity = _initialEnvironmentLightIntensity;
        RenderSettings.fogColor = _initialFogColor;
        RenderSettings.fogDensity = _initialFogDensity;
    }

    private IEnumerator GlobalLightOn() {
        while(_sunLight.intensity < _sunLightIntensity - _epsilon) {
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

    public void RestoreCore(int stage) {
        if(stage < 0 || stage > 3) {
            Debug.Log("CoreController.RestoreCore: Invalid stage index");
            return;
        }
        _batteries[stage].materials[1] = _blueEmission;
        _batteryLights[stage].intensity = _batteryLightIntensity;
    }
}
