using System;
using System.IO;
using System.Linq;
using UGameTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_IOS

#endif


public sealed class PublishTools
{

    [MenuItem("Publish/LinuxServer")]
    public static void PublishLinuxServer()
    {
        //EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        //BuildPipeline.BuildPlayer()
    }

    [MenuItem("Publish/Ios/Dev")]
    public static void PublishIsoDev()
    {

    }
    [MenuItem("Publish/Ios/Release")]
    public static void PublishIsoRelease()
    {

    }

    private static void Build(string output, BuildTarget plat, BuildOptions op = BuildOptions.AcceptExternalModificationsToPlayer)
    {
        var scenes = EditorBuildSettings.scenes.Where(t => t.enabled).Select(t => t.path).ToArray();
        Debug.Log($"Out:{output} -> {plat}");
        BuildPipeline.BuildPlayer(scenes, output, plat, op);
    }

    private static Tuple< BuildOptions, string, BuildTarget, BuildTargetGroup> ParserArgs()
    {
        var output = "publish/iOS";
        bool dev = false;
        var target = BuildTarget.iOS;
        var group = BuildTargetGroup.iOS;
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {

                case "-d":
                    {
                        dev = true;
                    }
                    break;
                case "-o":
                    {
                        output = args[++i];
                    }
                    break;

                case "-p":
                    {
                        switch (args[++i])
                        {
                            case "ios":
                                target = BuildTarget.iOS;
                                group = BuildTargetGroup.iOS;
                                break;
                            case "android":
                                target = BuildTarget.Android;
                                group = BuildTargetGroup.Android;
                                break;
                            case "linux":
                                target = BuildTarget.StandaloneLinux64;
                                group = BuildTargetGroup.Standalone;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }
        }

        BuildOptions op = BuildOptions.None;
        if (dev) op |= (BuildOptions.Development| BuildOptions.EnableDeepProfilingSupport);
        
        return new Tuple<BuildOptions, string, BuildTarget, BuildTargetGroup>(op, output, target, group);
    }

    public static void BeginBuild()
    {
        (BuildOptions op, string output, BuildTarget plat, _) = ParserArgs();
        Build(output, plat, op);
    }
}
