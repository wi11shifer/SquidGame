using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenScript : MonoBehaviour
{
    public GameObject DeathScreen;
    public Button ExitButton;
    public Button RestartButton;

    public static bool IsMenuOpen = false; // Флаг для блокировки камеры

    void Start()
    {
        DeathScreen.SetActive(false);
        ExitButton.onClick.AddListener(Exit);
        RestartButton.onClick.AddListener(Restart);

        LockCursor(true); // Блокируем курсор в начале
    }

    public void GameOver()
    {
        DeathScreen.SetActive(true);
        LockCursor(false); // Разблокируем курсор
        IsMenuOpen = true; // Меню открыто ? блокируем управление
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        IsMenuOpen = false; // Меню закроется после перезапуска
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LockCursor(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
