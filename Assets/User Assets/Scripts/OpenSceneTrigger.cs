using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenSceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private PlayerControllerActions player;

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponentInParent<PlayerControllerActions>();
        if (player == null) return;

        UnityEngine.Debug.Log("Player entered door trigger");

        player.InteractPressed += OnInteract;
    }

    private void OnTriggerExit(Collider other)
    {
        var exitingPlayer = other.GetComponentInParent<PlayerControllerActions>();
        if (exitingPlayer == player)
        {
            player.InteractPressed -= OnInteract;
            player = null;
        }
    }

    private void OnDisable()
    {
        if (player != null)
            player.InteractPressed -= OnInteract;
    }

    private void OnInteract()
    {
        UnityEngine.Debug.Log("Interact pressed → loading scene");
        SceneManager.LoadScene(sceneToLoad);
    }
}
