using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    private float mouseInputY;
    private float rotSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rotSpeed = player.sensetivityX;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
