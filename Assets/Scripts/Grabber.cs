using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    private GameObject selectedObject;
    Touch touch;
    Camera mainCamera;

    private bool isPickedUp = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    
    void Update()
    {
        if(Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);    
            RaycastHit hit = CastRay(touch);
            if(hit.collider != null && hit.collider.gameObject.tag == "Block")
            {   
                selectedObject = hit.collider.gameObject;
            }

            if(selectedObject != null)
            {
                Vector3 position = new Vector3(touch.position.x, 
                                                touch.position.y, 
                                                mainCamera.WorldToScreenPoint(selectedObject.transform.position).z);
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(position);
                if(!isPickedUp)
                {
                    selectedObject.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z - 1f);
                    isPickedUp = true;
                }
                selectedObject.transform.position = new Vector3(worldPos.x, worldPos.y, selectedObject.transform.position.z);
            }
        }
        else
        {
            if(selectedObject != null)
            {
                if(isPickedUp)
                {
                    selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, 
                                                                selectedObject.transform.position.y, 
                                                                selectedObject.transform.position.z + 1f);
                    isPickedUp = false;
                }

                selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, 
                                                                selectedObject.transform.position.y, 
                                                                selectedObject.transform.position.z);
                selectedObject = null;
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
}
