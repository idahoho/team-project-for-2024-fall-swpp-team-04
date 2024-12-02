using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_2_Trigger : MonoBehaviour
{
    [SerializeField] private bool _isFromLobby;
    private Door_2 _door;

    private void Start() {
        _door = transform.GetComponentInParent<Door_2>();
    }
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")) {
            if(_isFromLobby) {
                if(_door.isOpenableFromLobby) {
                    gameObject.SetActive(false);
                }
            } else {
                if(_door.isOpenableFromStage) {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
