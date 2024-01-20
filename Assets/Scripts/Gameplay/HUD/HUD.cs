using UnityEngine;

public class HUD : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        GameManager.OnGameEnded += OnGameOver;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnGameEnded -= OnGameOver;
    }

    private void OnGameOver()
    {
        gameObject.SetActive(false);
    }
}