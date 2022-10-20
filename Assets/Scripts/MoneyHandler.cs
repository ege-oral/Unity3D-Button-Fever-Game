using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyHandler : MonoBehaviour
{
    private static float currentMoney = 0;
    public float CurrentMoney { get{ return currentMoney; } set{ currentMoney = value; }}

    private static float moneyToBeEarned = 0;
    public float MoneyToBeEarned { get{ return moneyToBeEarned; } set{ moneyToBeEarned = value; }}

    public float[] moneyMultipliersValues = new float[] {1, 1, 1, 1, 1, 1};


    [SerializeField] TextMeshProUGUI currentMoneyText;
    [SerializeField] TextMeshProUGUI moneyToBeEarnedText;

    void Update()
    {
        DisplayMoney();
    }

    private void DisplayMoney()
    {
        currentMoneyText.text = $"Current Money: {currentMoney.ToString()}";
        moneyToBeEarnedText.text = $"Money To Be Earned: {(MoneyToBeEarned * moneyMultipliersValues[0] * moneyMultipliersValues[1] * moneyMultipliersValues[2] * moneyMultipliersValues[3] * moneyMultipliersValues[4] * moneyMultipliersValues[5]).ToString()}";
    }

    public void EarnMoney()
    {
        CurrentMoney += (MoneyToBeEarned * moneyMultipliersValues[0] * moneyMultipliersValues[1] * moneyMultipliersValues[2] * moneyMultipliersValues[3] * moneyMultipliersValues[4] * moneyMultipliersValues[5]);
    }
}
