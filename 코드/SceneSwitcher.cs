using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // 버튼 클릭 시 호출될 함수
    public void LoadEasyScene()
    {
        SceneManager.LoadScene("EasyScene");
    }

    public void LoadNormalScene()
    {
        SceneManager.LoadScene("NormalScene");
    }

    public void LoadHardScene()
    {
        SceneManager.LoadScene("HardScene");
    }
}
