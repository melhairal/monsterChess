using UnityEngine;

public class CancelSelect : MonoBehaviour
{
    [SerializeField, Header("���x���I�����")] private GameObject levelSelect;
    public void OnClick()
    {
        levelSelect.SetActive(false);
    }
}
