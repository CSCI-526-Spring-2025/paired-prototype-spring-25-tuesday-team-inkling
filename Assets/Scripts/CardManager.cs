using TMPro;
using UnityEngine;
using UnityEngine.UI; // Make sure to include this for UI components

public class CardManager : MonoBehaviour
{
    public TextMeshProUGUI cardText; // Drag your CardText here in the Inspector

    public enum CardType { House, Factory }
    [System.Serializable]
    public class Card
    {
        public CardType buildingType;
    }

    public Card currentCard;

    public void DrawCard()
    {
        // For simplicity, randomly choose a building type:
        currentCard = new Card();
        currentCard.buildingType = (Random.value > 0.5f) ? CardType.House : CardType.Factory;
        if (cardText != null)
        {
            cardText.text = "Place a " + currentCard.buildingType.ToString();
        }
    }
}
