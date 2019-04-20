using UnityEditor;
using UnityEngine;

using MultiMod.Shared;

namespace MultiMod.Editor
{
    internal class ExporterEditorWindow : EditorWindow
    {
        private EditorScriptableSingleton<ExportSettings> exportSettings;
        private UnityEditor.Editor exportSettingsEditor;

        [MenuItem("MultiMod/Export Mod")]
        public static void ShowWindow()
        {
            var window = GetWindow<ExporterEditorWindow>();
            window.maxSize = new Vector2(385f, 265);
            window.minSize = new Vector2(300f, 265);
            window.titleContent = new GUIContent("MultiMod Exporter");
        }

        private void OnEnable()
        {
            exportSettings = new EditorScriptableSingleton<ExportSettings>();
            exportSettingsEditor = UnityEditor.Editor.CreateEditor(exportSettings.instance);
        }

        private void OnDisable()
        {
            DestroyImmediate(exportSettingsEditor);
        }

        private void OnGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling && !Application.isPlaying;

            exportSettingsEditor.OnInspectorGUI();

            GUILayout.FlexibleSpace();

            var buttonPressed = GUILayout.Button("Export", GUILayout.Height(30));

            if (buttonPressed)
                Export.ExportMod(exportSettings.instance);
        }
    }
}