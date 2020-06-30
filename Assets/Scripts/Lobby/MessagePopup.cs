using UnityEngine;
using UnityEngine.UI;

public class MessagePopup : MonoBehaviour
{
    [SerializeField] private Button closeBtn;
    
    void Start()
    {
        if (closeBtn == null)
        {
            Debug.LogError("Close Button not set in the inspector!");
            return;
        }
            
        closeBtn.onClick.AddListener(CanvasManager.Instance.HidePopup);
    }
    
}
