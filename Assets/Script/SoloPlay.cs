using UnityEngine;

public class SoloPlay : MonoBehaviour
{
    [SerializeField, Header("���x���I�����")] private GameObject levelSelect;
    public void OnClick()
    {
        levelSelect.SetActive(true);
    }
}
