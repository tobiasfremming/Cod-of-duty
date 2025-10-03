using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    
    private void Start()
    {
        GameStateManager.Instance.OnEndGame += GameStateManager_OnEndGame;
        Hide();
    }

    private void GameStateManager_OnEndGame()
    {
        Show();
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
}
