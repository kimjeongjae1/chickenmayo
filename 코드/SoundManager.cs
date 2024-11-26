using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource bgmSource;
    [SerializeField]
    private AudioSource effectsource;

    [SerializeField]
    private AudioClip[] effectClips;



    /*
     * 0 ����
     * 1 �Ϲ� ������
     * 2 ��ֹ� ������
     * 3 ���μ���
     * 4 ��ź
     * ���̾�
     * */
    public void PlayEffect(int index)
    {
        effectsource.PlayOneShot(effectClips[index]);
    }
    
    //��Ʈ���� ����Ʈ
    public void PlayPopEffect(PuzzleType type)
    {
        PlayEffect((int)type+1);
    }


}
