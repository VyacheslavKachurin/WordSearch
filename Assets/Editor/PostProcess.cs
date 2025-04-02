using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using UnityEngine.UI;

public class PostProcess
{
    const string k_TrackingDescription = "Your data will be used to provide you a better and personalized ad experience.";
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
            rootDict.SetString("NSUserTrackingUsageDescription", k_TrackingDescription);

            if (!rootDict.values.ContainsKey("SKAdNetworkItems"))
            {
                PlistElementArray skAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");

                // Create a dictionary inside the SKAdNetworkItems array
                PlistElementDict skAdNetworkDict = skAdNetworkItems.AddDict();

                string[] skAdNetworkIdentifiers = new string[]
          {
                "cstr6suwn9.skadnetwork",
                "4fzdc2evr5.skadnetwork",
                "2fnua5tdw4.skadnetwork",
                "ydx93a7ass.skadnetwork",
                "p78axxw29g.skadnetwork",
                "v72qych5uu.skadnetwork",
                "ludvb6z3bs.skadnetwork",
                "cp8zw746q7.skadnetwork",
                "3sh42y64q3.skadnetwork",
                "c6k4g5qg8m.skadnetwork",
                "s39g8k73mm.skadnetwork",
                "3qy4746246.skadnetwork",
                "hs6bdukanm.skadnetwork",
                "mlmmfzh3r3.skadnetwork",
                "v4nxqhlyqp.skadnetwork",
                "wzmmz9fp6w.skadnetwork",
                "su67r6k2v3.skadnetwork",
                "yclnxrl5pm.skadnetwork",
                "7ug5zh24hu.skadnetwork",
                "gta9lk7p23.skadnetwork",
                "vutu7akeur.skadnetwork",
                "y5ghdn5j9k.skadnetwork",
                "v9wttpbfk9.skadnetwork",
                "n38lu8286q.skadnetwork",
                "47vhws6wlr.skadnetwork",
                "kbd757ywx3.skadnetwork",
                "9t245vhmpl.skadnetwork",
                "a2p9lx4jpn.skadnetwork",
                "22mmun2rn5.skadnetwork",
                "4468km3ulz.skadnetwork",
                "2u9pt9hc89.skadnetwork",
                "8s468mfl3y.skadnetwork",
                "ppxm28t8ap.skadnetwork",
                "uw77j35x4d.skadnetwork",
                "pwa73g5rt2.skadnetwork",
                "578prtvx9j.skadnetwork",
                "4dzt52r2t5.skadnetwork",
                "tl55sbb4fm.skadnetwork",
                "e5fvkxwrpn.skadnetwork",
                "8c4e2ghe7u.skadnetwork",
                "3rd42ekr43.skadnetwork",
                "3qcr597p9d.skadnetwork"
          };

                foreach (var identifier in skAdNetworkIdentifiers)
                {
                    skAdNetworkDict.SetString("SKAdNetworkIdentifier", identifier);
                }

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
         //   File.WriteAllText(pathToBuiltProject + "/info2.plist", plist.WriteToString());
        }
    }
}