using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public string blockName;
    [SerializeField] int blockValue;
    // We can't say that blockPlacePosition = Vector2.zero
    // Because we use that value. 
    public Vector2 blockPlacePosition = new Vector2(99f, 99f);

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
