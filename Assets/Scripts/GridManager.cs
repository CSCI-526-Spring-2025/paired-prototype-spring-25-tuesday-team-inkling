using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject gridPrefab;  // Assign a UI Button prefab
    public Transform gridParent;   // Assign the parent panel (Grid container)
    public int gridSize = 5;       // Default grid size 5x5
    private GameObject[,] gridCells;

    public TextMeshProUGUI totalMoneyText;
    private int totalMoney = 0;

    private Button selectedCard; // Stores the selected card
    public Button[] cardSlots;   // UI Buttons for card selection

    public CardManager cardManager; // Reference to CardManager

    private int cardsPlayed = 0;
    private int factoryIncome = 0;
    private const int maxCardsPerTurn = 2; // Maximum cards per turn

    public Button regenerateButton;  // Button to regenerate cards manually

    private List<GameObject> farm = new List<GameObject>(); // Track factories

    private List<GameObject> factories = new List<GameObject>(); // Track factories
    private HashSet<GameObject> blockedCells = new HashSet<GameObject>(); // Track blocked cells

    void Start()
    {
        GenerateGrid();
        UpdateTotalMoney();
        regenerateButton.onClick.AddListener(RegenerateCards);
    }

    void GenerateGrid()
    {
        gridCells = new GameObject[gridSize, gridSize];

        GridLayoutGroup gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = gridParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // Set up Grid Layout parameters
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize;
        gridLayout.cellSize = new Vector2(100, 100);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.padding = new RectOffset(5, 5, 5, 5);

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject newCell = Instantiate(gridPrefab, gridParent);
                newCell.GetComponent<Button>().onClick.AddListener(() => PlaceCard(newCell));
                newCell.GetComponent<Image>().color = Color.white;
                newCell.GetComponentInChildren<TextMeshProUGUI>().text = "0";
                gridCells[x, y] = newCell;
            }
        }
    }

    public void SelectCard(Button card)
    {
        selectedCard = card;
    }

    void PlaceCard(GameObject cell)
    {
        if (selectedCard == null || blockedCells.Contains(cell)) return;

        Card cardScript = selectedCard.GetComponent<Card>();
        if (cardScript == null) return;

        int cellValue = int.Parse(cell.GetComponentInChildren<TextMeshProUGUI>().text);
        Color cardColor = selectedCard.GetComponent<Image>().color;

        // House Placement Rule (UNCHANGED)
        if (cardColor == cardManager.houseButton.GetComponent<Image>().color)
        {
            if (cell.GetComponent<Image>().color == cardManager.resourceButton.GetComponent<Image>().color && cardScript.cardValue <= cellValue)
            {
                int newValue = Mathf.FloorToInt(0.3f * cellValue) + cardScript.cardValue;
                cell.GetComponentInChildren<TextMeshProUGUI>().text = newValue.ToString();
                cell.GetComponent<Image>().color = cardColor;
                totalMoney += newValue;
            }
            else
            {
                Debug.Log("House can only be placed on a Resource with equal or higher value.");
                return;
            }
        }
        // Factory Placement Rule
        else if (cardColor == cardManager.factoryButton.GetComponent<Image>().color)
        {
            if (cell.GetComponent<Image>().color == cardManager.resourceButton.GetComponent<Image>().color)
            {
                // Factory on Resource
                int newValue = Mathf.FloorToInt(0.5f * cellValue) + cardScript.cardValue;
                cell.GetComponentInChildren<TextMeshProUGUI>().text = newValue.ToString();
                totalMoney += newValue;
            }
            else
            {
                // Factory on Empty Cell - Block Adjacent Empty Cells
                BlockAdjacentCells(cell);

                // Apply Factory value to the empty cell
                cell.GetComponentInChildren<TextMeshProUGUI>().text = cardScript.cardValue.ToString();
            }

            cell.GetComponent<Image>().color = cardColor;
            factories.Add(cell); // Track factories for income
        }
        // Farm Placement Rule
        else if (cardColor == cardManager.farmButton.GetComponent<Image>().color)
        {
            if (cell.GetComponent<Image>().color == cardManager.resourceButton.GetComponent<Image>().color)
            {
                // Farm on Resource
                int newValue = Mathf.FloorToInt(0.1f * cellValue) + cardScript.cardValue;
                cell.GetComponentInChildren<TextMeshProUGUI>().text = newValue.ToString();
                totalMoney += newValue;
            }
            else
            {
                // Apply Farm value to the empty cell
                cell.GetComponentInChildren<TextMeshProUGUI>().text = cardScript.cardValue.ToString();
            }

            cell.GetComponent<Image>().color = cardColor;
            farm.Add(cell); // Track factories for income
        }
        // Hospital Placement Rule
        else if (cardColor == cardManager.hospitalButton.GetComponent<Image>().color)
        {
            if (cell.GetComponent<Image>().color == cardManager.resourceButton.GetComponent<Image>().color && cardScript.cardValue <= cellValue)
            {
                // Hospital can replace the resource card
                cell.GetComponentInChildren<TextMeshProUGUI>().text = cardScript.cardValue.ToString();
                cell.GetComponent<Image>().color = cardColor;  // Set the hospital color
                totalMoney += cardScript.cardValue;
            }
            else
            {
                Debug.Log("Hospital can only be placed on a Resource with equal or higher value.");
                return;
            }
        }
        else
        {
            // Default placement
            cell.GetComponent<Image>().color = cardColor;
            cell.GetComponentInChildren<TextMeshProUGUI>().text = cardScript.cardValue.ToString();
            totalMoney += cardScript.cardValue;
        }

        UpdateTotalMoney();

        // Remove card from selection panel
        selectedCard.GetComponentInChildren<TextMeshProUGUI>().text = "";
        selectedCard.GetComponent<Image>().color = Color.white;
        selectedCard = null;

        cardsPlayed++;

        // If 1 or 2 cards are played, generate new cards
        if (cardsPlayed >= maxCardsPerTurn)
        {
            GenerateFactoryIncome(); // Factory income every turn
            cardManager.GenerateCards();
            cardsPlayed = 0; // Reset for the next turn
        }
    }



    //void UpdateTotalMoney()
    //{
    //    totalMoneyText.text = "Total Money: " + totalMoney;
    //}
    void UpdateTotalMoney()
    {
        totalMoney = 0; // Reset totalMoney before recalculating it.

        // Sum up the values of all grid cells.
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int cellValue = int.Parse(gridCells[x, y].GetComponentInChildren<TextMeshProUGUI>().text);
                totalMoney += cellValue;  // Add cell value to total money
            }
        }
        totalMoney = totalMoney + factoryIncome;
        totalMoneyText.text = "Total Money: " + totalMoney;
    }


    void RegenerateCards()
    {
        cardManager.GenerateCards();
    }

    void GenerateFactoryIncome()
    {
        
        foreach (GameObject factory in factories)
        {
            int factoryValue = int.Parse(factory.GetComponentInChildren<TextMeshProUGUI>().text);
            int income = Mathf.FloorToInt(0.3f * factoryValue);
            factoryIncome += income;
        }
        foreach (GameObject farm in farm)
        {
            int farmValue = int.Parse(farm.GetComponentInChildren<TextMeshProUGUI>().text);
            int income = Mathf.FloorToInt(0.2f * farmValue);
            factoryIncome += income;
        }
        UpdateTotalMoney();
    }

    void BlockAdjacentCells(GameObject factoryCell)
    {
        int factoryX = -1, factoryY = -1;

        // Find the position of the factory in the grid
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (gridCells[x, y] == factoryCell)
                {
                    factoryX = x;
                    factoryY = y;
                    break;
                }
            }
            if (factoryX != -1) break;
        }

        if (factoryX == -1 || factoryY == -1) return; // Factory not found

        // Check adjacent positions and block empty cells
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int adjX = factoryX + dx[i];
            int adjY = factoryY + dy[i];

            if (adjX >= 0 && adjX < gridSize && adjY >= 0 && adjY < gridSize)
            {
                GameObject adjacentCell = gridCells[adjX, adjY];

                if (adjacentCell.GetComponent<Image>().color == Color.white) // Only block empty cells
                {
                    adjacentCell.GetComponent<Image>().color = Color.gray;
                    blockedCells.Add(adjacentCell);
                }
            }
        }
    }
}
