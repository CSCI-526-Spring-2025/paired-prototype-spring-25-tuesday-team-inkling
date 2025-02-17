using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public GameObject eventPanel;       // Reference to the Event UI Panel
    public TextMeshProUGUI eventText;   // Event description text
    public Button choice1Button;        // "Let the people suffer"
    public Button choice2Button;        // "Follow the Event Rule"

    private GridManager gridManager;
    private CardManager cardManager;

    private List<EventData> eventList = new List<EventData>();
    public int movesCounter = 0;   // Track total moves played
    public int nextEventMove = 5;  // Define when the next event will occur
    private EventData currentEvent;

    public Button nextButton; // Assign in Inspector

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        cardManager = FindObjectOfType<CardManager>();

        // Hide event panel at start
        eventPanel.SetActive(false);

        // Assign button listeners
        choice1Button.onClick.AddListener(ApplyChoice1);
        choice2Button.onClick.AddListener(ApplyChoice2);

        // Define event list with random events
        PopulateEvents();
    }

    void PopulateEvents()
    {        
        eventList.Add(new EventData("Economic Crisis! Lose money:", 1, Random.Range(10, 15)));
        eventList.Add(new EventData("Natural Disaster! Lose 40-60% of houses. Percent is: ", 2, Random.Range(40, 60)));
        eventList.Add(new EventData("Financial Crash! Lose 2-5 grid value. Number is: ", 3, Random.Range(2, 5)));
        eventList.Add(new EventData("Medical Crisis! Lose 2-4 value from house. Number is: ", 4, Random.Range(2, 4)));

    }

    public void IncrementMove()
    {
        movesCounter++;

        if (movesCounter >= nextEventMove)
        {
            TriggerEvent();
            nextEventMove += Random.Range(4, 6); // Schedule next event after 5 moves
        }
    }

    void TriggerEvent()
    {
        // Disable card selection during the event
        cardManager.DisableCardSelection();

        // Disable the Next button
        if (nextButton != null)
        {
            nextButton.interactable = false;
        }

        // Pick a random event
        EventData selectedEvent = eventList[Random.Range(0, eventList.Count)];
        eventText.text = selectedEvent.description + " " + selectedEvent.value;

        // Store event type for processing
        currentEvent = selectedEvent;

        // Show event panel
        eventPanel.SetActive(true);
    }

    void ApplyChoice1()
    {
        //if (currentEvent.type == 2) // Remove 50% of houses
        //{
            gridManager.RemoveHouses(20);
            gridManager.ReduceFactoryValues();
            gridManager.SubtractHospitalGridValue(1);
            gridManager.SubtractGridValue(2);
        //}

        // Hide event panel and resume game
        ResumeGame();
    }

    void ApplyChoice2()
    {
        if (currentEvent.type == 1) gridManager.SubtractMoney(-currentEvent.value);
        else if (currentEvent.type == 2)
        {
            gridManager.RemoveHouses(currentEvent.value);
        }
        else if (currentEvent.type == 3) gridManager.SubtractGridValue(currentEvent.value);
        else if (currentEvent.type == 4) gridManager.SubtractHospitalGridValue(currentEvent.value);


        // Hide event panel and resume game
        ResumeGame();
    }

    void ResumeGame()
    {

        // Disable the Next button
        if (nextButton != null)
        {
            nextButton.interactable = true;
        }


        eventPanel.SetActive(false);
        cardManager.EnableCardSelection();
    }
}

public class EventData
{
    public string description;
    public int type;
    public int value;


    public EventData(string desc, int t, int val)
    {
        description = desc;
        type = t;
        value = val; 
    }
}
