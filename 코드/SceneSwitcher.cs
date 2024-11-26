using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // ��ư Ŭ�� �� ȣ��� �Լ�
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
