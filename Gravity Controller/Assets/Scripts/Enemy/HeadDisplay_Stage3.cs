using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadDisplay_Stage3 : MonoBehaviour
{
	[SerializeField] private float _alternateSpeed;

	private float _elapsedTime = 0;
	private	Material _material;
	// Start is called before the first frame update
	void Start()
    {
		_elapsedTime = 0;
		_material = GetComponent<Renderer>().materials[1];

	}

    // Update is called once per frame
    void Update()
    {
        _elapsedTime += Time.deltaTime;
		_material.SetFloat("_Progress", Mathf.Sin(_elapsedTime * _alternateSpeed));
    }
}
