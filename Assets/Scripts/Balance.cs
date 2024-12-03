using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Balance
{
    public static event Action<decimal> OnBalanceChanged;

    private const string BALANCE_KEY = "balance";
    private const int STARTING_BALANCE = 300;
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
        //check if file exists
        if (System.IO.File.Exists(PATH))
        {
            var txt = System.IO.File.ReadAllText(PATH);
            if (txt != null)
            {
                var jsonData = txt.ToString();
                _balanceData = JsonConvert.DeserializeObject<BalanceData>(jsonData);
                _balance = _balanceData.Balance;
            }
        }
        else
        {
            _balance = STARTING_BALANCE;
            Save();
        }
    }

    public static void AddBalance(double amount, int delay = 0)
    {
        Debug.Log($"Adding {amount}");
        // AnimateBalance((int)amount, delay);
        _balance += (int)amount;

        OnBalanceChanged?.Invoke(_balance);
        Save();


    }

    private static async void AnimateBalance(int amount, int delay)
    {
        var targetBalance = _balance + amount;
        while (_balance != targetBalance)
        {
            _balance++;
            OnBalanceChanged?.Invoke(_balance);
            await Task.Delay(delay);
        }

        Save();
    }

    public static void ClearBalance()
    {
         if (System.IO.File.Exists(PATH))
        {
            System.IO.File.Delete(PATH);
        }

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