using UnityEngine;
using UnityEngine.SceneManagement;

public class Level0 : MonoBehaviour
{
    public void OnClick()
    {
        TitleManager.SetIsAi(true);
        TitleManager.SetLevel(0);
        Initiate.Fade("Main", Color.black, 2.0f);
    }
}
