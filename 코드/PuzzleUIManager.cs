using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIManager : MonoBehaviour
{
    [Header("UI Components")]
    public Text targetScoreText; // 목표 점수 텍스트
    public Text pointText;       // 현재 점수 텍스트
    public GameObject gameClearPanel; // 게임 클리어 패널

    // 현재 점수를 UI에 업데이트
    public void SetPointUI(int point)
    {
        if (pointText != null)
        {
            pointText.text = "Score: " + point;
        }
    }

    // 목표 점수를 UI에 업데이트
    public void SetTargetScoreUI(int targetScore)
    {
        if (targetScoreText != null)
        {
            targetScoreText.text = "Target: " + targetScore;
        }
    }

    // 게임 클리어 UI 활성화
    public void ShowGameClearUI()
    {
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
    }
}