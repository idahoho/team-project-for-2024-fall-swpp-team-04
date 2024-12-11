using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door_Boss : MonoBehaviour, IDoor
{
    [SerializeField] private GameObject _doorLeft;
    [SerializeField] private GameObject _doorRight;
    private float _duration = 3f;
    private float _offsetZ = 0.3f;
    private float _offsetX = 6;
    private Vector3 _originalPosLeft;
    private Vector3 _originalPosRight;
    private float _delta = 0;
    private bool _isOpened = false;

	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _doorSound;

	private void Start() {
        _originalPosLeft = _doorLeft.transform.position;
        _originalPosRight = _doorRight.transform.position;
    }

    public void Open() {
        if(_isOpened) {
            return;
        }
		_audioSource.PlayOneShot(_doorSound);
		_delta = 0;
        _isOpened = true;
        StartCoroutine(MoveDoor());
    }
    public void Close() {
        if(!_isOpened) {
            return;
        }
		_audioSource.PlayOneShot(_doorSound);
		_delta = 0;
        _isOpened = false;
        StartCoroutine(MoveDoor());
    }

    private IEnumerator MoveDoor() {
        while(_delta < _duration) {
            float t = _isOpened ? _delta/_duration : 1 - _delta/_duration;
            _doorLeft.transform.position = new Vector3(Mathf.Lerp(_originalPosLeft.x, _originalPosLeft.x - _offsetX, EaseInOutQuint(t)), 
                                                       _doorLeft.transform.position.y, 
                                                       Mathf.Lerp(_originalPosLeft.z, _originalPosLeft.z + _offsetZ, EaseOutExpo(t)));
            _doorRight.transform.position = new Vector3(Mathf.Lerp(_originalPosRight.x, _originalPosRight.x + _offsetX, EaseInOutQuint(t)), 
                                                       _doorRight.transform.position.y, 
                                                       Mathf.Lerp(_originalPosRight.z, _originalPosRight.z + _offsetZ, EaseOutExpo(t)));

            _delta += Time.deltaTime;
            yield return null;
        }
    }

    private float EaseInOutQuint(float x) {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }

    private float EaseOutExpo(float x) {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    }
}
