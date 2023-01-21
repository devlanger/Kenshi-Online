using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class GameServerBuilder
{
    [MenuItem("Building/Build Game Client for MacOS")]
    public static void BuildGameClient ()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Game Client", "", "");
        string[] levels = new string[] {"Assets/Scenes/MenuScene.unity", "Assets/Scenes/GameRoomScene.unity"};
        UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.x64ARM64;
        
        BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        {
            scenes = levels,
            locationPathName = path + "/kenshi.app",
            target = BuildTarget.StandaloneOSX
        });
    }
    
    [MenuItem("Building/Build for Game Server for Ubuntu")]
    public static void BuildGameServer ()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        string[] levels = new string[] {"Assets/Scenes/GameServerScene.unity"};

        BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        {
            scenes = levels,
            locationPathName = path + "/gameserver.x86_64",
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server
        });
    }
    
    [MenuItem("Building/Deploy Game Server")]
    public static void DeployGameServer ()
    {
        ExecuteShellScript("Deploying Game Server", "/Users/piotrlanger/RiderProjects/KenshiBackend/tools/gameserver/", "deploy.sh");
    }
    
    [MenuItem("Building/Deploy Backend")]
    public static void DeployBackend ()
    {
        ExecuteShellScript("Deploying Backend", "/Users/piotrlanger/RiderProjects/KenshiBackend/tools/backend/", "deploy.sh");
    }
    
    [MenuItem("Building/Deploy GameServer And Backend")]
    public static void DeployGameServerAndBackend ()
    {
        EditorUtility.DisplayProgressBar("Deploying Game Server", "Running deployment...", 0.1f);
        ExecuteShellScript("Deploying Game Server", "/Users/piotrlanger/RiderProjects/KenshiBackend/tools/gameserver/", "deploy.sh", false);
        EditorUtility.DisplayProgressBar("Deploying Backend", "Running deployment...", 0.75f);
        ExecuteShellScript("Deploying Backend", "/Users/piotrlanger/RiderProjects/KenshiBackend/tools/backend/", "deploy.sh", false);
        EditorUtility.ClearProgressBar();
    }
    
    private static void ExecuteShellScript(string title, string path, string script, bool loadingScreen = true)
    {
        try
        {
            if(loadingScreen)
                EditorUtility.DisplayProgressBar(title, "Running deployment...", 0.1f);
            
            string argument = script;
            UnityEngine.Debug.Log("============== Start Executing [" + argument + "] ===============");
            ProcessStartInfo startInfo = new ProcessStartInfo("/bin/bash")
            {
                WorkingDirectory = path,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Process myProcess = new Process
            {
                StartInfo = startInfo
            };
            myProcess.StartInfo.Arguments = argument;
            myProcess.Start();
            string output = myProcess.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Result for [" + argument + "] is : \n" + output);
            myProcess.WaitForExit();
            UnityEngine.Debug.Log("============== End ===============");
            
            if(loadingScreen)
                EditorUtility.ClearProgressBar();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
            
            if(loadingScreen)
                EditorUtility.ClearProgressBar();
        }
    }
}
