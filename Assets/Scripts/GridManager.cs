using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject[,] switchGrid = new GameObject[6,6];
    [SerializeField] GameObject[] switches;
    Grabber grabber;
    MoneyHandler moneyHandler;
    
    private void Awake() 
    {
        CreateSwitchGrid();
    }

    private void Start() 
    {
        grabber = FindObjectOfType<Grabber>();
        moneyHandler = FindObjectOfType<MoneyHandler>();
    }

    private void CreateSwitchGrid()
    {
        int currentIndex = 0;
        for(int row = 0; row < switchGrid.GetLength(0); row++)
        {
            for(int col = 0; col < switchGrid.GetLength(1); col++)
            {
                switchGrid[row, col] = switches[currentIndex];
                currentIndex += 1;
            }
        }
    }

    public void FindConnectedSwitches()
    {
        moneyHandler.MoneyToBeEarned = 0;
        ResetConnections();
        
        if(switchGrid[0, 5].GetComponent<Switch>().isPlaceable == false)
        {
            // Starting from right down position (0, 5).
            ExploreGrid(switchGrid, 0, 5);
        }
    }

    private void ExploreGrid(GameObject[,] swithcGrid, int row, int col)
    {
        if(!(row >= 0) || !(row < swithcGrid.GetLength(0))) { return; }
        if(!(col >= 0) || !(col < swithcGrid.GetLength(1))) { return; }

        Switch _switch = switchGrid[row, col].GetComponent<Switch>();

        if(_switch.isVisited) { return; }
        if(_switch.isPlaceable) { return; }

        _switch.isVisited = true;
        _switch.holdingBlock.GetComponent<Block>().isInPath = true;
        grabber.ChangeBlockMaterial(_switch.holdingBlock, _switch.holdingBlock.GetComponent<Block>().blockMaterial);
        moneyHandler.MoneyToBeEarned += _switch.value;

        ExploreGrid(switchGrid, row + 1, col);
        ExploreGrid(switchGrid, row - 1, col);
        ExploreGrid(switchGrid, row, col + 1);
        ExploreGrid(switchGrid, row, col - 1);
    }
    
    private void ResetConnections()
    {
        for(int row = 0; row < switchGrid.GetLength(0); row++)
        {
            for(int col = switchGrid.GetLength(1) - 1; col > -1; col--)
            {
                switchGrid[row, col].GetComponent<Switch>().isVisited = false;
                if(switchGrid[row, col].GetComponent<Switch>().holdingBlock != null)
                {
                    switchGrid[row, col].GetComponent<Switch>().holdingBlock.GetComponent<Block>().isInPath = false;
                }
            }
        }
    }
}
