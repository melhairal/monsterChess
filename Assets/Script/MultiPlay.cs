using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiPlay : MonoBehaviour
{
    public void OnClick()
    {
        TitleManager.SetIsAi(false);
        Initiate.Fade("Main", Color.black, 2.0f);
    }
}
