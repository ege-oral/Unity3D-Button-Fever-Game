using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyHandler : MonoBehaviour
{
    private static float currentMoney = 1000;
    public float CurrentMoney { get{ return currentMoney; } set{ currentMoney = value; }}

    private static float moneyEarnedSoFar = 0;
    public float MoneyEarnedSoFar { get{ return moneyEarnedSoFar; } set{ moneyEarnedSoFar = value; }}

    private static float moneyToBeEarned = 0;
    public float MoneyToBeEarned { get{ return moneyToBeEarned; } set{ moneyToBeEarned = value; }}

    public float[] moneyMultipliersValues = new float[] {1, 1, 1, 1, 1, 1};


    [SerializeField] TextMeshProUGUI currentMoneyText;
    [SerializeField] TextMeshProUGUI moneyEarnedSoFarText;
    [SerializeField] TextMeshProUGUI moneyToBeEarnedText;

    void Update()
    {
        DisplayMoney();
    }

    private void DisplayMoney()
    {
        currentMoneyText.text = $"Current Money: {currentMoney.ToString("F2")}$";
        moneyEarnedSoFarText.text = $"Earned So Far: {moneyEarnedSoFar.ToString("F2")}$";
        moneyToBeEarnedText.text = $"Money To Be Earned: {MultiplyMoney().ToString("F2")}$";
    }

    public void EarnMoney()
    {
        CurrentMoney += MultiplyMoney();
        MoneyEarnedSoFar += MultiplyMoney();
    }

    private float MultiplyMoney()
    {
        return (MoneyToBeEarned * moneyMultipliersValues[0] * moneyMultipliersValues[1] * moneyMultipliersValues[2] * moneyMultipliersValues[3] * moneyMultipliersValues[4] * moneyMultipliersValues[5]);
    }
}
