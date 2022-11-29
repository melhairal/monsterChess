using UnityEngine;

public class SoloPlay : MonoBehaviour
{
    [SerializeField, Header("ƒŒƒxƒ‹‘I‘ð‰æ–Ê")] private GameObject levelSelect;
    public void OnClick()
    {
        levelSelect.SetActive(true);
    }
}
