using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class BuildPostProcessor
{


    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // Read.
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            //string targetName = project.GetUnityFrameworkTargetGuid() // note, not "project." ...
            string targetGuid = project.GetUnityFrameworkTargetGuid();

            AddFrameworks(project, targetGuid);

            // Write.
            File.WriteAllText(projectPath, project.WriteToString());
        }
    }

    static void AddFrameworks(PBXProject project, string targetGuid)
    {
        // Frameworks (eppz! Photos, Google Analytics).
        //project.AddFrameworkToProject(targetGUID, "MessageUI.framework", false);
        //project.AddFrameworkToProject(targetGUID, "AdSupport.framework", false);
        //project.AddFrameworkToProject(targetGUID, "CoreData.framework", false);
        //project.AddFrameworkToProject(targetGUID, "SystemConfiguration.framework", false);
        project.AddFrameworkToProject(targetGuid, "libz.dylib", false);
        //project.AddFrameworkToProject(targetGUID, "libsqlite3.tbd", false);
        project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

        // Add `-ObjC` to "Other Linker Flags".
        //project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");
    }
}