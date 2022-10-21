using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyMultiplierHandler : MonoBehaviour
{
    [SerializeField] GameObject[] moneyMultipliers;
    GridManager gridManager;
    MoneyHandler moneyHandler;

    [SerializeField] Material defaultMaterial;
    [SerializeField] Material fullMaterial;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        moneyHandler = FindObjectOfType<MoneyHandler>();
    }

    public void CheckIfRowFull()
    {
        for(int row = 0; row < gridManager.switchGrid.GetLength(0); row++)
        {
            int counter = 0;
            for(int col = 0; col < gridManager.switchGrid.GetLength(1); col++)
            {
                if(gridManager.switchGrid[row, col].GetComponent<Switch>().holdingBlock != null && gridManager.switchGrid[row, col].GetComponent<Switch>().holdingBlock.GetComponent<Block>().isInPath)
                {
                    counter += 1;
                }
                if(counter == gridManager.switchGrid.GetLength(1))
                {
                    moneyHandler.moneyMultipliersValues[row] = moneyMultipliers[row].GetComponent<MoneyMultiplier>().multiplyValue;
                    moneyMultipliers[row].gameObject.GetComponent<Renderer>().material = fullMaterial;
                }
            }
            if(counter < gridManager.switchGrid.GetLength(1))
            {
                moneyHandler.moneyMultipliersValues[row] = 1f;
                moneyMultipliers[row].gameObject.GetComponent<Renderer>().material = defaultMaterial;
            }
        }
    }
}
