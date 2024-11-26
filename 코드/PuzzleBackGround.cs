using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PuzzleBackGround : MonoBehaviour
{
    [SerializeField]
    private Image image;


    //�ʱ�ȭ - ��ġ, ��������Ʈ ����
    public void Init(Vector2 pos, Sprite spr)
    {
        image.rectTransform.anchoredPosition = pos;
        image.sprite = spr;
    }


}
