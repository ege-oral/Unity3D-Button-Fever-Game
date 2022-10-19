using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyHandler : MonoBehaviour
{
    private static float currnetMoney = 0;
    public float CurrentMoney { get{ return currnetMoney; } set{ currnetMoney = value; }}

    [SerializeField] TextMeshProUGUI currentMoneyText;

    // Update is called once per frame
    void Update()
    {
        DisplayMoney();
    }

    private void DisplayMoney()
    {
        currentMoneyText.text = $"Money: {currnetMoney.ToString()}";
    }
}
