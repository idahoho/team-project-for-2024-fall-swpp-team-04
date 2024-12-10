using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    
    [SerializeField] private CoreController _core;
    [SerializeField] private List<Door_2> _stageDoors;
    [SerializeField] private List<GameObject> _stages;
    [SerializeField] private GameObject _bossStageDoor;
    [SerializeField] private GameObject _bossStage;

	[Header("BGM Settings")]
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _lobbyBGM;
	[SerializeField] private AudioClip _stage1BGM;
	[SerializeField] private AudioClip _stage2BGM;
	[SerializeField] private AudioClip _stage3BGM;
	[SerializeField] private AudioClip _stage4BGM;
	[SerializeField] private AudioClip _bossBGM;

	private List<bool> _isCleared;
    private int _maxStage = 4;
    private int _currentStage = 1;
    

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
        LoadStage(0);
		PlayBGM(_stage1BGM);
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadStage(int stage) {
        if(stage < 0 || stage > _maxStage) {
            Debug.Log("StageManager.LoadStage: stage index out of range");
            return;
        }
		else if(stage == _maxStage) LoadBossStage();
		else
		{
			_currentStage = stage;
			for (int i = 0; i < _maxStage; i++)
			{
				if (i == stage)
				{
					_stages[i].SetActive(true);
					_stageDoors[i].isOpenableFromLobby = true;
				}
				else
				{
					_stages[i].SetActive(false);
					_stageDoors[i].isOpenableFromLobby = false;
					_stageDoors[i].Close();
				}
				
				switch (stage)
				{
					case 0:
						PlayBGM(_stage1BGM);
						break;
					case 1:
						PlayBGM(_stage2BGM);
						break;
					case 2:
						PlayBGM(_stage3BGM);
						break;
					case 3:
						PlayBGM(_stage4BGM);
						break;
					default:
						PlayBGM(_lobbyBGM);
						break;
				}
				
			}
		}

		_currentStage++;
    }

    public void EnterStage(int stage) {
        if(stage < 0 || stage >= _maxStage) {
            Debug.Log("StageManager.StageEnter: stage index out of range");
            return;
        }
        _stageDoors[stage].isOpenableFromLobby = false;
        _stageDoors[stage].isOpenableFromStage = false;
	}

    public void ClearStage(int stage) {
        if(stage < 0 || stage >= _maxStage) {
            Debug.Log("StageManager.StageCleared: stage index out of range");
            return;
        }
        _isCleared[stage] = true;
        _stageDoors[stage].isOpenableFromStage = true;
        _core.RestoreCore(stage);

		PlayBGM(_lobbyBGM);
	}

    private void LoadBossStage() {
        for(int i = 0; i < _maxStage; i++) {
            _stages[i].SetActive(false);
            _stageDoors[i].isOpenableFromLobby = false;
        }
        _bossStage.SetActive(true);
        _bossStageDoor.GetComponent<IDoor>().Open();
    }

	private void PlayBGM(AudioClip clip)
	{
		if (_audioSource == null)
		{
			Debug.LogWarning("StageManager: AudioSource is not assigned.");
			return;
		}

		if (clip == null)
		{
			Debug.LogWarning("StageManager: Attempt to play null AudioClip.");
			return;
		}

		_audioSource.clip = clip;
		_audioSource.loop = true;
		_audioSource.Play();
	}
}
