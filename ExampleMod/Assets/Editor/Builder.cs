using UnityEngine;
using MultiMod.Editor;
    
class Builder 
{
    static void PerformBuild()
    {
        Debug.Log("### BUILDING ###");
        ExporterEditorWindow.ExportMod();
    }
}