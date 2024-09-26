using System;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;

public static class Balance
{
    public static event Action<decimal> OnBalanceChanged;

    private const string BALANCE_KEY = "balance";
    private const int STARTING_BALANCE = 500;
    private static int _balance;

    private const string PATH = "Assets/Resources/Balance.json";
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
        Debug.Log("reading balance from file");

        if (System.IO.File.Exists(PATH))
        {
            var jsonData = System.IO.File.ReadAllText(PATH);
            _balanceData = JsonConvert.DeserializeObject<BalanceData>(jsonData);
            _balance = _balanceData.Balance;
        }
        else
        {
            _balance = STARTING_BALANCE;
            Save();
        }
    }

    public static void AddBalance(int amount)
    {
        _balance += amount;
        OnBalanceChanged?.Invoke(_balance);
        Save();
    }


    private static void Save()
    {
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