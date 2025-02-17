using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridManager : MonoBehaviour
{
    public GameObject gridPrefab;  // Assign a UI Button prefab
    public Transform gridParent;   // Assign the parent panel (Grid container)
    public int gridSize = 5;       // Default grid size 5x5
    private GameObject[,] gridCells;

    public GameObject gameOverUI;  


    public TextMeshProUGUI totalMoneyText;
    private int totalMoney = 0;

    private Button selectedCard; // Stores the selected card
    public Button[] cardSlots;   // UI Buttons for card selection

    public CardManager cardManager; // Reference to CardManager

    private int cardsPlayed = 0;
    private int factoryIncome = 0;
    private int subtractAmount = 0;

    private EventManager eventManager;


    private const int maxCardsPerTurn = 2; // Maximum cards per turn

    public Button regenerateButton;  // Button to regenerate cards manually

    private List<GameObject> farm = new List<GameObject>(); // Track farms
    private List<GameObject> factories = new List<GameObject>(); // Track factories
    private HashSet<GameObject> blockedCells = new HashSet<GameObject>(); // Track blocked cells

    void Start()
    {
        if (gameOverUI == null)
        {
            gameOverUI = GameObject.Find("GameOverUI");  // Finds UI by name
            if (gameOverUI == null)
            {
                Debug.LogError("GameOverUI not found in the scene! Check if it's active in Hierarchy.");
            }
        }
        //GameObject gameOverUI = GameObject.Find("GameOverUI"); // Assuming you have a UI panel named "GameOverUI"
        if (gameOverUI != null)
        {
            Debug.Log("Active21");
            gameOverUI.SetActive(false); // Show the Game Over UI
            Debug.Log("Active2");
        }
        GenerateGrid();
        UpdateTotalMoney();
        regenerateButton.onClick.AddListener(RegenerateCards);
    }

    void GenerateGrid()
    {
        if (gameOverUI == null)
        {
            gameOverUI = GameObject.Find("GameOverUI");  // Finds UI by name
            if (gameOverUI == null)
            {
                Debug.LogError("GameOverUI not found in the scene! Check if it's active in Hierarchy.");
            }
        }
        //GameObject gameOverUI = GameObject.Find("GameOverUI"); // Assuming you have a UI panel named "GameOverUI"
        if (gameOverUI != null)
        {
            Debug.Log("Active21");
            gameOverUI.SetActive(false); // Show the Game Over UI
            Debug.Log("Active2");
        }

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
        if (selectedCard == null || blockedCells.Contains(cell))
            return;

        // Retrieve colors for comparison
        Color cardColor = selectedCard.GetComponent<Image>().color;
        Color cellColor = cell.GetComponent<Image>().color;
        Color resourceColor = cardManager.resourceButton.GetComponent<Image>().color;
        Color houseColor = cardManager.houseButton.GetComponent<Image>().color;
        Color factoryColor = cardManager.factoryButton.GetComponent<Image>().color;
        Color farmColor = cardManager.farmButton.GetComponent<Image>().color;
        Color hospitalColor = cardManager.hospitalButton.GetComponent<Image>().color;

        // NEW CHECK: Prevent building cards (farm, factory, house, hospital)
        // from being placed on top of one another.
        if ((cardColor == houseColor || cardColor == factoryColor ||
             cardColor == farmColor || cardColor == hospitalColor)
             && (cellColor != Color.white && cellColor != resourceColor))
        {
            Debug.Log("Cannot place this building card on top of an existing building card.");
            return;
        }

        // NEW RULE:
        // If the cell was previously generating income as a factory or farm,
        // remove it from the corresponding list so it no longer generates money.
        factories.Remove(cell);
        farm.Remove(cell);

        Card cardScript = selectedCard.GetComponent<Card>();
        if (cardScript == null)
            return;

        int cellValue = int.Parse(cell.GetComponentInChildren<TextMeshProUGUI>().text);

        // House Placement Rule
        if (cardColor == houseColor)
        {
            if (cellColor == resourceColor && cardScript.cardValue <= cellValue)
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
        else if (cardColor == factoryColor)
        {
            if (cellColor == resourceColor)
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
        else if (cardColor == farmColor)
        {
            if (cellColor == resourceColor)
            {

                // Farm on Resource
                int newValue = Mathf.Abs(Mathf.FloorToInt(0.5f * cellValue) - cardScript.cardValue);
                cell.GetComponentInChildren<TextMeshProUGUI>().text = newValue.ToString();
                totalMoney += newValue;
            }
            else
            {
                // Apply Farm value to the empty cell
                cell.GetComponentInChildren<TextMeshProUGUI>().text = cardScript.cardValue.ToString();
            }
            cell.GetComponent<Image>().color = cardColor;
            farm.Add(cell); // Track farms for income
        }
        // Hospital Placement Rule
        else if (cardColor == hospitalColor)
        {
            if (cellColor == resourceColor && cardScript.cardValue <= cellValue)
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
        // New Rule: Resource card placed over an existing Resource card
        else if (cardColor == resourceColor && cellColor == resourceColor)
        {
            // Sum the values of the existing resource and the newly placed resource card
            int newValue = cellValue + cardScript.cardValue;
            cell.GetComponentInChildren<TextMeshProUGUI>().text = newValue.ToString();
            // The cell color remains as the resource card color.
        }
        else
        {
            // Default placement for any other cards or when placing a resource card on a non-resource cell.
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

        // After maxCardsPerTurn, generate factory income and new cards.
        if (cardsPlayed >= maxCardsPerTurn)
        {
            GenerateFactoryIncome();
            cardManager.GenerateCards();
            EventManager eventManager = FindObjectOfType<EventManager>();
            eventManager.IncrementMove();
            cardsPlayed = 0;
        }
    }

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
        totalMoney = totalMoney + factoryIncome + subtractAmount;
        totalMoneyText.text = "Total Money: " + totalMoney;

        // Check for game over
        if (totalMoney < 0)
        {
            Debug.Log("Active");
            GameOver();
        }


    }




    void RegenerateCards()
    {
        cardManager.GenerateCards();
        EventManager eventManager = FindObjectOfType<EventManager>();
        eventManager.IncrementMove();
    }

    void GenerateFactoryIncome()
    {
        int tempsum = 0;
        foreach (GameObject factory in factories)
        {
            int factoryValue = int.Parse(factory.GetComponentInChildren<TextMeshProUGUI>().text);
            int income = Mathf.FloorToInt(factoryValue);
            tempsum += income;
        }
        factoryIncome = factoryIncome + Mathf.FloorToInt(tempsum * 0.3f);
        tempsum = 0;
        foreach (GameObject farmCell in farm)
        {
            int farmValue = int.Parse(farmCell.GetComponentInChildren<TextMeshProUGUI>().text);
            int income = Mathf.FloorToInt(farmValue);
            tempsum += income;
        }
        factoryIncome = factoryIncome + Mathf.FloorToInt(tempsum * 0.2f);
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
            if (factoryX != -1)
                break;
        }

        if (factoryX == -1 || factoryY == -1)
            return; // Factory not found

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

                if (adjacentCell.GetComponent<Image>().color == Color.white)
                {
                    adjacentCell.GetComponent<Image>().color = Color.gray;
                    blockedCells.Add(adjacentCell);
                }
            }
        }
    }
    public void RemoveHouses(int number)
    {
        List<GameObject> houseCells = new List<GameObject>();
        Color houseColor = cardManager.houseButton.GetComponent<Image>().color;
        // Find all house cells
        foreach (GameObject cell in gridCells)
        {
            if (cell.GetComponent<Image>().color == houseColor)
            {
                houseCells.Add(cell);
            }
        }

        // Calculate number of houses to remove
        int percent = number; 
        int housesToRemove = Mathf.FloorToInt(houseCells.Count * percent / 100f);
        if (housesToRemove == 0) housesToRemove = 1;

        for (int i = 0; i < housesToRemove; i++)
        {
            if (houseCells.Count == 0) break;
            GameObject house = houseCells[Random.Range(0, houseCells.Count)];
            house.GetComponent<Image>().color = Color.white;
            house.GetComponentInChildren<TextMeshProUGUI>().text = "0";
            houseCells.Remove(house);
        }
    }

    public void ReduceFactoryValues()
    {
        foreach (GameObject factory in factories)
        {
            int value = int.Parse(factory.GetComponentInChildren<TextMeshProUGUI>().text);
            value = Mathf.FloorToInt(value * 0.8f); // Reduce by 20%
            factory.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();
        }
    }
    public void SubtractMoney(int subamt)
    {
        subtractAmount = subtractAmount + subamt;
        UpdateTotalMoney();
    }


    public void SubtractGridValue(int val)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject cell = gridCells[x, y];
                TextMeshProUGUI textComponent = cell.GetComponentInChildren<TextMeshProUGUI>();

                int value = int.Parse(textComponent.text);
                value -= val;

                if (value <= 0)
                {
                    value = 0; // Set to 0 instead of negative values
                    cell.GetComponent<Image>().color = Color.white; // Reset color
                }

                textComponent.text = value.ToString(); // Update displayed value
            }
        }


        UpdateTotalMoney();
    }


    void GameOver()
    {
        if (gameOverUI == null)
        {
            gameOverUI = GameObject.Find("GameOverUI");  // Finds UI by name
            if (gameOverUI == null)
            {
                Debug.LogError("GameOverUI not found in the scene! Check if it's active in Hierarchy.");
            }
        }

        //GameObject gameOverUI = GameObject.Find("GameOverUI"); // Assuming you have a UI panel named "GameOverUI"
        if (gameOverUI != null)
        {
            Debug.Log("Active21");
            gameOverUI.SetActive(true); // Show the Game Over UI
            Invoke("RegenerateGrid", 5f); // Calls ShowGameOver after 2 seconds
            Debug.Log("Active2");
        }

    }

    void RegenerateGrid()
    {
        // Destroy current grid cells and regenerate
        foreach (GameObject cell in gridCells)
        {
            Destroy(cell);
        }

        GenerateGrid();  // Regenerate the grid from scratch
        eventManager = FindObjectOfType<EventManager>();
        cardManager = FindObjectOfType<CardManager>();

        GameObject gameOverUI = GameObject.Find("GameOverUI");
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false); // Show the Game Over UI
        }
        totalMoney = 0;  // Reset total money
        subtractAmount = 0;
        cardsPlayed = 0;
        factoryIncome = 0;
        if (eventManager != null)
        {
            eventManager.movesCounter = 0;
        }
        if (cardManager != null)
        {
            cardManager.moveCount = 1;
            cardManager.UpdateMoveCount();
        }
        UpdateTotalMoney();  
    }




    //public void SubtractGridValue(int val)
    //{
    //    int amount = val;
    //    foreach (GameObject cell in gridCells)
    //    {
    //        int value = int.Parse(cell.GetComponentInChildren<TextMeshProUGUI>().text);
    //        value -= amount;
    //        if (value < 0) value = 0;
    //        cell.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();
    //    }

    //    UpdateTotalMoney();
    //}


}
