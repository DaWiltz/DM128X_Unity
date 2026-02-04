using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EscapeToCorridor : MonoBehaviour
{
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private string corridorSceneName = "Korridor";

    private void OnEnable()
    {
        backAction.action.Enable();
        Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
 
}

    private void Update()
    {
        if (backAction.action.WasPressedThisFrame())
        {
            SceneManager.LoadScene(corridorSceneName);
        }
    }
}
