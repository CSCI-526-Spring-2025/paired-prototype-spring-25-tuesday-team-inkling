using UnityEngine;
using UnityEngine.UI;

public class RulesPanelManager : MonoBehaviour
{
    public GameObject rulesPanel;  // Assign the Rules Panel in Inspector
    public Button rulesButton;     // Assign the Rules Button in Inspector
    public Button closeButton;     // Assign the Close Button in Inspector

    void Start()
    {
        // Ensure the rules panel is hidden at the start
        rulesPanel.SetActive(false);

        // Add click listeners to buttons
        rulesButton.onClick.AddListener(ShowRules);
        closeButton.onClick.AddListener(HideRules);
    }

    void ShowRules()
    {
        rulesPanel.SetActive(true);
    }

    void HideRules()
    {
        rulesPanel.SetActive(false);
    }
}
