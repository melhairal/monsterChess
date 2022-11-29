using UnityEngine;

public class BackHelp : MonoBehaviour
{
    [SerializeField] private GameObject helpPannel;
    [SerializeField] private GameObject backPannel;
    public void OnClick()
    {
        helpPannel.SetActive(false);
        backPannel.SetActive(true);
    }
}
