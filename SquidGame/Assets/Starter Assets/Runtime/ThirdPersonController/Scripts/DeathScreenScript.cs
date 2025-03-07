using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenScript : MonoBehaviour
{
    public GameObject DeathScreen;
    public Button ExitButton;
    public Button RestartButton;

    public static bool IsMenuOpen = false; // ���� ��� ���������� ������

    void Start()
    {
        DeathScreen.SetActive(false);
        ExitButton.onClick.AddListener(Exit);
        RestartButton.onClick.AddListener(Restart);

        LockCursor(true); // ��������� ������ � ������
    }

    public void GameOver()
    {
        DeathScreen.SetActive(true);
        LockCursor(false); // ������������ ������
        IsMenuOpen = true; // ���� ������� ? ��������� ����������
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        IsMenuOpen = false; // ���� ��������� ����� �����������
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LockCursor(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
