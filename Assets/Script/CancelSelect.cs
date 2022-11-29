using UnityEngine;

public class CancelSelect : MonoBehaviour
{
    [SerializeField, Header("ƒŒƒxƒ‹‘I‘ð‰æ–Ê")] private GameObject levelSelect;
    public void OnClick()
    {
        levelSelect.SetActive(false);
    }
}
