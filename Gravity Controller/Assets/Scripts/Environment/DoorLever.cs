using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorLever : MonoBehaviour, IInteractable
{
    [SerializeField] private Material _lightOff;
    [SerializeField] private GameObject _door;
    [SerializeField] private bool _isInitiallyActicated;
    private Material _lightOn;
    private Animator _animator;
    private Renderer _renderer;
    private bool _isActivated;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _renderer = transform.GetChild(0).GetComponent<Renderer>();
        _lightOn = _renderer.material;
        _isActivated = _isInitiallyActicated;
    }

    public void Interactive() {
        if(_isActivated) {
            return;
        }
        _isActivated = true;
        _animator.SetTrigger("Activate");
        _renderer.material = _lightOff;
        if(_door.TryGetComponent<IDoor>(out IDoor door)) {
            door.Open();
        }
    }

    public void ResetLever() {
        _renderer.material = _lightOn;
        StartCoroutine(ReActivate());
    }

    private IEnumerator ReActivate() {
        yield return new WaitForSeconds(2.0f);
        _isActivated = false;
    }
}
