using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    Touch touch;
    Camera mainCamera;
    BoardManager boardManager;

    GameObject selectedBlock;
    GameObject nearestSwitch = null;
    GameObject nearestPlaceholder = null;
    private bool isPickedUp = false;

    [SerializeField] GameObject[] placeholders;
    [SerializeField] GameObject block1; // Holding +1 block.
    [SerializeField] GameObject block2; // Holding +2 block.
    [SerializeField] GameObject block4; // Holding +4 block.
    [SerializeField] GameObject block8; // Holding +8 block.


    void Start()
    {
        mainCamera = Camera.main;
        boardManager = FindObjectOfType<BoardManager>();
    }

    void Update()
    {
        if(Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Moved)
            {
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
                    PickBlock();
                }
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
                DropBlock();
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
        for(int row = 0; row < boardManager.switchGrid.GetLength(0); row++)
        {
            for(int col = 0; col < boardManager.switchGrid.GetLength(1); col++)
            {
                // Ignoring z axis.
                float distance = Vector2.Distance(boardManager.switchGrid[row,col].transform.position, 
                                                  selectedBlock.transform.position);
                // Prevent far distance placing.
                if(distance < 1f)
                {
                    if(distance < minDistanceToSwitch)
                    {
                        minDistanceToSwitch = distance;
                        tmpNearestSwitch = boardManager.switchGrid[row,col];
                    }
                }
                else
                    nearestSwitch = null;
            }
        }
        nearestSwitch = tmpNearestSwitch;
    }

    private void CalcuateDistanceToNearestPlaceholder()
    {
        float minDistanceToPlaceholder = 99f;
        GameObject tmpNearestPlaceholder = null;
        for(int i = 0; i < placeholders.Length; i++)
        {
            float distance = Vector2.Distance(placeholders[i].transform.localPosition, 
                                              selectedBlock.transform.position);
            if(distance < 1f)
            {
                if(distance < minDistanceToPlaceholder)
                {
                    minDistanceToPlaceholder = distance;
                    tmpNearestPlaceholder = placeholders[i];
                }
            }
            else
                nearestPlaceholder = null;
        }
        nearestPlaceholder = tmpNearestPlaceholder;
    }

    private void PickBlock()
    {
        Block _selectedBlock = selectedBlock.GetComponent<Block>();
        if(_selectedBlock.blockPlaceholder != null)
        {
            _selectedBlock.blockPlaceholder.GetComponent<BlockPlaceholder>().isBlockPlaced = false;
            _selectedBlock.blockPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = null;
            _selectedBlock.blockPlaceholder = null;
        }

        else if(_selectedBlock.blockPlacePosition != new Vector2(99f,99f))
        {
            boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                    (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>().isPlaceable = true;

            boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                    (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>().value = 0;

            if(_selectedBlock.blockName == "+2")
            {
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().isPlaceable = true;
            }
            else if(_selectedBlock.blockName == "+4")
            {

                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().isPlaceable = true;
                
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y + 1].GetComponent<Switch>().isPlaceable = true;
            }
            else if(_selectedBlock.blockName == "+8")
            {

                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().isPlaceable = true;
                
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x + 1, 
                                        (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>().isPlaceable = true;
            }
        }
        // When pick up reset _selectedBlock.blockPlacePosition.
        _selectedBlock.blockPlacePosition = new Vector2(99f,99f);

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
        CalcuateDistanceToNearestPlaceholder();
        CalcuateDistanceToNearestSwitch();
    }

    private void DropBlock()
    {
        string blockName = selectedBlock.GetComponent<Block>().blockName;
        if(nearestPlaceholder != null)
        {
            if(nearestPlaceholder.GetComponent<BlockPlaceholder>().isBlockPlaced 
               && 
               nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.GetComponent<Block>().blockName == selectedBlock.GetComponent<Block>().blockName)
            {
                MergeBlocks();
            }
            else if(!nearestPlaceholder.GetComponent<BlockPlaceholder>().isBlockPlaced)
            {
                selectedBlock.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
                nearestPlaceholder.GetComponent<BlockPlaceholder>().isBlockPlaced = true;
                nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = selectedBlock.gameObject;

                selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x, 
                                                            nearestPlaceholder.transform.position.y, 
                                                            nearestPlaceholder.transform.position.z);
            }
            else
            {
                FindEmptyPlaceHolder(selectedBlock);
            }
        }
        else if(nearestSwitch != null)
        {
            Switch _nearestSwitch = nearestSwitch.GetComponent<Switch>();
            if(blockName == "+1")
            {
                if(_nearestSwitch.isPlaceable)
                {
                    _nearestSwitch.isPlaceable = false;
                    _nearestSwitch.value = 1;
                    // We change y and x places for matrix position.
                    selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                        nearestSwitch.transform.position.x);

                    selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                    nearestSwitch.transform.position.y, 
                                                                    nearestSwitch.transform.position.z);
                }
                else
                {
                    FindEmptyPlaceHolder(selectedBlock);
                }
            }

            else if(blockName == "+2")
            {
                if((int) _nearestSwitch.transform.position.x - 1 > -1 && _nearestSwitch.isPlaceable)
                {
                    // This location is different in boardManager switchGrid.
                    if(boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable)
                    {
                        _nearestSwitch.isPlaceable = false;
                        _nearestSwitch.value = 2;
                        // Check if left switch is placable ?
                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable = false;
                        // We change y and x places for matrix position.
                        selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                                nearestSwitch.transform.position.x);

                        selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                        nearestSwitch.transform.position.y, 
                                                                        nearestSwitch.transform.position.z);
                    }
                    else
                    {
                        FindEmptyPlaceHolder(selectedBlock);
                    }
                }
                else
                {
                    FindEmptyPlaceHolder(selectedBlock);
                }
            }

            else if(blockName == "+4")
            {
                if((int) _nearestSwitch.transform.position.x - 1 > -1 && (int) _nearestSwitch.transform.position.x + 1 < 6 && _nearestSwitch.isPlaceable)
                {
                    // Check if left and right switch is placable ?
                    if(boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable
                       &&
                       boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x + 1].GetComponent<Switch>().isPlaceable)
                    {
                            _nearestSwitch.isPlaceable = false;
                            _nearestSwitch.value = 4;
                            boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable = false;
                            boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x + 1].GetComponent<Switch>().isPlaceable = false;
                            // We change y and x places for matrix position.
                            selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                                 nearestSwitch.transform.position.x);

                            selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                            nearestSwitch.transform.position.y, 
                                                                            nearestSwitch.transform.position.z);
                    }
                    else
                    {
                        FindEmptyPlaceHolder(selectedBlock);
                    }
                }
                else
                {
                    FindEmptyPlaceHolder(selectedBlock);
                }
            }

            else if(blockName == "+8")
            {
                if((int) _nearestSwitch.transform.position.x - 1 > -1 && (int) _nearestSwitch.transform.position.y + 1 < 6 && _nearestSwitch.isPlaceable)
                {
                    // Check if left and up switch is placable ?
                    if(boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable
                       &&
                       boardManager.switchGrid[(int) _nearestSwitch.transform.position.y + 1, (int) _nearestSwitch.transform.position.x].GetComponent<Switch>().isPlaceable)
                    {
                        _nearestSwitch.isPlaceable = false;
                        _nearestSwitch.value = 8;

                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable = false;
                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y + 1, (int) _nearestSwitch.transform.position.x].GetComponent<Switch>().isPlaceable = false;

                        selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                                nearestSwitch.transform.position.x);

                        selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                        nearestSwitch.transform.position.y, 
                                                                        nearestSwitch.transform.position.z);
                    }
                    else
                    {
                        FindEmptyPlaceHolder(selectedBlock);
                    }
                }
                else
                {
                    FindEmptyPlaceHolder(selectedBlock);
                }
            }
        }
    }

    private void FindEmptyPlaceHolder(GameObject selectedBlock)
    {
        for(int i = 0; i < placeholders.Length; i ++)
        {
            BlockPlaceholder _tmpPlaceHolder = placeholders[i].GetComponent<BlockPlaceholder>();
            if(!_tmpPlaceHolder.isBlockPlaced)
            {
                _tmpPlaceHolder.isBlockPlaced = true;
                _tmpPlaceHolder.holdingBlock = selectedBlock.gameObject;
                selectedBlock.GetComponent<Block>().blockPlaceholder = _tmpPlaceHolder;
                selectedBlock.transform.position = placeholders[i].transform.position;
                break;
            }
        }
    }

    private void MergeBlocks()
    {
        if(selectedBlock.GetComponent<Block>().blockName == "+1")
        {
            Destroy(selectedBlock);
            GameObject newObject = Instantiate(block2, 
                                                nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.transform.position,
                                                Quaternion.identity);
            newObject.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newObject;
        }

        else if(selectedBlock.GetComponent<Block>().blockName == "+2")
        {
            Destroy(selectedBlock);
            GameObject newObject = Instantiate(block4, 
                                                nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.transform.position,
                                                Quaternion.identity);
            newObject.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newObject;
        }

        else if(selectedBlock.GetComponent<Block>().blockName == "+4")
        {
            Destroy(selectedBlock);
            GameObject newObject = Instantiate(block8, 
                                                nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.transform.position,
                                                Quaternion.identity);
            newObject.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newObject;
        }
        else{
            FindEmptyPlaceHolder(selectedBlock);
        }
    }
}
