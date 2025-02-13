using UnityEngine;
using UnityEngine.EventSystems; // Needed if you use UI raycasting

public class BuildingPlacer : MonoBehaviour
{
    public GridManager gridManager;
    public CardManager cardManager;

    // References to the building prefabs (assign in Inspector)
    public GameObject housePrefab;
    public GameObject factoryPrefab;

    void Update()
    {
        // On left mouse button click, attempt to place building
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Convert mouse position to world coordinates
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            // Raycast to check which grid cell was clicked
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                GridCell cell = hit.collider.GetComponent<GridCell>();
                if (cell != null && !cell.isOccupied)
                {
                    PlaceBuilding(cell);
                }
            }
        }
    }
    void PlaceBuilding(GridCell cell)
    {
        // Check if there is a current card drawn
        if (cardManager.currentCard == null)
        {
            Debug.Log("No card drawn!");
            return;
        }

        // Ensure the cell is not already occupied
        if (cell.isOccupied)
        {
            Debug.Log("Cell is already occupied!");
            return;
        }

        // Mark the cell as occupied
        cell.SetOccupied(true);

        // Determine the building type from the card
        BuildingType buildingType = (BuildingType)cardManager.currentCard.buildingType;
        GameObject buildingPrefab = null;

        if (buildingType == BuildingType.House)
        {
            buildingPrefab = housePrefab;
        }
        else if (buildingType == BuildingType.Factory)
        {
            buildingPrefab = factoryPrefab;
        }

        // Instantiate the building prefab at the cell position
        if (buildingPrefab != null)
        {
            Instantiate(buildingPrefab, cell.transform.position, Quaternion.identity, cell.transform);
        }

        // 🏭 Factory Rule: Occupy all available adjacent cells
        if (buildingType == BuildingType.Factory)
        {
            gridManager.UpdateAdjacentCells(cell.x, cell.y, buildingType);
        }

        // Update UI and reset the card
        cardManager.currentCard = null;
        if (cardManager.cardText != null)
        {
            cardManager.cardText.text = "Draw a card!";
        }
    }


    //void PlaceBuilding(GridCell cell)
    //{
    //    // Check if there is a current card drawn
    //    if (cardManager.currentCard == null)
    //    {
    //        Debug.Log("No card drawn!");
    //        return;
    //    }



    //    // Mark cell as occupied
    //    cell.SetOccupied(true);

    //    // Instantiate the appropriate building prefab at the cell position
    //    GameObject buildingPrefab = null;
    //    if ((int)cardManager.currentCard.buildingType == (int)BuildingType.House)
    //    {
    //        buildingPrefab = housePrefab;
    //    }
    //    else if ((int)cardManager.currentCard.buildingType == (int)BuildingType.Factory)
    //    {
    //        buildingPrefab = factoryPrefab;
    //    }

    //    if (buildingPrefab != null)
    //    {
    //        // Optionally, parent the building to the cell so that it stays in position
    //        Instantiate(buildingPrefab, cell.transform.position, Quaternion.identity, cell.transform);
    //    }

    //    // Update adjacent cells based on the card’s rules
    //    gridManager.UpdateAdjacentCells(cell.x, cell.y);

    //    // Optionally clear the current card so the player must draw a new one for the next move
    //    cardManager.currentCard = null;
    //    if (cardManager.cardText != null)
    //        cardManager.cardText.text = "Draw a card!";
    //}

}
