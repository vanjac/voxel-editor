#if UNITY_IOS

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

// https://forum.unity.com/threads/how-can-you-add-items-to-the-xcode-project-targets-info-plist-using-the-xcodeapi.330574/
public static class IOSPostBuild
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

            var nspaceDocTypeDict = documentTypesArray.AddDict();
            // reference: https://developer.apple.com/library/archive/documentation/General/Reference/InfoPlistKeyReference/Articles/CoreFoundationKeys.html#//apple_ref/doc/uid/TP40009249-101685-TPXREF107
            nspaceDocTypeDict.CreateArray("CFBundleTypeIconFiles"); // empty
            nspaceDocTypeDict.SetString("CFBundleTypeName", "N-Space World");
            nspaceDocTypeDict.SetString("LSHandlerRank", "Owner"); // this app both creates and opens N-Space files
            var nspaceContentTypesArray = nspaceDocTypeDict.CreateArray("LSItemContentTypes");
            nspaceContentTypesArray.AddString("com.vantjac.nspace");

            AddImportType(documentTypesArray, "JSON file", "public.json");
            AddImportType(documentTypesArray, "MP3 audio", "public.mp3");
            AddImportType(documentTypesArray, "Waveform audio", "com.microsoft.waveform-audio");
            AddImportType(documentTypesArray, "AIFF-C audio", "public.aifc-audio");
            AddImportType(documentTypesArray, "AIFF audio", "public.aiff-audio");

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

    private static void AddImportType(PlistElementArray documentTypesArray, string name, string uti)
    {
        var docTypeDict = documentTypesArray.AddDict();
        docTypeDict.CreateArray("CFBundleTypeIconFiles");
        docTypeDict.SetString("CFBundleTypeName", name);
        docTypeDict.SetString("LSHandlerRank", "Alternate"); // this app is a secondary viewer of JSON files
        var contentTypesArray = docTypeDict.CreateArray("LSItemContentTypes");
        contentTypesArray.AddString(uti);
    }
}

#endif