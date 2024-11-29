using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    
    [SerializeField] private CoreController _core;
    [SerializeField] private List<Door_2> _stageDoors;
    [SerializeField] private List<GameObject> _stages;

    private List<bool> _isCleared;
    private int _maxStage = 4;
    private int _currentStage = 0;
    

    private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

    void Start()
    {
        _isCleared = new List<bool>(new bool[] {false, false, false, false});
        LoadStage(3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadStage(int stage) {
        if(stage < 0 || stage >= _maxStage) {
            Debug.Log("StageManager.LoadStage: stage index out of range");
            return;
        }
        _currentStage = stage;
        for(int i = 0; i < _maxStage; i++) {
            if(i == stage) {
                _stages[i].SetActive(true);
                _stageDoors[i]._isTriggerOpenable = true;
            } else {
                _stages[i].SetActive(false);
                _stageDoors[i]._isTriggerOpenable = false;
            }
        }
    }

    public void StageCleared(int stage) {
        if(stage < 0 || stage >= _maxStage) {
            Debug.Log("StageManager.StageCleared: stage index out of range");
            return;
        }
        _isCleared[stage] = true;
        _core.RestoreCore(stage);
    }
}
