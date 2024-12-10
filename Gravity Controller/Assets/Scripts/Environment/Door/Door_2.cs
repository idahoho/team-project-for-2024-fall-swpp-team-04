using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Door_2 : MonoBehaviour, IDoor
{
    [SerializeField] private GameObject _openTriggerFromLobby;
    [SerializeField] private GameObject _openTriggerFromStage;
    [SerializeField] private GameObject _closeTriggerFromLobby;
    [SerializeField] private GameObject _closeTriggerFromStage;
    private Animator _animator;
    public bool isOpenableFromLobby = false;
    public bool isOpenableFromStage = false;
    private bool _isOpened = false;

	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _doorSound;

	void Start()
    {
        _animator = GetComponent<Animator>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(isOpenableFromLobby) {
            if(_isOpened && _closeTriggerFromLobby && !_closeTriggerFromLobby.activeSelf) {
                Close();
                _openTriggerFromLobby.SetActive(true);
                _openTriggerFromStage.SetActive(true);
                _closeTriggerFromStage.SetActive(true);
            } else if(!_isOpened && _openTriggerFromLobby && !_openTriggerFromLobby.activeSelf) {
                Open();
                _closeTriggerFromLobby.SetActive(true);
                _openTriggerFromStage.SetActive(true);
                _closeTriggerFromStage.SetActive(true);
            }
        }
        if(isOpenableFromStage) {
            if(_isOpened && _closeTriggerFromStage && !_closeTriggerFromStage.activeSelf) {
                Close();
                _openTriggerFromStage.SetActive(true);
                _openTriggerFromLobby.SetActive(true);
                _closeTriggerFromLobby.SetActive(true);
            } else if (!_isOpened && _openTriggerFromStage && !_openTriggerFromStage.activeSelf) {
                Open();
                _closeTriggerFromStage.SetActive(true);
                _openTriggerFromLobby.SetActive(true);
                _closeTriggerFromLobby.SetActive(true);
            }
        }
    }

    public void Open() {
        if(_isOpened) {
            return;
        }
		_audioSource.PlayOneShot(_doorSound);
		_isOpened = true;
        _animator.SetBool("Open", true);
        // GetComponent<MeshCollider>().enabled = false;
    }

    public void Close() {
        if(!_isOpened) {
            return;
        }
		_audioSource.PlayOneShot(_doorSound);
		_isOpened = false;
        _animator.SetBool("Open", false);
    }
}
