using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    Touch touch;
    Camera mainCamera;
    GridManager boardManager;
    MoneyHandler moneyHandler;
    MoneyMultiplierHandler moneyMultiplierHandler;

    GameObject selectedBlock;
    GameObject nearestSwitch = null;
    GameObject nearestPlaceholder = null;
    private bool isPickedUp = false;

    [SerializeField] GameObject[] placeholders;
    [SerializeField] GameObject block1; // +1 block.
    [SerializeField] GameObject block2; // +2 block.
    [SerializeField] GameObject block4; // +4 block.
    [SerializeField] GameObject block8; // +8 block.


    void Start()
    {
        mainCamera = Camera.main;
        boardManager = FindObjectOfType<GridManager>();
        moneyHandler = FindObjectOfType<MoneyHandler>();
        moneyMultiplierHandler = FindObjectOfType<MoneyMultiplierHandler>();
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
            // If Block is in path earn money.
            else if(touch.phase == TouchPhase.Began)
            {
                RaycastHit hit = CastRay(touch);
                if(hit.collider != null && hit.collider.gameObject.tag == "Block" && hit.collider.gameObject.GetComponent<Block>().isInPath)
                {
                    moneyHandler.EarnMoney();
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
                                                                selectedBlock.transform.position.z + 2f);
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
        float minDistanceToSwitch = Mathf.Infinity;
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
        float minDistanceToPlaceholder = Mathf.Infinity;
        GameObject tmpNearestPlaceholder = null;
        for(int i = 0; i < placeholders.Length; i++)
        {
            float distance = Vector2.Distance(placeholders[i].transform.localPosition, 
                                              selectedBlock.transform.position);
            if(distance < 2f)
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


            boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                    (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>().holdingBlock = null;

            

            if(_selectedBlock.blockName == "+2")
            {
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().isPlaceable = true;

 
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().holdingBlock = null;
            }
            else if(_selectedBlock.blockName == "+4")
            {

                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().isPlaceable = true;

                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().holdingBlock = null;
                
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y + 1].GetComponent<Switch>().isPlaceable = true;
   
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y + 1].GetComponent<Switch>().holdingBlock = null;
            }
            else if(_selectedBlock.blockName == "+8")
            {

                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().isPlaceable = true;

     
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, 
                                        (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>().holdingBlock = null;
                
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x + 1, 
                                        (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>().isPlaceable = true;

               
                boardManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x + 1, 
                                        (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>().holdingBlock = null;
            }
            boardManager.FindConnectedSwitches();
            moneyMultiplierHandler.CheckIfRowFull();
        }
        // When pick up reset _selectedBlock.blockPlacePosition.
        _selectedBlock.blockPlacePosition = new Vector2(99f,99f);

        Vector3 position = new Vector3(touch.position.x, 
                                       touch.position.y, 
                                       mainCamera.WorldToScreenPoint(selectedBlock.transform.position).z);

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(position);

        if(!isPickedUp)
        {
            selectedBlock.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z - 2f);
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
                PlaceBlockToPlaceholder(selectedBlock, nearestPlaceholder);
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
                    _nearestSwitch.holdingBlock = selectedBlock.gameObject;
                    // We change y and x places for matrix position.
                    selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                        nearestSwitch.transform.position.x);

                    selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                    nearestSwitch.transform.position.y, 
                                                                    nearestSwitch.transform.position.z);
                    boardManager.FindConnectedSwitches(); 
                    moneyMultiplierHandler.CheckIfRowFull();
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
                    // Check if left switch is placable ?
                    if(boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable)
                    {
                        _nearestSwitch.isPlaceable = false;
                        _nearestSwitch.value = 2;
                        _nearestSwitch.holdingBlock = selectedBlock.gameObject;

                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable = false;
                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().holdingBlock = selectedBlock.gameObject;
                        // We change y and x places for matrix position.
                        selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                                nearestSwitch.transform.position.x);

                        selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                        nearestSwitch.transform.position.y, 
                                                                        nearestSwitch.transform.position.z);
                        boardManager.FindConnectedSwitches(); 
                        moneyMultiplierHandler.CheckIfRowFull();
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
                            _nearestSwitch.holdingBlock = selectedBlock.gameObject;

                            boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable = false;
                            boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().holdingBlock = selectedBlock.gameObject;

                            boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x + 1].GetComponent<Switch>().isPlaceable = false;
                            boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x + 1].GetComponent<Switch>().holdingBlock = selectedBlock.gameObject;
                            // We change y and x places for matrix position.
                            selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                                 nearestSwitch.transform.position.x);

                            selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                            nearestSwitch.transform.position.y, 
                                                                            nearestSwitch.transform.position.z);
                            boardManager.FindConnectedSwitches(); 
                            moneyMultiplierHandler.CheckIfRowFull();
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
                        _nearestSwitch.holdingBlock = selectedBlock.gameObject;

                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().isPlaceable = false;
                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>().holdingBlock = selectedBlock.gameObject;

                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y + 1, (int) _nearestSwitch.transform.position.x].GetComponent<Switch>().isPlaceable = false;
                        boardManager.switchGrid[(int) _nearestSwitch.transform.position.y + 1, (int) _nearestSwitch.transform.position.x].GetComponent<Switch>().holdingBlock = selectedBlock.gameObject;

                        selectedBlock.GetComponent<Block>().blockPlacePosition = new Vector2(nearestSwitch.transform.position.y,
                                                                                                nearestSwitch.transform.position.x);

                        selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                        nearestSwitch.transform.position.y, 
                                                                        nearestSwitch.transform.position.z);
                        boardManager.FindConnectedSwitches(); 
                        moneyMultiplierHandler.CheckIfRowFull();
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
                PlaceBlockToPlaceholder(selectedBlock, placeholders[i]);
                break;
            }
        }
    }

    private void MergeBlocks()
    {
        Vector3 _nearestPlaceholderPosition = nearestPlaceholder.transform.position;
        if(selectedBlock.GetComponent<Block>().blockName == "+1")
        {
            Destroy(selectedBlock);
            GameObject newObject = Instantiate(block2, 
                                               new Vector3(_nearestPlaceholderPosition.x + 0.5f, _nearestPlaceholderPosition.y, _nearestPlaceholderPosition.z),
                                               Quaternion.identity);
                                               
            newObject.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newObject;
        }

        else if(selectedBlock.GetComponent<Block>().blockName == "+2")
        {
            Destroy(selectedBlock);
            GameObject newObject = Instantiate(block4, 
                                                _nearestPlaceholderPosition,
                                                Quaternion.identity);
            newObject.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newObject;
        }

        else if(selectedBlock.GetComponent<Block>().blockName == "+4")
        {
            Destroy(selectedBlock);
            GameObject newObject = Instantiate(block8, 
                                                new Vector3(_nearestPlaceholderPosition.x + 0.5f, _nearestPlaceholderPosition.y - 0.5f, _nearestPlaceholderPosition.z),
                                                Quaternion.identity);
            newObject.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newObject;
        }
        else{
            FindEmptyPlaceHolder(selectedBlock);
        }
    }

    private void PlaceBlockToPlaceholder(GameObject selectedBlock, GameObject nearestPlaceholder)
    {
        string blockName = selectedBlock.GetComponent<Block>().blockName;
        if(blockName == "+1" || blockName == "+4")
        {
            selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x, 
                                                           nearestPlaceholder.transform.position.y, 
                                                           nearestPlaceholder.transform.position.z);                                        
        }
        else if(blockName == "+2")
        {
            selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x + 0.5f, 
                                                           nearestPlaceholder.transform.position.y, 
                                                           nearestPlaceholder.transform.position.z); 
        }
        else if(blockName == "+8")
        {
            selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x + 0.5f, 
                                                           nearestPlaceholder.transform.position.y - 0.5f, 
                                                           nearestPlaceholder.transform.position.z); 
        }
    }

}
