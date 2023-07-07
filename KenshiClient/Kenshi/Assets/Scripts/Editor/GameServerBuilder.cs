using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MS.Shell.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameServerBuilder
{
    private static int progressId;

    private const string UNITY_EDITOR_PATH = "/Applications/Unity/Hub/Editor/2022.3.1f1/Unity.app/Contents/MacOS/Unity";
    private const string UNITY_PROJECT_PATH = "/Users/piotrlanger/Repositories/Kenshi/KenshiClient/Kenshi_clone_0";
    private const string GAMESERVER_DEPLOY_SHELL_SCRIPT_PATH = "/Users/piotrlanger/Repositories/Kenshi/tools/gameserver/docker_deploy.sh";
    private const string GAMESERVER_BUILD_PATH = @"/Users/piotrlanger/Repositories/Kenshi/Builds/gameserver";
    private static List<string> _scenesToBuild = new List<string>()
    {
        "Assets/Scenes/GameServerScene.unity", 
        "Assets/Scenes/Map_Forest.unity",
        "Assets/Scenes/Map_Dungeon1.unity",
        "Assets/Scenes/Map_Crypt.unity",
    };

    [MenuItem("Building/Build and Deploy server")]
    public static async void BuildDeployServerDocker()
    {
        progressId = Progress.Start("Deploying Backend", "Running deployment...");
        Progress.Report(progressId, 0.25f);
        await BuildWithOtherEditor();
        Progress.Report(progressId, 0.65f);
        await DeployServerDocker();
        Progress.Remove(progressId);
    }

    private static async Task BuildWithOtherEditor()
    {
        var arg =
            $@"{UNITY_EDITOR_PATH} -quit -batchmode -logFile build_log.txt -projectPath {UNITY_PROJECT_PATH} -executeMethod GameServerBuilder.BuildGameServer";

        await ExecuteShellScript("Building Server", arg);
    }
    
    [MenuItem("Building/Deploy server docker")]
    public static async Task DeployServerDocker()
    {
        await ExecuteShellScript("Deploying Server", GAMESERVER_DEPLOY_SHELL_SCRIPT_PATH);
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("Building/Build Game Server for Ubuntu")]
    public static void BuildGameServer ()
    {
        string path = GAMESERVER_BUILD_PATH;

        BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        {
            scenes = _scenesToBuild.ToArray(),
            locationPathName = path + "/gameserver.x86_64",
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server
        });
        EditorUtility.ClearProgressBar();
    }

    private static async Task ExecuteShellScript(string title, string script, bool loadingScreen = true)
    {
        try
        {
            var operation = EditorShell.Execute(script);
            operation.onExit += (exitCode)=>{
    
            };
            operation.onLog += (EditorShell.LogType LogType,string log)=>{
                UnityEngine.Debug.Log(log);
                Progress.Report(progressId, 0.8f, log);
            };

            int exitCode = await operation; //support async/await
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }
}
