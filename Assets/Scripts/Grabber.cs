using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    Touch touch;
    Camera mainCamera;
    GridManager gridManager;
    MoneyHandler moneyHandler;
    MoneyMultiplierHandler moneyMultiplierHandler;

    GameObject selectedBlock;
    GameObject tmpSelectedBlock;
    GameObject nearestSwitch = null;
    GameObject nearestPlaceholder = null;
    private bool raiseTheBlock = false;
    private bool getCurrentBlock = true;

    [SerializeField] GameObject[] placeholders;
    [SerializeField] GameObject block1; // +1 block.
    [SerializeField] GameObject block2; // +2 block.
    [SerializeField] GameObject block4; // +4 block.
    [SerializeField] GameObject block8; // +8 block.

    [SerializeField] Material defaultMaterial;

    
    void Start()
    {
        mainCamera = Camera.main;
        gridManager = FindObjectOfType<GridManager>();
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
                GetBlock();
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
                if(raiseTheBlock)
                {
                    selectedBlock.transform.position = new Vector3(selectedBlock.transform.position.x, selectedBlock.transform.position.y, selectedBlock.transform.position.z + 2f);
                    raiseTheBlock = false;
                }
                DropBlock();
                selectedBlock = null;
            }
        }
    }

    private void GetBlock()
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

    private void PickBlock()
    {
        GetBlockValues();
        ChangeBlockMaterial(selectedBlock, defaultMaterial);
        
        Block _selectedBlock = selectedBlock.GetComponent<Block>();

        // If block is on blockPlaceholder.
        if(_selectedBlock.blockPlaceholder != null)
        {
            _selectedBlock.blockPlaceholder.GetComponent<BlockPlaceholder>().isBlockPlaced = false;
            _selectedBlock.blockPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = null;
            _selectedBlock.blockPlaceholder = null;
        }

        // If block is on switch.
        else if(_selectedBlock.blockPlacePosition != new Vector2(99f,99f))
        {

            Switch _switch = gridManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>();

            _switch.isPlaceable = true;
            _switch.value = 0;
            _switch.holdingBlock = null;            

            if(_selectedBlock.blockName == "+2")
            {
                Switch _leftSwitch = gridManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>();
                _leftSwitch.isPlaceable = true;
                _leftSwitch.GetComponent<Switch>().holdingBlock = null;
            }
            else if(_selectedBlock.blockName == "+4")
            {
                Switch _leftSwitch = gridManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>();
                _leftSwitch.isPlaceable = true;
                _leftSwitch.holdingBlock = null;
                
                Switch _rightSwitch = gridManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, (int) _selectedBlock.blockPlacePosition.y + 1].GetComponent<Switch>();
                _rightSwitch.isPlaceable = true;
                _rightSwitch.holdingBlock = null;
            }
            else if(_selectedBlock.blockName == "+8")
            {
                Switch _leftSwitch = gridManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x, (int) _selectedBlock.blockPlacePosition.y - 1].GetComponent<Switch>();
                _leftSwitch.isPlaceable = true;
                _leftSwitch.holdingBlock = null;
                
                Switch _upSwitch = gridManager.switchGrid[(int) _selectedBlock.blockPlacePosition.x + 1, (int) _selectedBlock.blockPlacePosition.y].GetComponent<Switch>();
                _upSwitch.isPlaceable = true;
                _upSwitch.holdingBlock = null;
            }

            gridManager.FindConnectedSwitches();
            moneyMultiplierHandler.CheckIfRowFull();
        }
        // When pick up reset _selectedBlock.blockPlacePosition.
        _selectedBlock.blockPlacePosition = new Vector2(99f,99f);

        Vector3 position = new Vector3(touch.position.x, 
                                       touch.position.y, 
                                       mainCamera.WorldToScreenPoint(selectedBlock.transform.position).z);

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(position);

        if(!raiseTheBlock)
        {
            selectedBlock.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z - 2f);
            raiseTheBlock = true;
        }
        selectedBlock.transform.position = new Vector3(worldPos.x, worldPos.y, selectedBlock.transform.position.z);
        
        CalcuateDistanceToNearestPlaceholder();
        CalcuateDistanceToNearestSwitch();
    }

    private void DropBlock()
    {
        
        getCurrentBlock = true;

        Block _selectedBlock = selectedBlock.GetComponent<Block>();
        if(nearestPlaceholder != null)
        {
            ChangeBlockMaterial(selectedBlock, selectedBlock.GetComponent<Block>().blockMaterial);
            BlockPlaceholder _nearestPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            if(_nearestPlaceholder.isBlockPlaced && _nearestPlaceholder.holdingBlock.GetComponent<Block>().blockName == _selectedBlock.blockName)
            {
                MergeBlocks();
            }
            else if(!_nearestPlaceholder.isBlockPlaced)
            {                
                _selectedBlock.blockPlaceholder = _nearestPlaceholder;
                _nearestPlaceholder.isBlockPlaced = true;
                _nearestPlaceholder.holdingBlock = selectedBlock;
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
            if(_selectedBlock.blockName == "+1")
            {
                if(_nearestSwitch.isPlaceable)
                {
                    _nearestSwitch.isPlaceable = false;
                    _nearestSwitch.value = 1;
                    _nearestSwitch.holdingBlock = selectedBlock;

                    // We change y and x places for matrix position.
                   _selectedBlock.blockPlacePosition = new Vector2(nearestSwitch.transform.position.y, nearestSwitch.transform.position.x);
                    selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                   nearestSwitch.transform.position.y, 
                                                                   nearestSwitch.transform.position.z);
                    gridManager.FindConnectedSwitches(); 
                    moneyMultiplierHandler.CheckIfRowFull();
                }
                else
                {
                    FindEmptyPlaceHolder(selectedBlock);
                }
            }

            else if(_selectedBlock.blockName == "+2")
            {
                if((int) _nearestSwitch.transform.position.x - 1 > -1 && _nearestSwitch.isPlaceable)
                {
                    // This location is different in gridManager switchGrid.
                    Switch _leftSwitch = gridManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>();
                    // Check if left switch is placable ?
                    if(_leftSwitch.isPlaceable)
                    {
                        _nearestSwitch.isPlaceable = false;
                        _nearestSwitch.value = 2;
                        _nearestSwitch.holdingBlock = selectedBlock;

                        _leftSwitch.isPlaceable = false;
                        _leftSwitch.holdingBlock = selectedBlock;

                        // We change y and x places for matrix position.
                        _selectedBlock.blockPlacePosition = new Vector2(nearestSwitch.transform.position.y, nearestSwitch.transform.position.x);
                        selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                       nearestSwitch.transform.position.y, 
                                                                       nearestSwitch.transform.position.z);
                        gridManager.FindConnectedSwitches(); 
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

            else if(_selectedBlock.blockName == "+4")
            {
                if((int) _nearestSwitch.transform.position.x - 1 > -1 && (int) _nearestSwitch.transform.position.x + 1 < 6 && _nearestSwitch.isPlaceable)
                {
                    Switch _leftSwitch = gridManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>();
                    Switch _rightSwitch = gridManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x + 1].GetComponent<Switch>();
                    // Check if left and right switch is placable ?
                    if(_leftSwitch.isPlaceable && _rightSwitch.isPlaceable)
                    {
                            _nearestSwitch.isPlaceable = false;
                            _nearestSwitch.value = 4;
                            _nearestSwitch.holdingBlock = selectedBlock.gameObject;
                            
                            _leftSwitch.isPlaceable = false;
                            _leftSwitch.holdingBlock = selectedBlock;

                            _rightSwitch.isPlaceable = false;
                            _rightSwitch.holdingBlock = selectedBlock;

                            // We change y and x places for matrix position.
                            _selectedBlock.blockPlacePosition = new Vector2(nearestSwitch.transform.position.y, nearestSwitch.transform.position.x);
                            selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                           nearestSwitch.transform.position.y, 
                                                                           nearestSwitch.transform.position.z);
                            gridManager.FindConnectedSwitches(); 
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

            else if(_selectedBlock.blockName == "+8")
            {
                if((int) _nearestSwitch.transform.position.x - 1 > -1 && (int) _nearestSwitch.transform.position.y + 1 < 6 && _nearestSwitch.isPlaceable)
                {
                    Switch _leftSwitch = gridManager.switchGrid[(int) _nearestSwitch.transform.position.y, (int) _nearestSwitch.transform.position.x - 1].GetComponent<Switch>();
                    Switch _upSwitch = gridManager.switchGrid[(int) _nearestSwitch.transform.position.y + 1, (int) _nearestSwitch.transform.position.x].GetComponent<Switch>();
                    // Check if left and up switch is placable ?
                    if(_leftSwitch.isPlaceable && _upSwitch.isPlaceable)
                    {
                        _nearestSwitch.isPlaceable = false;
                        _nearestSwitch.value = 8;
                        _nearestSwitch.holdingBlock = selectedBlock.gameObject;

                        _leftSwitch.isPlaceable = false;
                        _leftSwitch.holdingBlock = selectedBlock;
                        
                        _upSwitch.isPlaceable = false;
                        _upSwitch.holdingBlock = selectedBlock.gameObject;

                        // We change y and x places for matrix position.
                        _selectedBlock.blockPlacePosition = new Vector2(nearestSwitch.transform.position.y, nearestSwitch.transform.position.x);
                        selectedBlock.transform.position = new Vector3(nearestSwitch.transform.position.x, 
                                                                       nearestSwitch.transform.position.y, 
                                                                       nearestSwitch.transform.position.z);
                        gridManager.FindConnectedSwitches(); 
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
        else
        {
            ReturnBlock();
        }
        if(!tmpSelectedBlock.activeInHierarchy)
        {
            Destroy(tmpSelectedBlock);
        }
    }

    private RaycastHit CastRay(Touch touch)
    {
        Vector3 screenTouchPosFar = new Vector3(touch.position.x, touch.position.y, mainCamera.farClipPlane);
        Vector3 screenTouchPosNear = new Vector3(touch.position.x, touch.position.y, mainCamera.nearClipPlane);

        Vector3 worldTouchPosFar = mainCamera.ScreenToWorldPoint(screenTouchPosFar);
        Vector3 worldTouchPosNear = mainCamera.ScreenToWorldPoint(screenTouchPosNear);

        RaycastHit hit;
        Physics.Raycast(worldTouchPosNear, worldTouchPosFar - worldTouchPosNear, out hit);        

        return hit;
    }

    private void CalcuateDistanceToNearestSwitch()
    {
        float minDistanceToSwitch = Mathf.Infinity;
        GameObject _nearestSwitch = null;
        for(int row = 0; row < gridManager.switchGrid.GetLength(0); row++)
        {
            for(int col = 0; col < gridManager.switchGrid.GetLength(1); col++)
            {
                // Ignoring z axis.
                float distance = Vector2.Distance(gridManager.switchGrid[row,col].transform.position, 
                                                  selectedBlock.transform.position);
                // Prevent far distance placing.
                if(distance < 1f)
                {
                    if(distance < minDistanceToSwitch)
                    {
                        minDistanceToSwitch = distance;
                        _nearestSwitch = gridManager.switchGrid[row,col];
                    }
                }
                else
                {
                    nearestSwitch = null;
                }
            }
        }
        nearestSwitch = _nearestSwitch;
    }

    private void CalcuateDistanceToNearestPlaceholder()
    {
        float minDistanceToPlaceholder = Mathf.Infinity;
        GameObject _nearestPlaceholder = null;
        for(int i = 0; i < placeholders.Length; i++)
        {
            float distance = Vector2.Distance(placeholders[i].transform.localPosition, 
                                              selectedBlock.transform.position);
            if(distance < 2f)
            {
                if(distance < minDistanceToPlaceholder)
                {
                    minDistanceToPlaceholder = distance;
                    _nearestPlaceholder = placeholders[i];
                }
            }
            else
            {
                nearestPlaceholder = null;
            }
        }
        nearestPlaceholder = _nearestPlaceholder;
    }

    private void FindEmptyPlaceHolder(GameObject selectedBlock)
    {
        for(int i = 0; i < placeholders.Length; i ++)
        {
            BlockPlaceholder _placeHolder = placeholders[i].GetComponent<BlockPlaceholder>();
            if(!_placeHolder.isBlockPlaced)
            {
                _placeHolder.isBlockPlaced = true;
                _placeHolder.holdingBlock = selectedBlock.gameObject;
                selectedBlock.GetComponent<Block>().blockPlaceholder = _placeHolder;
                ChangeBlockMaterial(selectedBlock, selectedBlock.GetComponent<Block>().blockMaterial);
                PlaceBlockToPlaceholder(selectedBlock, placeholders[i]);
                break;
            }
        }
    }

    private void PlaceBlockToPlaceholder(GameObject selectedBlock, GameObject nearestPlaceholder)
    {
        Block _selectedBlock = selectedBlock.GetComponent<Block>();
        if(_selectedBlock.blockName == "+1" || _selectedBlock.blockName == "+4")
        {
            selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x, 
                                                           nearestPlaceholder.transform.position.y, 
                                                           nearestPlaceholder.transform.position.z);                                        
        }
        else if(_selectedBlock.blockName == "+2")
        {
            selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x + 0.5f, 
                                                           nearestPlaceholder.transform.position.y, 
                                                           nearestPlaceholder.transform.position.z); 
        }
        else if(_selectedBlock.blockName == "+8")
        {
            selectedBlock.transform.position = new Vector3(nearestPlaceholder.transform.position.x + 0.5f, 
                                                           nearestPlaceholder.transform.position.y - 0.5f, 
                                                           nearestPlaceholder.transform.position.z); 
        }
    }
    
    private void MergeBlocks()
    {
        Vector3 _nearestPos = nearestPlaceholder.transform.position;
        if(selectedBlock.GetComponent<Block>().blockName == "+1")
        {
            Destroy(selectedBlock);

            GameObject newBlock = Instantiate(block2, new Vector3(_nearestPos.x + 0.5f, _nearestPos.y, _nearestPos.z), Quaternion.identity);                                   
            newBlock.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();

            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newBlock;
        }

        else if(selectedBlock.GetComponent<Block>().blockName == "+2")
        {
            Destroy(selectedBlock);

            GameObject newBlock = Instantiate(block4, _nearestPos, Quaternion.identity);
            newBlock.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();

            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newBlock;
        }

        else if(selectedBlock.GetComponent<Block>().blockName == "+4")
        {
            Destroy(selectedBlock);

            GameObject newBlock = Instantiate(block8, new Vector3(_nearestPos.x + 0.5f, _nearestPos.y - 0.5f, _nearestPos.z), Quaternion.identity);
            newBlock.GetComponent<Block>().blockPlaceholder = nearestPlaceholder.GetComponent<BlockPlaceholder>();
            
            Destroy(nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock.gameObject);
            nearestPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = newBlock;
        }
        else
        {
            FindEmptyPlaceHolder(selectedBlock);
        }
    }

    public void ChangeBlockMaterial(GameObject selectedBlock, Material material)
    {
        selectedBlock.GetComponent<Renderer>().material = material;

        if(selectedBlock.transform.childCount > 0)
        {
            foreach(Transform child in selectedBlock.transform)
            {
                if(child.gameObject.tag != "Text")
                {
                    child.GetComponent<Renderer>().material = material;
                }
            }
        }
    }

    private void GetBlockValues()
    {
        if(getCurrentBlock)
        {
            tmpSelectedBlock = Instantiate(selectedBlock);
            tmpSelectedBlock.SetActive(false);
            getCurrentBlock = false;
        }
    }

    private void ReturnBlock()
    {
        Destroy(selectedBlock);
        tmpSelectedBlock.SetActive(true);
        if(tmpSelectedBlock.GetComponent<Block>().blockPlaceholder != null)
        {
            tmpSelectedBlock.GetComponent<Block>().blockPlaceholder.GetComponent<BlockPlaceholder>().isBlockPlaced = true;
            tmpSelectedBlock.GetComponent<Block>().blockPlaceholder.GetComponent<BlockPlaceholder>().holdingBlock = tmpSelectedBlock;
        }
        else if(tmpSelectedBlock.GetComponent<Block>().blockPlacePosition != new Vector2(99f,99f))
        {
            FindEmptyPlaceHolder(tmpSelectedBlock);
        }

    }

}
