using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Door_2 : MonoBehaviour, IDoor
{
    [SerializeField] private GameObject _openTrigger;
    [SerializeField] private GameObject _closeTrigger;
    private Animator _animator;
    public bool _isTriggerOpenable = false;
    private bool _isOpened = false;
    
    void Start()
    {
        _animator = GetComponent<Animator>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(_isTriggerOpenable) {
            if(_isOpened && _closeTrigger && !_closeTrigger.activeSelf) {
                Close();
                _openTrigger.SetActive(true);
            } else if(!_isOpened && _openTrigger && !_openTrigger.activeSelf) {
                Open();
                _closeTrigger.SetActive(true);
            }
        }
    }

    public void Open() {
        if(_isOpened) {
            return;
        }
        _isOpened = true;
        _animator.SetBool("Open", true);
        // GetComponent<MeshCollider>().enabled = false;
    }

    public void Close() {
        if(!_isOpened) {
            return;
        }
        _isOpened = false;
        _animator.SetBool("Open", false);
    }
}
