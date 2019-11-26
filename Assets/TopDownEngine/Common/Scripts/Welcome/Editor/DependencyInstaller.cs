using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    public class DependencyInstaller : Editor
    {
        public class AfterImport : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string importedAsset in importedAssets)
                {
                    if (importedAsset.Contains("DependencyInstaller.cs"))
                    {
                        bool installHappened = false;
                        DependencyInstaller.DefineSymbol.RemoveDefineSymbol(WelcomeWindow.DEFINE_POSTPROCESSING_INSTALLED);
                        DependencyInstaller.DefineSymbol.RemoveDefineSymbol(WelcomeWindow.DEFINE_CINEMACHINE_INSTALLED);
                        DependencyInstaller.DefineSymbol.RemoveDefineSymbol(WelcomeWindow.DEFINE_PIXELPERFECT_INSTALLED);

                        if (!PackageInstallation.IsInstalled(WelcomeWindow.PostProcessingPackageID))
                        {
                            InstallPostProcessing();
                            installHappened = true;
                        }

                        if (!PackageInstallation.IsInstalled(WelcomeWindow.CinemachinePackageID))
                        {
                            InstallCinemachine();
                            installHappened = true;
                        }

                        if (!PackageInstallation.IsInstalled(WelcomeWindow.PixelPerfectPackageID))
                        {
                            InstallPixelPerfect();
                            installHappened = true;
                        }

                        if (installHappened)
                        {
                            AssetDatabase.Refresh();
                            ReloadCurrentScene();
                        }

                        WelcomeWindow.ShowWindow();
                    }
                }
            }
        }

        public static void ReloadCurrentScene()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.OpenScene(currentScenePath);
        }

        public static void InstallPostProcessing()
        {
            if (!PackageInstallation.IsInstalled(WelcomeWindow.PostProcessingPackageID))
            {
                PackageInstallation.Install(WelcomeWindow.PostProcessingPackageVersionID);

            }
            DefineSymbol.AddDefineSymbol(WelcomeWindow.DEFINE_POSTPROCESSING_INSTALLED);

        }

        public static void InstallCinemachine()
        {
            if (!PackageInstallation.IsInstalled(WelcomeWindow.CinemachinePackageID))
            {
                PackageInstallation.Install(WelcomeWindow.CinemachinePackageVersionID);
            }
            DefineSymbol.AddDefineSymbol(WelcomeWindow.DEFINE_CINEMACHINE_INSTALLED);

        }

        public static void InstallPixelPerfect()
        {
            if (!PackageInstallation.IsInstalled(WelcomeWindow.PixelPerfectPackageID))
            {
                PackageInstallation.Install(WelcomeWindow.PixelPerfectPackageVersionID);
            }
            DefineSymbol.AddDefineSymbol(WelcomeWindow.DEFINE_PIXELPERFECT_INSTALLED);
            
        }

        public static void TestDependencies()
        {
            if (PackageInstallation.IsInstalled(WelcomeWindow.PostProcessingPackageID))
            {
                DefineSymbol.AddDefineSymbol(WelcomeWindow.DEFINE_POSTPROCESSING_INSTALLED);
            }
            if (PackageInstallation.IsInstalled(WelcomeWindow.CinemachinePackageID))
            {
                DefineSymbol.AddDefineSymbol(WelcomeWindow.DEFINE_CINEMACHINE_INSTALLED);
            }
            if (PackageInstallation.IsInstalled(WelcomeWindow.PixelPerfectPackageID))
            {
                DefineSymbol.AddDefineSymbol(WelcomeWindow.DEFINE_PIXELPERFECT_INSTALLED);
            }
        }
        
        public class DefineSymbol : Editor
        {
            public static void AddDefineSymbol(string symbol)
            {
                var buildTargets = Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>().Where(x => x != BuildTargetGroup.Unknown).Where(x => !ObsoleteBuild(x)); 
                foreach (var buildTarget in buildTargets)
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Trim();
                    var definesList = defines.Split(';', ' ').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    if (definesList.Contains(symbol))
                    {
                        continue;
                    }
                    definesList.Add(symbol);
                    defines = definesList.Aggregate((a, b) => a + ";" + b);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
                }
            }

            public static void RemoveDefineSymbol(string symbol)
            {
                var buildTargets = Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>().Where(x => x != BuildTargetGroup.Unknown).Where(x => !ObsoleteBuild(x)); 
                foreach (var buildTarget in buildTargets)
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Trim();
                    var definesList = defines.Split(';', ' ').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    if (definesList.Contains(symbol))
                    {
                        definesList.Remove(symbol);
                    }
                    if (definesList.Count > 0)
                    {
                        defines = definesList.Aggregate((a, b) => a + ";" + b);
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
                    }                    
                }
            }

            private static bool ObsoleteBuild(BuildTargetGroup group)
            {
                var attributes = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
                return ( (attributes.Length > 0) && (attributes != null) );
            }
        }
    }

    public class PackageInstallation
    {                
        public static bool IsInstalled(string packageID)
        {
            string packagesFolder = Application.dataPath + "/../Packages/";
            string manifestFile = packagesFolder + "manifest.json";
            string manifest = File.ReadAllText(manifestFile);

            return manifest.Contains(packageID);
        }

        public static void Install(string packageVersionID)
        {
            Client.Add(packageVersionID);
        }
    }
}
