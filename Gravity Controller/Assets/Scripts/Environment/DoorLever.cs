using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLever : MonoBehaviour, IInteractable
{
    [SerializeField] private Material _lightoff;
    [SerializeField] private GameObject _door;
    private Animator _animator;
    private Renderer _renderer;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _renderer = transform.GetChild(0).GetComponent<Renderer>();
    }

    public void Interactive() {
        _animator.SetTrigger("Activate");
        _renderer.material = _lightoff;
        if(_door.TryGetComponent<IDoor>(out IDoor door)) {
            door.Open();
        }
    }
}
