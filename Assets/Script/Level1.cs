using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1 : MonoBehaviour
{
    public void OnClick()
    {
        TitleManager.SetIsAi(true);
        TitleManager.SetLevel(1);
        Initiate.Fade("Main", Color.black, 2.0f);
    }
}
