using UnityEngine;

public class ActivateCanvasOnClick : MonoBehaviour
{
    [SerializeField] GameObject dialogToActivate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnMouseDown() {
        if (dialogToActivate == null)
            return;
        dialogToActivate.SetActive(true);
    }
    
        
    
}
