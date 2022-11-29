using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public void OnClick()
    {
        gameManager.GetComponent<GameManager>().Pause();
    }
}

