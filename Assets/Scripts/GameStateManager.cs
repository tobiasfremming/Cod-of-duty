using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
 
   public static GameStateManager Instance { get; private set; }
    
   public event Action OnEndGame;
   public event Action OnRestartGame;

   public enum GameState
    {
        WaitingToStart,
        GamePlaying,
        GameOver,
    }
    
    private GameState gameState;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        SetGameState(GameState.GamePlaying);
    }
    
    public void StartGame(){
        SetGameState(GameState.GamePlaying);
    }

    public void EndGame()
    {
        SetGameState(GameState.GameOver);
        OnEndGame?.Invoke();
    }

    public void RestartGame()
    {
        if (SceneSwitcher.Instance)
        {
            SceneSwitcher.Instance.RestartCurrentScene();
        }
        else
        {
            SetGameState(GameState.GamePlaying);
            OnRestartGame?.Invoke();
        }
    }
    
    public GameState GetGameState()
    {
        return gameState;
    }
    
    public void SetGameState(GameState newState)
    {
        gameState = newState;
    }
}
