using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerA : MonoBehaviour
{
    private Image image;
    [SerializeField] private GameManager gameManager;
    private Vector3 pos;
    private Vector3 pos2;
    private Animator animator;
    void Start()
    {
        image = GetComponent<Image>();
        pos = transform.position;
        pos2 = new Vector3 (pos.x, pos.y - 0.3f, pos.z);
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        //フェーズ毎の画像制御
        if (gameManager.GetComponent<GameManager>().winner == 0)
        {
            if (gameManager.GetComponent<GameManager>().phase == GameManager.ePhase.PLAYER1TURN)
            {
                image.color = new Color32(255, 255, 255, 255);
                transform.position = pos;
            }
            else
            {
                image.color = new Color32(100, 100, 100, 255);
                transform.position = pos2;
            }
        }
        else
        {
            image.color = new Color32(255, 255, 255, 255);
            transform.position = pos;
            animator.SetInteger("winner", gameManager.GetComponent<GameManager>().winner);
        }
    }
    public void SetUltAniA()
    {
        animator.SetBool("ult", true);
    }
    public void SetDefaultAniA()
    {
        animator.SetBool("ult", false);
    }
}
