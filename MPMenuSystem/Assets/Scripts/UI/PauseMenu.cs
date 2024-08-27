using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] InputAction pause;
    public static bool GameIsPaused = false;

    [SerializeField] private GameObject lobbyCodeObject;
    [SerializeField] private GameObject leaveGameObject;


    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        pause.Enable();
    }

    private void OnDisable()
    {
        pause.Disable();
    }

    private void Pause()
    {
        var wasPressed = pause.triggered && pause.ReadValue<float>() > 0;

        if (wasPressed)
        {
            if (GameIsPaused)
            {
                RemovePauseScreen();
            }
            else
            {
                ShowPauseScreen();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Pause();
    }

    private void ShowPauseScreen()
    {
        leaveGameObject?.SetActive(true);
        lobbyCodeObject?.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameIsPaused = true;
        Debug.Log("Pause");
    }
    private void RemovePauseScreen()
    {
        leaveGameObject?.SetActive(false);
        lobbyCodeObject?.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        GameIsPaused = false;
        Debug.Log("Resume");
    }
}
