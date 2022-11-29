using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public static int level = 0;
    public static bool isAi = false;
    public static void SetLevel(int num)
    {
        level = num;
    }
    public static int GetLevel()
    {
        return level;
    }
    public static void SetIsAi(bool num)
    {
        isAi = num;
    }
    public static bool GetIsAi()
    {
        return isAi;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
