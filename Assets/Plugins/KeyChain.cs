using UnityEngine;
using System.Runtime.InteropServices;

public class KeyChain
{


	[DllImport("__Internal")]
	private static extern string getKeyChainUser();

	public static string BindGetKeyChainUser()
	{
		string id = string.Empty;
#if UNITY_IOS && !UNITY_EDITOR
		id = getKeyChainUser();
#else
		id = "UNITY_EDITOR";
#endif
		return id;
	}

	[DllImport("__Internal")]
	private static extern void setKeyChainUser(string userId, string uuid);

	public static void BindSetKeyChainUser(string userId, string uuid)
	{
		setKeyChainUser(userId, uuid);
	}

	[DllImport("__Internal")]
	private static extern void deleteKeyChainUser();

	public static void BindDeleteKeyChainUser()
	{
		deleteKeyChainUser();
	}

}

