using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BezelEditorSettings
{
    static UnityEditor.Build.NamedBuildTarget target = UnityEditor.Build.NamedBuildTarget.Standalone;

    static string defineSymbols = "ANOTHER_IMPORTER_HAS_HIGHER_PRIORITY";

    static BezelEditorSettings()
    {

        AddBezelDefineSymbols();
    }

    static void AddBezelDefineSymbols()
    {
        string defines = PlayerSettings.GetScriptingDefineSymbols(target);

        // Critical: Only add once, and not to affect existing project define symbols
        if (!defines.Contains(defineSymbols))
        {
            defines += ";" + defineSymbols;

            // Set the updated define symbols for the active build target group
            PlayerSettings.SetScriptingDefineSymbols(target, defines);
        }
    }
}