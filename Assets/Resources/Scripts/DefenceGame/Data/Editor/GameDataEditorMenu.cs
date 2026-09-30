using System.IO;
using UnityEngine;
using UnityEditor;

namespace DefenceGame.Data.Editor
{
    public static class GameDataEditorMenu
    {
        private const string EXCEL_PATH_KEY = "DefenceGame_ExcelPath";
        private const string OUTPUT_PATH_KEY = "DefenceGame_OutputPath";
        private const string AUTO_CONVERT_KEY = "DefenceGame_AutoConvert";

        private static FileSystemWatcher _watcher;
        private static string _watchedExcelPath;

        [MenuItem("Tools/DefenceGame/Convert Excel to GameData", priority = 1)]
        public static void ConvertExcel()
        {
            string excelPath = GetExcelPath();
            string outputPath = GetOutputPath();

            if (string.IsNullOrEmpty(excelPath) || !File.Exists(excelPath))
            {
                EditorUtility.DisplayDialog("Error", "Excel file not found. Please set the path in Tools > DefenceGame > Settings", "OK");
                return;
            }

            ExcelConverter.ConvertExcelToScriptableObject(excelPath, outputPath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Excel converted successfully!", "OK");
        }

        [MenuItem("Tools/DefenceGame/Settings", priority = 2)]
        public static void OpenSettings()
        {
            GameDataSettingsWindow.ShowWindow();
        }

        [MenuItem("Tools/DefenceGame/Toggle Auto Convert", priority = 3)]
        public static void ToggleAutoConvert()
        {
            bool isEnabled = !EditorPrefs.GetBool(AUTO_CONVERT_KEY, false);
            EditorPrefs.SetBool(AUTO_CONVERT_KEY, isEnabled);

            if (isEnabled)
            {
                StartFileWatcher();
                Debug.Log("Auto Convert: Enabled");
            }
            else
            {
                StopFileWatcher();
                Debug.Log("Auto Convert: Disabled");
            }

            Menu.SetChecked("Tools/DefenceGame/Toggle Auto Convert", isEnabled);
        }

        [MenuItem("Tools/DefenceGame/Toggle Auto Convert", true)]
        public static bool ToggleAutoConvertValidate()
        {
            Menu.SetChecked("Tools/DefenceGame/Toggle Auto Convert", EditorPrefs.GetBool(AUTO_CONVERT_KEY, false));
            return true;
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // 에디터 시작 시 자동 변환이 켜져 있으면 파일 감시 시작
            if (EditorPrefs.GetBool(AUTO_CONVERT_KEY, false))
            {
                StartFileWatcher();
            }
        }

        private static void StartFileWatcher()
        {
            StopFileWatcher();

            string excelPath = GetExcelPath();
            if (string.IsNullOrEmpty(excelPath) || !File.Exists(excelPath))
            {
                Debug.LogWarning("Cannot start file watcher: Excel file not found");
                return;
            }

            string directory = Path.GetDirectoryName(excelPath);
            string fileName = Path.GetFileName(excelPath);

            _watchedExcelPath = excelPath;
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnExcelFileChanged;

            Debug.Log($"File watcher started for: {fileName}");
        }

        private static void StopFileWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Changed -= OnExcelFileChanged;
                _watcher.Dispose();
                _watcher = null;
                Debug.Log("File watcher stopped");
            }
        }

        private static void OnExcelFileChanged(object sender, FileSystemEventArgs e)
        {
            // 파일이 변경되면 Unity 메인 스레드에서 변환 실행
            EditorApplication.delayCall += () =>
            {
                Debug.Log($"Excel file changed: {e.FullPath}");
                ConvertExcel();
            };
        }

        public static string GetExcelPath()
        {
            return EditorPrefs.GetString(EXCEL_PATH_KEY, "Assets/Data/GameData.xlsx");
        }

        public static void SetExcelPath(string path)
        {
            EditorPrefs.SetString(EXCEL_PATH_KEY, path);
        }

        public static string GetOutputPath()
        {
            return EditorPrefs.GetString(OUTPUT_PATH_KEY, "Assets/ScriptableObjects");
        }

        public static void SetOutputPath(string path)
        {
            EditorPrefs.SetString(OUTPUT_PATH_KEY, path);
        }
    }
}
