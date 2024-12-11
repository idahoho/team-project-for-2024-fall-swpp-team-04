using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyLightController : MonoBehaviour
{
    [SerializeField] private float _rotSpeed;
    private GameObject _lights;
    [SerializeField] private bool _isTurnedOn = true;
    // Start is called before the first frame update
    void Start()
    {
        _lights = transform.GetChild(0).gameObject;
		_lights.SetActive(_isTurnedOn);
	}

    // Update is called once per frame
    void Update()
    {
        if(_isTurnedOn) {
            transform.Rotate(Vector3.up, _rotSpeed * Time.deltaTime);
        }
    }

	public void TurnOn()
	{
		_isTurnedOn = true;
		_lights.SetActive(true);
	}

	public void TurnOff() {
        _isTurnedOn = false;
        _lights.SetActive(false);
    }
}
