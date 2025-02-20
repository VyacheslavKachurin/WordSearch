using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Balance
{
    public static event Action<int> OnBalanceChanged;
    private static int _balance;
    public static int BalanceValue => _balance;

    public static bool UseAbility(int price)
    {
        if (_balance < price) return false;

        _balance -= price;
        OnBalanceChanged?.Invoke(_balance);

        Save();
        return true;
    }

    public static void InitBalance(int balance)
    {
        _balance = balance;
        OnBalanceChanged?.Invoke(_balance);
    }

    public static void AddBalance(int amount)
    {
        _balance += amount;
        OnBalanceChanged?.Invoke(_balance);
        Save();
    }

    private static void Save()
    {
        ProgressService.SaveCoins(_balance);
    }

}
