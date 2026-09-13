#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Warfest.Editor
{
    [InitializeOnLoad]
    public static class WarfestIconConfigurator
    {
        static WarfestIconConfigurator()
        {
            EditorApplication.delayCall += ConfigurePlayerSettingsIcons;
        }

        [MenuItem("Warfest/Configure App Icons")]
        public static void ConfigurePlayerSettingsIcons()
        {
            string iconPath = "Assets/icon.png";
            if (!File.Exists(iconPath))
            {
                iconPath = "Assets/AppIcon_1024.png";
            }

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] { icon });
                Debug.Log($"[WarfestIconConfigurator] Configured PlayerSettings icons with {iconPath}");
            }
        }
    }
}
#endif
