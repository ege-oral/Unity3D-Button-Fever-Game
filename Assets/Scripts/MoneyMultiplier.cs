using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyMultiplier : MonoBehaviour
{
    
    [SerializeField] GameObject[] moneyMultipliers;
    GridManager gridManager;

    [SerializeField] Material defaultMaterial;
    [SerializeField] Material fullMaterial;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    void Update()
    {
        CheckIfRowFull();
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
                    moneyMultipliers[row].gameObject.GetComponent<Renderer>().material = fullMaterial;
                }
                else
                {
                    moneyMultipliers[row].gameObject.GetComponent<Renderer>().material = defaultMaterial;
                }
            }
        }
    }
}
