using System;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Balance
{
    public static event Action<decimal> OnBalanceChanged;

    private const string BALANCE_KEY = "balance";
    private const int STARTING_BALANCE = 500;
    private static int _balance;

    private static readonly string PATH = Application.persistentDataPath + "/Balance.json";
    private static BalanceData _balanceData;

    public static bool UseAbility(int price)
    {
        if (_balance < price) return false;
        _balance -= price;
        OnBalanceChanged?.Invoke(_balance);
        Save();
        return true;
    }

    public static decimal GetBalance()
    {
        if (_balanceData == null) Load();
        return _balance;
    }

    public static void Load()
    {
        var txt = Resources.Load<TextAsset>(PATH);
        if (txt != null)
        {

            var jsonData = txt.ToString();
            _balanceData = JsonConvert.DeserializeObject<BalanceData>(jsonData);
            _balance = _balanceData.Balance;
        }
        else
        {
            _balance = STARTING_BALANCE;
            Save();
        }
    }

    public static void AddBalance(double amount)
    {

        _balance += (int)amount;
        OnBalanceChanged?.Invoke(_balance);
        Save();
    }


    private static void Save()
    {
        
        Debug.Log($"Saving balance: {_balance}");
        var balanceData = new BalanceData(_balance);
        var jsonData = JsonConvert.SerializeObject(balanceData);
        System.IO.File.WriteAllText(PATH, jsonData);
    }


}

public class BalanceData
{
    public int Balance { get; set; }

    public BalanceData(int balance)
    {
        Balance = balance;
    }
}