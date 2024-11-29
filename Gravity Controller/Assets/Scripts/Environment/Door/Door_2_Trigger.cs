using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_2_Trigger : MonoBehaviour
{
    private Door_2 _door;

    private void Start() {
        _door = transform.GetComponentInParent<Door_2>();
    }
    private void OnTriggerEnter(Collider other) {
        if(_door._isTriggerOpenable && other.CompareTag("Player")) {
            gameObject.SetActive(false);
        }
    }
}
