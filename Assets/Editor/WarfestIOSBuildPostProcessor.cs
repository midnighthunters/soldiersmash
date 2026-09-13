#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Warfest.Editor
{
    /// <summary>
    /// Automatically runs after Unity exports an iOS Xcode build.
    /// Guarantees that Info.plist is always configured for Portrait orientation on both iPhone and iPad,
    /// so future builds from Unity Editor never revert to landscape.
    /// </summary>
    public static class WarfestIOSBuildPostProcessor
    {
        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if (!File.Exists(plistPath)) return;

            try
            {
                string content = File.ReadAllText(plistPath);

                const string iphoneOrientations = 
                    "<key>UISupportedInterfaceOrientations</key>\n" +
                    "    <array>\n" +
                    "      <string>UIInterfaceOrientationPortrait</string>\n" +
                    "    </array>";

                const string ipadOrientations = 
                    "<key>UISupportedInterfaceOrientations~ipad</key>\n" +
                    "    <array>\n" +
                    "      <string>UIInterfaceOrientationPortrait</string>\n" +
                    "      <string>UIInterfaceOrientationPortraitUpsideDown</string>\n" +
                    "    </array>";

                string patternPhone = @"<key>UISupportedInterfaceOrientations<\/key>\s*<array>[\s\S]*?<\/array>";
                if (Regex.IsMatch(content, patternPhone))
                {
                    content = Regex.Replace(content, patternPhone, iphoneOrientations);
                }

                string patternIpad = @"<key>UISupportedInterfaceOrientations~ipad<\/key>\s*<array>[\s\S]*?<\/array>";
                if (Regex.IsMatch(content, patternIpad))
                {
                    content = Regex.Replace(content, patternIpad, ipadOrientations);
                }
                else
                {
                    int lastDictIdx = content.LastIndexOf("</dict>", StringComparison.OrdinalIgnoreCase);
                    if (lastDictIdx >= 0)
                    {
                        content = content.Insert(lastDictIdx, "    " + ipadOrientations + "\n  ");
                    }
                }

                if (!content.Contains("ITSAppUsesNonExemptEncryption"))
                {
                    int lastDictIdx = content.LastIndexOf("</dict>", StringComparison.OrdinalIgnoreCase);
                    if (lastDictIdx >= 0)
                    {
                        content = content.Insert(lastDictIdx, "    <key>ITSAppUsesNonExemptEncryption</key>\n    <false />\n  ");
                    }
                }

                File.WriteAllText(plistPath, content);
                Debug.Log("[WarfestIOSBuildPostProcessor] Enforced Portrait orientation and non-exempt encryption in Info.plist");

                EnsureAppIcons(pathToBuiltProject);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WarfestIOSBuildPostProcessor] Error updating build: " + ex.Message);
            }
        }

        private static void EnsureAppIcons(string pathToBuiltProject)
        {
            try
            {
                string iconSetDir = Path.Combine(pathToBuiltProject, "Unity-iPhone", "Images.xcassets", "AppIcon.appiconset");
                if (!Directory.Exists(iconSetDir))
                {
                    Directory.CreateDirectory(iconSetDir);
                }

                string masterIcon = Path.Combine(Application.dataPath, "AppIcon_1024.png");
                if (!File.Exists(masterIcon))
                {
                    masterIcon = Path.Combine(Application.dataPath, "icon.png");
                }

                if (File.Exists(masterIcon))
                {
                    string target1024 = Path.Combine(iconSetDir, "Icon-1024.png");
                    File.Copy(masterIcon, target1024, true);
                    Debug.Log("[WarfestIOSBuildPostProcessor] Ensured Icon-1024.png exists in AppIcon.appiconset");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[WarfestIOSBuildPostProcessor] Failed to ensure app icons: " + ex.Message);
            }
        }
    }
}
#endif
