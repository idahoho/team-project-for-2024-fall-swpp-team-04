using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyLightController : MonoBehaviour
{
    [SerializeField] private float _rotSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, _rotSpeed * Time.deltaTime);
    }
}
