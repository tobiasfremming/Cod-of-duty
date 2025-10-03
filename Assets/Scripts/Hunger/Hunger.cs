

using UnityEngine;


public class Hunger : MonoBehaviour
{
    [SerializeField] private float initialHunger = 100f;
    private float maxHunger = 100f;

    [SerializeField] private float hungerDecreaseRate = 1f;
    private float currentHunger;
   
    void Start()
    {
        GameStateManager.Instance.OnRestartGame += GameStateManager_OnRestartGame;
        maxHunger = initialHunger;
        currentHunger = initialHunger;
    }

    void Update()
    {
        if (GameStateManager.Instance.GetGameState() == GameStateManager.GameState.GamePlaying)
        {
            currentHunger -= hungerDecreaseRate * Time.deltaTime;
        }
        
        if(currentHunger <= 0)
        {
            Starved();
        }
    }

    public void IncreaseHunger(float amount)
    {
        currentHunger += amount;
    }

    public void IncreaseMaxHunger(float amount)
    {
        maxHunger += amount;
        currentHunger += amount;
    }
    
    public float GetHunger()
    {
        return currentHunger;
    }

    private void Starved()
    {
       GameStateManager.Instance.EndGame();
    }
    
    private void GameStateManager_OnRestartGame()
    {
        maxHunger = initialHunger;
        currentHunger = initialHunger;
    }
}
