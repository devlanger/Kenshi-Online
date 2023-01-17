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
}
