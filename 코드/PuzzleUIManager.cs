using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIManager : MonoBehaviour
{
    [Header("UI Components")]
    public Text targetScoreText; // ��ǥ ���� �ؽ�Ʈ
    public Text pointText;       // ���� ���� �ؽ�Ʈ
    public GameObject gameClearPanel; // ���� Ŭ���� �г�

    // ���� ������ UI�� ������Ʈ
    public void SetPointUI(int point)
    {
        if (pointText != null)
        {
            pointText.text = "Score: " + point;
        }
    }

    // ��ǥ ������ UI�� ������Ʈ
    public void SetTargetScoreUI(int targetScore)
    {
        if (targetScoreText != null)
        {
            targetScoreText.text = "Target: " + targetScore;
        }
    }

    // ���� Ŭ���� UI Ȱ��ȭ
    public void ShowGameClearUI()
    {
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
    }
}