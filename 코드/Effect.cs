using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    //����Ʈ �������� ���� �ı�
    public void EndEffect()
    {
        Destroy(gameObject);
    }

    //����Ʈ �������� �θ� �ı�
    public void EndEffectParent()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
