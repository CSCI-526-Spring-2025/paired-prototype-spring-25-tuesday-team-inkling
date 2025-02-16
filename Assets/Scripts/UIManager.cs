using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI totalMoneyText;
    private int totalMoney = 0;

    public void UpdateTotalMoney(int amount)
    {
        totalMoney += amount;
        totalMoneyText.text = "Total Money: " + totalMoney;
    }
}
