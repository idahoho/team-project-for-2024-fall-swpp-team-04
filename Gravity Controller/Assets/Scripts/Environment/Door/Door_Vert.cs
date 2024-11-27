using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Vert : MonoBehaviour, IDoor
{
    [SerializeField] private float _moveDistance;
    [SerializeField] private float _moveSpeed;
    private Vector3 _targetPos;
    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
    }
    public void Open() {
        _targetPos = transform.position + new Vector3(0, _moveDistance, 0);
        _audioSource.Play();
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor() {
        while(transform.position.y < _targetPos.y) {
            transform.position = Vector3.Slerp(transform.position, _targetPos, Time.deltaTime * _moveSpeed);
            yield return null;
        }
    }
}
