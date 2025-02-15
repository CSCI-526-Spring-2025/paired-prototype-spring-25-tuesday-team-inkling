using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardValue;
    public Button cardButton;
    public Color cardColor;  // Added for color reference

    void Start()
    {
        if (cardButton == null)
            Debug.LogError("Card button is NULL on " + gameObject.name);
        if (cardButton != null)
            cardButton.onClick.AddListener(() => FindObjectOfType<GridManager>().SelectCard(cardButton));
        else
            Debug.LogError("cardButton is NULL on " + gameObject.name);
    }
}
