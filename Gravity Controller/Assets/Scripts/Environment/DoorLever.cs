using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorLever : MonoBehaviour, IInteractable
{
    [SerializeField] private Material _lightOff;
    [SerializeField] private GameObject _door;
    [SerializeField] private bool _isInitiallyInteractable;
    private Material _lightOn;
    private Animator _animator;
    private Renderer _renderer;
    private bool _isInteractable;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _renderer = transform.GetChild(0).GetComponent<Renderer>();
        _lightOn = _renderer.material;
        _isInteractable = _isInitiallyInteractable;
    }

    public void Interactive() {
        if(!_isInteractable) {
            return;
        }
        _isInteractable = false;
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
        _isInteractable = true;
    }
}
