using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject[,] switchGrid = new GameObject[6,6];
    [SerializeField] GameObject[] switches;
    
    
    private void Awake() 
    {
        CreateSwitchGrid();
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

    
}
