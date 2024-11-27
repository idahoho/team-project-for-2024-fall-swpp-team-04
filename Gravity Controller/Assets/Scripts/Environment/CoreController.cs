using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreController : MonoBehaviour, IInteractable
{
    [Header("Global Light On")]
    [SerializeField] private Light _sunLight;
    [SerializeField] private float _sunLightIntensity;
    [SerializeField] private Color _environmentLight;
    [SerializeField] private float _fogDensity;
    [SerializeField] private float _lightOnDamping;
    private bool _isLightOn = false;
    private float _epsilon = 1e-10f;

    [Header("Core Restored")]
    [SerializeField] private Renderer[] _batteries;
    [SerializeField] private Material _blueEmission;
    [SerializeField] private Light[] _batteryLights;
    [SerializeField] private float _batteryLightIntensity;
    private bool _fullCharged = false;

    // Start is called before the first frame update
    void Start()
    {
        
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

    private IEnumerator GlobalLightOn() {
        while(_sunLight.intensity < _sunLightIntensity - _epsilon) {
            _sunLight.intensity = Mathf.Lerp(_sunLight.intensity, _sunLightIntensity, Time.deltaTime * _lightOnDamping);
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, _environmentLight, Time.deltaTime * _lightOnDamping);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, _fogDensity, Time.deltaTime * _lightOnDamping);
            yield return null;
        }
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
