using UnityEngine;

public class MainHelp : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject helpPannel;
    public void OnClick()
    {
        gameManager.GetComponent<GameManager>().Pause();
        helpPannel.SetActive(true);
    }
}

