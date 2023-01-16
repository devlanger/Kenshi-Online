using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GameServerBuilder
{
    [MenuItem("Game Server/Build for Ubuntu")]
    public static void BuildGame ()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        string[] levels = new string[] {"Assets/Scenes/GameServerScene.unity"};

        // Build player.
        // BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        // {
        //     scenes = levels,
        //     locationPathName = path + "/gameserver.x86_64",
        //     target = BuildTarget.StandaloneLinux64,
        //     subtarget = (int)StandaloneBuildSubtarget.Server
        // });

        //psi.Arguments = Application.dataPath + "/../tools/scripts/build.sh";
        
        
        FileInfo info = new FileInfo("~/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal");
        System.Diagnostics.Process.Start(info.FullName);
        Speak("x");

        // Run the game (Process class from System.Diagnostics).
        // Process proc = new Process();
        // proc.StartInfo.FileName = path + "/BuiltGame.exe";
        // proc.Start();
    }
    
    private static void Speak (string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("/bin/bash");
        startInfo.WorkingDirectory = Application.dataPath + "/../tools/scripts/";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
 
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
 
        process.StandardInput.WriteLine("say " + command);
        process.StandardInput.WriteLine("exit");  // if no exit then WaitForExit will lockup your program
        process.StandardInput.Flush();
 
        string line = process.StandardOutput.ReadLine();
       
        process.WaitForExit();
    }
}
