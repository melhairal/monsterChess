using UnityEngine;

public class SoloPlay : MonoBehaviour
{
    [SerializeField, Header("レベル選択画面")] private GameObject levelSelect;
    public void OnClick()
    {
        levelSelect.SetActive(true);
    }
}
