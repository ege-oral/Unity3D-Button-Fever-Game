using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyBlockHandler : MonoBehaviour
{
    private float blockValue = 10f;
    [SerializeField] GameObject buyButton;
    [SerializeField] TextMeshProUGUI blockValueText;
    [SerializeField] GameObject block1; // +1 block

    [SerializeField] BlockPlaceholder[] placeholders;

    MoneyHandler moneyHandler;
    void Start()
    {
        moneyHandler = FindObjectOfType<MoneyHandler>();
    }

    
    void Update()
    {
        blockValueText.text = $"Buy +1 Block: {blockValue}$";
        ChangeButtonColor();
    }

    public void BuyBlock()
    {
        if(moneyHandler.CurrentMoney >= blockValue)
        {
            for(int i = 0; i < placeholders.Length; i++)
            {
                if(placeholders[i].holdingBlock == null)
                {
                    GameObject newBlock = Instantiate(block1, placeholders[i].transform.position, Quaternion.identity);
                    newBlock.GetComponent<Block>().blockPlaceholder = placeholders[i];
                    placeholders[i].GetComponent<BlockPlaceholder>().holdingBlock = newBlock;
                    placeholders[i].GetComponent<BlockPlaceholder>().isBlockPlaced = true;
                    moneyHandler.CurrentMoney -= blockValue;
                    blockValue += 10f;
                    return;
                }
            }
            buyButton.GetComponent<Image>().color = Color.red;
        }
    }

    private void ChangeButtonColor()
    {
        if(moneyHandler.CurrentMoney < blockValue)
        {
            buyButton.GetComponent<Image>().color = Color.red;
        }
        else
        {
            foreach(BlockPlaceholder placeholder in placeholders)
            {
                if(placeholder.holdingBlock == null)
                {
                    buyButton.GetComponent<Image>().color = Color.green;
                    return;
                }
            }
            buyButton.GetComponent<Image>().color = Color.red;
        }
    }
}
