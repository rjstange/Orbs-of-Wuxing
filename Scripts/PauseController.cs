using UnityEngine;

public class PauseController : MonoBehaviour
{
    private bool paused = false;
    public GameObject pauseMenu;
    public GameObject pauseInstructions;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && paused)
        {
            Unpause();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Pause();
        }
    }

    private void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        pauseInstructions.SetActive(false);
    }

    private void Unpause()
    {
        paused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        pauseInstructions.SetActive(true);
    }
}
