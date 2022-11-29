using UnityEngine;
using UnityEngine.SceneManagement;

public class GoTitle : MonoBehaviour
{
    public void OnClick()
    {
        Initiate.Fade("Title", Color.black, 2.0f);
    }
}
