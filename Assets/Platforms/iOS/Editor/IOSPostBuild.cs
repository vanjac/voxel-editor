using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

// https://forum.unity.com/threads/how-can-you-add-items-to-the-xcode-project-targets-info-plist-using-the-xcodeapi.330574/
public class IOSPostBuild
{
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            Debug.Log("iOS post build");
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;

            // allow the app to open JSON files
            // https://developer.apple.com/documentation/uikit/view_controllers/adding_a_document_browser_to_your_app/setting_up_a_document_browser_app

            // TODO? https://developer.apple.com/library/archive/documentation/General/Reference/InfoPlistKeyReference/Articles/iPhoneOSKeys.html#//apple_ref/doc/uid/TP40009252-SW37
            //rootDict.SetBoolean("UISupportsDocumentBrowser", true);

            var documentTypesArray = rootDict.CreateArray("CFBundleDocumentTypes");
            var jsonDocTypeDict = documentTypesArray.AddDict();
            // reference: https://developer.apple.com/library/archive/documentation/General/Reference/InfoPlistKeyReference/Articles/CoreFoundationKeys.html#//apple_ref/doc/uid/TP40009249-101685-TPXREF107
            jsonDocTypeDict.CreateArray("CFBundleTypeIconFiles"); // empty
            jsonDocTypeDict.SetString("CFBundleTypeName", "JSON File"); // TODO?
            jsonDocTypeDict.SetString("LSHandlerRank", "Owner"); // this app both creates and opens JSON files
            var contentTypesArray = jsonDocTypeDict.CreateArray("LSItemContentTypes");
            contentTypesArray.AddString("public.json");
        }
    }
}