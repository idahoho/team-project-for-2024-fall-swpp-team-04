using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevelopUiOnOff : MonoBehaviour
{
    [SerializeField] private GameObject _developerUI;
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _core;

    [SerializeField] private InputField _inputFieldString;
    [SerializeField] private InputField _inputFieldParam;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) {
            _developerUI.SetActive(!_developerUI.activeSelf);
        }
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Developer_LoadStage(int stage) {
        _core.SendMessage("LoadStage", stage);
    }

    public void Dev_LoadBoss() {
        _core.SendMessage("LoadBossStage");
    }

    public void Developer_InitLight() {
        _core.SendMessage("InitializeGlobalLight");
    }

    public void Dev_Brighten() {
        _core.SendMessage("GlobalLightOn");
    }

    public void GotoLobby() {
        _player.transform.position = new Vector3(0,3,-5);
    }

    public void MessageToCore() {
        if(_inputFieldParam.text.Length == 0)
            _core.SendMessage(_inputFieldString.text);
        else
            _core.SendMessage(_inputFieldString.text, Int32.Parse(_inputFieldParam.text));
    }
    public void MessageToPlayer() {
        if(_inputFieldParam.text.Length == 0)
            _player.SendMessage(_inputFieldString.text);
        else
            _player.SendMessage(_inputFieldString.text, Int32.Parse(_inputFieldParam.text));
    }
}
