using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameCanvasManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberOfLivesText;
    [SerializeField] private TextMeshProUGUI turnInfo;
    [SerializeField] private TextMeshProUGUI infoMessage;
    [SerializeField] private GameObject endTurnBtn;

    private void Start()
    {
        StartChecks();
    }

    private void StartChecks()
    {
        if(numberOfLivesText == null)
            Debug.LogError("CanvasManagerGame: Number of lives not set in the inspector, make sure it's linked in the inspector!");
        if(turnInfo == null)
            Debug.LogError("CanvasManagerGame: Turn info not set in the inspector, make sure it's linked in the inspector!");
        if(endTurnBtn == null)
            Debug.LogError("CanvasManagerGame: End turn button not set in the inspector, make sure it's linked in the inspector!");
    }

    public void SetLives(int nrOfLives)
    {
        numberOfLivesText.text = nrOfLives.ToString();
    }

    public void AddListenerToEndTurnButton(UnityAction action)
    {
        endTurnBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        endTurnBtn.GetComponent<Button>().onClick.AddListener(action);
    }

    public void ToggleEndTurnBtn(bool hidden)
    {
        endTurnBtn.SetActive(!hidden);
    }

    public void SetTurnText(string playerName)
    {
        turnInfo.text = "Current Turn\n" + playerName;
    }

    public void HideInfoMessage()
    {
        infoMessage.text = "";
    }

    public void InfoMessageDrawCard(int amount)
    {
        infoMessage.text = "Draw " + amount + " cards";
    }
    
    public void InfoMessageDiscardCard(int amount)
    {
        infoMessage.text = "Discard or play " + amount + " cards first";
    }

    public void InfoActionCard()
    {
        infoMessage.text = "Select the target or use the cancel button do stop the action";
    }
}
