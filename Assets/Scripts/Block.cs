using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public int blockValue;
    [SerializeField] public string blockName;

    public bool isInPath = false;
    public BlockPlaceholder blockPlaceholder = null;
    public Vector2 blockPlacePosition = new Vector2(99f, 99f);

    [SerializeField] public Material blockMaterial;
}
