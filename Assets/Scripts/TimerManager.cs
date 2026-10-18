using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour {
    public static TimerManager instance;

    public EventHandler OnTimerEnded;
    public float CurrentTimer {get; private set;}

    private Coroutine _countdownRoutine;

    void Awake() {
        if (!instance) instance = this;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        PlayerManager.instance.OnPlayerSpawned += StartCountup;
        PlayerManager.instance.OnPlayerOOL += StopCountup;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerManager.instance.OnPlayerSpawned -= StartCountup;
        PlayerManager.instance.OnPlayerOOL -= StopCountup;
    }

    private void StartCountup(object sender, EventArgs e) {
        StopCountup(this, EventArgs.Empty); // Stops if there's an existing countdown
        this._countdownRoutine = StartCoroutine(Countup());
    }

    private void StopCountup(object sender, EventArgs e) {
        if (this._countdownRoutine == null) return;
        StopCoroutine(this._countdownRoutine);
        this._countdownRoutine = null;
    }

    private IEnumerator Countup() {
        while (this.CurrentTimer >= 0f) {
            GameUIManager.instance.UpdateTimerText(this.CurrentTimer);
            this.CurrentTimer += Time.deltaTime;
            yield return null; // Waits until next frame
        }
    }

    public float GetCurrentTimer() => this.CurrentTimer;
}
