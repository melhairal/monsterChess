using UnityEngine;

public class NextHelp : MonoBehaviour
{
    [SerializeField] private GameObject helpPannel;
    [SerializeField] private GameObject nextPannel;
    public void OnClick()
    {
        helpPannel.SetActive(false);
        nextPannel.SetActive(true);
    }
}
