using UnityEngine;

public class HelpButton : MonoBehaviour
{
    [SerializeField] private GameObject heloPannel;
    public void OnClick()
    {
        heloPannel.SetActive(true);
    }
}