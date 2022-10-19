using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject[,] switchGrid = new GameObject[6,6];
    [SerializeField] GameObject[] switches;
    MoneyHandler moneyHandler;
    
    private void Awake() 
    {
        CreateSwitchGrid();
    }

    private void Start() 
    {
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
        moneyHandler.CurrentMoney = 0;
        if(switchGrid[0, 5].GetComponent<Switch>().isPlaceable == true) { return; }

        for(int row = 0; row < switchGrid.GetLength(0); row++)
        {
            for(int col = switchGrid.GetLength(1) - 1; col > -1; col--)
            {
                switchGrid[row, col].GetComponent<Switch>().isVisited = false;
            }
        }

        Explore(switchGrid, 0, 5);
    }

    private void Explore(GameObject[,] swithcGrid, int row, int col)
    {
        if(!(row >= 0) || !(row < swithcGrid.GetLength(0))) { return; }
        if(!(col >= 0) || !(col < swithcGrid.GetLength(1))) { return; }

        Switch _switch = switchGrid[row, col].GetComponent<Switch>();

        if(_switch.isVisited) { return; }
        if(_switch.isPlaceable) { return; }

        _switch.isVisited = true;
        moneyHandler.CurrentMoney += _switch.value;

        Explore(switchGrid, row + 1, col);
        Explore(switchGrid, row - 1, col);
        Explore(switchGrid, row, col + 1);
        Explore(switchGrid, row, col - 1);
    }
    
}
