using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public EventHandler OnGameStart;

    void Awake() {
        if (!instance) instance = this;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "GameScene") StartGame();
    }

    private void StartGame() {
        OnGameStart?.Invoke(this, EventArgs.Empty);
    }
}
