using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_2 : MonoBehaviour, IDoor
{
    private Animator _animator;
    
    void Start()
    {
        _animator = GetComponent<Animator>();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open() {
        _animator.SetBool("Open", true);
        // GetComponent<MeshCollider>().enabled = false;
    }
}
