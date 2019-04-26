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
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // allow the app to open JSON/N-Space files
            // https://developer.apple.com/documentation/uikit/view_controllers/adding_a_document_browser_to_your_app/setting_up_a_document_browser_app

            // TODO? https://developer.apple.com/library/archive/documentation/General/Reference/InfoPlistKeyReference/Articles/iPhoneOSKeys.html#//apple_ref/doc/uid/TP40009252-SW37
            //rootDict.SetBoolean("UISupportsDocumentBrowser", true);

            var documentTypesArray = rootDict.CreateArray("CFBundleDocumentTypes");

            var jsonDocTypeDict = documentTypesArray.AddDict();
            // reference: https://developer.apple.com/library/archive/documentation/General/Reference/InfoPlistKeyReference/Articles/CoreFoundationKeys.html#//apple_ref/doc/uid/TP40009249-101685-TPXREF107
            jsonDocTypeDict.CreateArray("CFBundleTypeIconFiles"); // empty
            jsonDocTypeDict.SetString("CFBundleTypeName", "JSON File"); // TODO?
            jsonDocTypeDict.SetString("LSHandlerRank", "Owner"); // this app both creates and opens JSON files
            var jsonContentTypesArray = jsonDocTypeDict.CreateArray("LSItemContentTypes");
            jsonContentTypesArray.AddString("public.json");

            var nspaceDocTypeDict = documentTypesArray.AddDict();
            nspaceDocTypeDict.CreateArray("CFBundleTypeIconFiles");
            nspaceDocTypeDict.SetString("CFBundleTypeName", "N-Space World");
            nspaceDocTypeDict.SetString("LSHandlerRank", "Owner");
            var nspaceContentTypesArray = nspaceDocTypeDict.CreateArray("LSItemContentTypes");
            nspaceContentTypesArray.AddString("com.vantjac.nspace");

            // declare the N-Space file type
            var exportedTypeDeclarationsArray = rootDict.CreateArray("UTExportedTypeDeclarations");
            var nspaceDeclarationDict = exportedTypeDeclarationsArray.AddDict();
            nspaceDeclarationDict.SetString("UTTypeIdentifier", "com.vantjac.nspace");
            nspaceDeclarationDict.SetString("UTTypeDescription", "N-Space World");
            var conformsToArray = nspaceDeclarationDict.CreateArray("UTTypeConformsTo");
            conformsToArray.AddString("public.data");
            var tagSpecificationsDict = nspaceDeclarationDict.CreateDict("UTTypeTagSpecification");
            tagSpecificationsDict.SetString("com.apple.ostype", "NSPA");
            tagSpecificationsDict.SetString("public.filename-extension", "nspace");
            tagSpecificationsDict.SetString("public.mime-type", "application/vnd.vantjac.nspace");

            plist.WriteToFile(plistPath);
        }
    }
}