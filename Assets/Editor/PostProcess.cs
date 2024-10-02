using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PostProcess
{
    [PostProcessBuild(999)]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get plist file and read it.
            string plistPath = pathToBuiltProject + "/Info.plist";
            Debug.Log("In the ChangeXCodePlist, path is: " + plistPath);
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            Debug.Log("In the ChangeXCodePlist");

            // Get root
            PlistElementDict rootDict = plist.root;

            if (!rootDict.values.ContainsKey("SKAdNetworkItems"))
            {
                PlistElementArray skAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");

                // Create a dictionary inside the SKAdNetworkItems array
                PlistElementDict skAdNetworkDict = skAdNetworkItems.AddDict();

                // Add the SKAdNetworkIdentifier key and value
                skAdNetworkDict.SetString("SKAdNetworkIdentifier", "su67r6k2v3.skadnetwork");
            }

            if (!rootDict.values.ContainsKey("NSAdvertisingAttributionReportEndpoint"))
            {
                rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://postbacks-is.com");
            }

            if (!rootDict.values.ContainsKey("NSAppTransportSecurity"))
            {
                // Create NSAppTransportSecurity dictionary
                PlistElementDict appTransportSecurityDict = rootDict.CreateDict("NSAppTransportSecurity");

                // Add NSAllowsArbitraryLoads boolean and set it to YES
                appTransportSecurityDict.SetBoolean("NSAllowsArbitraryLoads", true);
            }


            Debug.Log("PLIST: " + plist.WriteToString());

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
            File.WriteAllText(pathToBuiltProject + "/info2.plist", plist.WriteToString());
        }
    }
}