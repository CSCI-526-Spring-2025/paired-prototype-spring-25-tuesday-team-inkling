using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public Button[] cardSlots; // Assign 3 UI buttons for card selection
    public Button resourceButton, houseButton, hospitalButton, farmButton, factoryButton; // Button references for different types
    private int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    public Card[] resourceCards; // Array for resource cards
    public Card[] policyCards;   // Array for policy cards

    public TextMeshProUGUI moveCountText;  // UI Text to display move count
    public int moveCount = 0;             // Move count to track the number of times cards are regenerated

    void Start()
    {
        GenerateCards();
    }

    public void GenerateCards()
    {
        if (cardSlots.Length < 3)
        {
            Debug.LogError("CardSlots array must contain at least 3 UI buttons.");
            return;
        }

        // Clear previous cards
        foreach (Button slot in cardSlots)
        {
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "";
            slot.GetComponent<Image>().color = Color.white;
        }

        int randomChoice = Random.Range(0, 2); // 0 = (2 resources, 1 policy), 1 = (1 resource, 2 policies)
        int cardCount = 0;

        if (randomChoice == 0)
        {
            // Assign 2 Resource Cards
            for (int i = 0; i < 2; i++)
            {
                int randomValue = values[Random.Range(0, values.Length)];
                AssignCard(cardSlots[cardCount], resourceButton.GetComponent<Image>().color, randomValue);
                cardCount++;
            }

            // Assign 1 Policy Card
            int policyIndex = Random.Range(0, policyCards.Length);
            int policyValue = values[Random.Range(0, values.Length)];
            if (policyIndex == 1) policyValue = values[Random.Range(4, values.Length)];
            if (policyIndex == 2) policyValue = values[Random.Range(3, values.Length)];
            if (policyIndex == 3) policyValue = values[Random.Range(2, values.Length)];


            AssignCard(cardSlots[cardCount], GetPolicyColor(policyIndex), policyValue);
        }
        else
        {
            // Assign 2 Policy Cards
            for (int i = 0; i < 2; i++)
            {
                int policyIndex = Random.Range(0, policyCards.Length);
                int policyValue = values[Random.Range(0, values.Length)];
                if (policyIndex == 1) policyValue = values[Random.Range(4, values.Length)];
                if (policyIndex == 2) policyValue = values[Random.Range(3, values.Length)];
                if (policyIndex == 3) policyValue = values[Random.Range(2, values.Length)];
                AssignCard(cardSlots[cardCount], GetPolicyColor(policyIndex), policyValue);
                cardCount++;
            }

            // Assign 1 Resource Card
            int randomValue = values[Random.Range(0, values.Length)];
            AssignCard(cardSlots[cardCount], resourceButton.GetComponent<Image>().color, randomValue);
        }

        // Increment move count and update UI
        moveCount++;
        UpdateMoveCount();
    }

    void AssignCard(Button cardSlot, Color color, int value)
    {
        Card card = cardSlot.GetComponent<Card>();
        if (card != null)
        {
            card.cardColor = color;
            card.cardValue = value;
            cardSlot.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();
            cardSlot.GetComponent<Image>().color = color;
        }
    }

    Color GetPolicyColor(int policyIndex)
    {
        switch (policyIndex)
        {
            case 0: return houseButton.GetComponent<Image>().color;  // House (brown)
            case 1: return factoryButton.GetComponent<Image>().color; // Factory (grey)
            case 2: return hospitalButton.GetComponent<Image>().color; // Hospital (red)
            case 3: return farmButton.GetComponent<Image>().color; // Farm (green)
            default: return Color.white;
        }
    }

    // Update the move count display
   public void UpdateMoveCount()
    {
        moveCountText.text = "Moves: " + moveCount;
    }
    // Function to disable card selection
    public void DisableCardSelection()
    {
        foreach (Button cardSlot in cardSlots)
        {
            cardSlot.interactable = false;
        }
    }

    // Function to enable card selection
    public void EnableCardSelection()
    {
        foreach (Button cardSlot in cardSlots)
        {
            cardSlot.interactable = true;
        }
    }
}
