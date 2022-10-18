using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    private GameObject selectedBlock;
    Touch touch;
    Camera mainCamera;
    private bool isPickedUp = false;

    GameObject[,] grid;
    [SerializeField] GameObject[] switches;

    GameObject nearestSwitch = null;

    void Start()
    {
        mainCamera = Camera.main;

        // Grid start size is 6x6.
        // TO DO change it to 6x6.
        grid = new GameObject[3,6];
        CreateGrid();

        
    }

    void Update()
    {
        if(Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            // If we have a selected block we don't have to Raycast every time.
            if(selectedBlock == null)
            {
                RaycastHit hit = CastRay(touch);
                if(hit.collider != null && hit.collider.gameObject.tag == "Block")
                {   
                    selectedBlock = hit.collider.gameObject;
                }
            }    

            if(selectedBlock != null)
            {
                Vector3 position = new Vector3(touch.position.x, 
                                                touch.position.y, 
                                                mainCamera.WorldToScreenPoint(selectedBlock.transform.position).z);
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(position);
                if(!isPickedUp)
                {
                    selectedBlock.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z - 1f);
                    isPickedUp = true;
                }
                selectedBlock.transform.position = new Vector3(worldPos.x, worldPos.y, selectedBlock.transform.position.z);
                CalcuateDistanceToNearestSwitch();
            }
        }
        else
        {
            if(selectedBlock != null)
            {
                if(isPickedUp)
                {
                    selectedBlock.transform.position = new Vector3(selectedBlock.transform.position.x, 
                                                                selectedBlock.transform.position.y, 
                                                                selectedBlock.transform.position.z + 1f);
                    isPickedUp = false;
                }
                if(nearestSwitch != null)
                {
                    selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                    nearestSwitch.transform.position.y, 
                                                                    selectedBlock.transform.position.z);
                }
                selectedBlock = null;
            }
        }
    }

    private RaycastHit CastRay(Touch touch)
    {
        Vector3 screenTouchPosFar = new Vector3(
            touch.position.x,
            touch.position.y,
            mainCamera.farClipPlane);

        Vector3 screenTouchPosNear = new Vector3(
            touch.position.x,
            touch.position.y,
            mainCamera.nearClipPlane);

        Vector3 worldTouchPosFar = mainCamera.ScreenToWorldPoint(screenTouchPosFar);
        Vector3 worldTouchPosNear = mainCamera.ScreenToWorldPoint(screenTouchPosNear);

        RaycastHit hit;
        Physics.Raycast(worldTouchPosNear, worldTouchPosFar - worldTouchPosNear, out hit);        

        return hit;
    }

    private void CalcuateDistanceToNearestSwitch()
    {
        float minDistanceToSwitch = 99f;
        GameObject tmpNearestSwitch = null;
        for(int row = 0; row < grid.GetLength(0); row++)
        {
            for(int col = 0; col < grid.GetLength(1); col++)
            {
                // Ignoring z axis.
                float distance = Vector2.Distance(grid[row,col].transform.position, selectedBlock.transform.position);
                // Prevent far distance placing.
                if(distance < 0.5f)
                {
                    if(distance < minDistanceToSwitch)
                    {
                        minDistanceToSwitch = distance;
                        tmpNearestSwitch = grid[row,col];
                    }
                }
                else
                    nearestSwitch = null;
            }
        }
        nearestSwitch = tmpNearestSwitch;
    }

    private void CreateGrid()
    {
        int currentIndex = 0;
        for(int row = 0; row < grid.GetLength(0); row++)
        {
            for(int col = 0; col < grid.GetLength(1); col++)
            {
                grid[row,col] = switches[currentIndex];
                currentIndex += 1;
            }
        }
    }
}
