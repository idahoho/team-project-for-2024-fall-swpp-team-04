using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Boss_Inner : MonoBehaviour, IDoor
{
    [SerializeField] private float _duration;
    [SerializeField] private float _offset;
    private Transform _doorUp;
    private Transform _doorDown;

    private Vector3 _originalPosUp;
    private Vector3 _originalPosDown;
    private float _delta = 0;
    private bool _isOpened = false;
    private bool _isLocked = false;

    private void Start() {
        _doorDown = transform.GetChild(1);
        _doorUp = transform.GetChild(2);

        _originalPosUp = _doorUp.position;
        _originalPosDown = _doorDown.position;
    }

    private void OnTriggerEnter() {
        if(!_isLocked) {
            if(!_isOpened) {
                Open();
            } else {
                _isLocked = true;
                Close();
            }
        }
    }

    public void Open() {
        _delta = 0;
        _isOpened = true;
        StartCoroutine(MoveDoor());
    }

    public void Close() {
        _delta = 0;
        _isOpened = false;
        StartCoroutine(MoveDoor());
    }

    private IEnumerator MoveDoor() {
        while(_delta < _duration) {
            float t = _isOpened ? _delta/_duration : 1 - _delta/_duration;
            _doorUp.position = Vector3.Lerp(_originalPosUp, _originalPosUp + Vector3.up * _offset, EaseInOutQuint(t));
            _doorDown.position = Vector3.Lerp(_originalPosDown, _originalPosDown + Vector3.down * _offset, EaseInOutQuint(t));

            _delta += Time.deltaTime;
            yield return null;
        }
    }

    private float EaseInOutQuint(float x) {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }
}
