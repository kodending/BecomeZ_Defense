using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SystemPath
{
    public static string GetPath(string fileName)
    {
        //플랫폼마다 경로위치가 바뀌기 때문에 그걸 맞추기 위함
        string path = GetPath();
        return Path.Combine(GetPath(), fileName);
    }

    public static string GetPath()
    {
        //스위치문으로 플랫폼마다 맞는 경로 설정
        string path = null;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(Application.persistentDataPath, "Resources/");
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", "Resources/");
            case RuntimePlatform.WindowsEditor:
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", "Resources/");
            default:
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Resources/");
        }
    }
}
