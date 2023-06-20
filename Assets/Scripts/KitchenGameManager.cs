using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour {


    public static KitchenGameManager Instance { get; private set; }



    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;


    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        GameWin
    }


    private State state;
    public float countdownToStartTimer = 3f;
    public float gamePlayingTimer;
    public float gamePlayingTimerMax = 60f;
    public bool isGamePaused = false;

    [SerializeField] private DeliveryManager deliveryManager;
    private int successfulRecipesAmount;
    public int RecipesToWin => deliveryManager.recipesToWin;


    private void Awake() {
        Instance = this;

        state = State.WaitingToStart;
    }

    private void Start() {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        deliveryManager = DeliveryManager.Instance;
        deliveryManager.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
    }
    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e) {
    successfulRecipesAmount = deliveryManager.GetSuccessfulRecipesAmount();
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e) {
        if (state == State.WaitingToStart) {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }

    private void Update() {
        switch (state) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f) {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (successfulRecipesAmount >= RecipesToWin) {
                    state = State.GameWin;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                if (gamePlayingTimer < 0f) {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
            case State.GameWin:
                break;
        }
    }

    public bool IsGamePlaying() {
        return state == State.GamePlaying;
    }

    public bool IsCountdownToStartActive() {
        return state == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer;
    }

    public bool IsGameOver() {
        return state == State.GameOver;
    }

    public bool IsGameWin() {
        return state == State.GameWin;
    }

    public float GetGamePlayingTimerNormalized() {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }

    public void TogglePauseGame() {
        isGamePaused = !isGamePaused;
        if (isGamePaused) {
            Time.timeScale = 0f;

            OnGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            Time.timeScale = 1f;

            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

}