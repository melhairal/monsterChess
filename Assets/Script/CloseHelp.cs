using UnityEngine;

public class CloseHelp : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject helpPannel;
    public void OnClick()
    {
        if(gameManager != null) gameManager.GetComponent<GameManager>().Pause();
        helpPannel.SetActive(false);
    }
}