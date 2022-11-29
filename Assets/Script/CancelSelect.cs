using UnityEngine;

public class CancelSelect : MonoBehaviour
{
    [SerializeField, Header("レベル選択画面")] private GameObject levelSelect;
    public void OnClick()
    {
        levelSelect.SetActive(false);
    }
}
