using UnityEngine;
using UnityEditor;

namespace DefenceGame.Data.Editor
{
    public class GameDataSettingsWindow : EditorWindow
    {
        private string _excelPath;
        private string _outputPath;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/DefenceGame/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameDataSettingsWindow>("GameData Settings");
            window.minSize = new Vector2(400, 200);
        }

        private void OnEnable()
        {
            _excelPath = GameDataEditorMenu.GetExcelPath();
            _outputPath = GameDataEditorMenu.GetOutputPath();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Excel Converter Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Excel File Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Excel File Path:", GUILayout.Width(120));
            _excelPath = EditorGUILayout.TextField(_excelPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFilePanel("Select Excel File", "Assets", "xlsx");
                if (!string.IsNullOrEmpty(path))
                {
                    _excelPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Output Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Path:", GUILayout.Width(120));
            _outputPath = EditorGUILayout.TextField(_outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _outputPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            // Save Button
            if (GUILayout.Button("Save Settings", GUILayout.Height(30)))
            {
                GameDataEditorMenu.SetExcelPath(_excelPath);
                GameDataEditorMenu.SetOutputPath(_outputPath);

                // 설정 변경 시 파일 감시 재시작
                if (EditorPrefs.GetBool("DefenceGame_AutoConvert", false))
                {
                    EditorApplication.delayCall += GameDataEditorMenu.ToggleAutoConvert;
                    EditorApplication.delayCall += GameDataEditorMenu.ToggleAutoConvert;
                }

                EditorUtility.DisplayDialog("Settings Saved", "Settings have been saved successfully!", "OK");
            }

            EditorGUILayout.Space(10);

            // Convert Now Button
            if (GUILayout.Button("Convert Now", GUILayout.Height(30)))
            {
                GameDataEditorMenu.ConvertExcel();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Make sure your Excel file has sheets named:\n" +
                "• Enemies\n" +
                "• Towers\n" +
                "• Waves\n" +
                "• GachaProbabilities\n" +
                "• UnitGrades\n\n" +
                "Each sheet's first row should contain field names.",
                MessageType.Info
            );

            EditorGUILayout.EndScrollView();
        }
    }
}
