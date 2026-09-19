using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public EventHandler OnGameStart;

    void Awake() {
        if (!instance) instance = this;
    }

    // private void StartGame() {
    //     OnGameStart?.Invoke(this, EventArgs.Empty);
    // }
}
