using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public static class UGS
{
    private const string ENV = "production";
    public static async Task Init()
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName(ENV);
        await UnityServices.InitializeAsync(options);
    }
}