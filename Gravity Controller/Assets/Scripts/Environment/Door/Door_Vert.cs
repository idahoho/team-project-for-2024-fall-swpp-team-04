using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Vert : MonoBehaviour, IDoor
{
    [SerializeField] private float _moveDistance;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _openTime;
    [SerializeField] DoorLever _doorLever;
    private Vector3 _originalPos;
    private Vector3 _targetPos;
    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
        _originalPos = transform.position;
        _targetPos = transform.position + new Vector3(0, _moveDistance, 0);
    }
    public void Open() {
        _audioSource.Play();
        StopCoroutine(CloseDoor());
        StartCoroutine(OpenDoor());
        StartCoroutine(CloseTimer());
    }
    public void Close() {
        _audioSource.Play();
        StopCoroutine(OpenDoor());
        _doorLever.ResetLever();
        StartCoroutine(CloseDoor());
    }

    private IEnumerator CloseTimer() {
        yield return new WaitForSeconds(_openTime);
        Close();
    }

    private IEnumerator OpenDoor() {
        while(transform.position.y < _targetPos.y) {
            transform.position = Vector3.Slerp(transform.position, _targetPos + Vector3.up, Time.deltaTime * _moveSpeed);
            yield return null;
        }
    }
    private IEnumerator CloseDoor() {
        while(transform.position.y > _originalPos.y) {
            transform.position = Vector3.Slerp(transform.position, _originalPos + Vector3.down, Time.deltaTime * _moveSpeed);
            yield return null;
        }
    }
}
