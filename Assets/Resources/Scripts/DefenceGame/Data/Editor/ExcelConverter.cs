using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using ExcelDataReader;
using UnityEngine;

namespace DefenceGame.Data.Editor
{
    public static class ExcelConverter
    {
        public static GameDataSO ConvertExcelToScriptableObject(string excelPath, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            string assetPath = Path.Combine(outputPath, "GameData.asset");
            string unityAssetPath = "Assets/" + assetPath.Substring(assetPath.IndexOf("Assets/") + 7).Replace("\\", "/");
            
            // Check if asset already exists
            GameDataSO gameData = UnityEditor.AssetDatabase.LoadAssetAtPath<GameDataSO>(unityAssetPath);
            
            if (gameData == null)
            {
                // Create new instance if doesn't exist
                gameData = ScriptableObject.CreateInstance<GameDataSO>();
                Debug.Log("Creating new GameData.asset");
            }
            else
            {
                Debug.Log("Updating existing GameData.asset");
            }
            
            using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    
                    // GameDataSO의 모든 List<T> 필드를 리플렉션으로 찾기
                    FieldInfo[] fields = typeof(GameDataSO).GetFields(BindingFlags.Public | BindingFlags.Instance);
                    
                    foreach (FieldInfo field in fields)
                    {
                        // List<T> 타입인지 확인
                        if (field.FieldType.IsGenericType && 
                            field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            // 시트명은 필드명과 일치해야 함
                            string sheetName = field.Name;
                            
                            // 엑셀에서 해당 시트 찾기
                            DataTable table = result.Tables[sheetName];
                            if (table != null)
                            {
                                // List<T>의 T 타입 가져오기
                                Type elementType = field.FieldType.GetGenericArguments()[0];
                                
                                // 시트 데이터를 List<T>로 파싱
                                IList list = ParseDataTableToList(table, elementType);
                                
                                // GameDataSO의 필드에 할당
                                field.SetValue(gameData, list);
                                
                                Debug.Log($"Converted sheet '{sheetName}' with {list.Count} rows");
                            }
                            else
                            {
                                Debug.LogWarning($"Sheet '{sheetName}' not found in Excel file");
                            }
                        }
                    }
                }
            }
            
            // Save asset
            if (UnityEditor.AssetDatabase.Contains(gameData))
            {
                // Asset already exists, just mark as dirty
                UnityEditor.EditorUtility.SetDirty(gameData);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log($"GameData updated at: {unityAssetPath}");
            }
            else
            {
                // Create new asset
                UnityEditor.AssetDatabase.CreateAsset(gameData, unityAssetPath);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log($"GameData saved to: {unityAssetPath}");
            }
            
            return gameData;
        }
        
        private static IList ParseDataTableToList(DataTable table, Type elementType)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            
            if (table.Rows.Count < 2) return list; // 헤더만 있으면 리턴
            
            // 첫 번째 행은 헤더
            DataRow headerRow = table.Rows[0];
            string[] headers = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                headers[i] = headerRow[i]?.ToString();
            }
            
            // 데이터 행 파싱 (2번째 행부터)
            for (int rowIndex = 1; rowIndex < table.Rows.Count; rowIndex++)
            {
                DataRow row = table.Rows[rowIndex];
                object instance = Activator.CreateInstance(elementType);
                
                for (int colIndex = 0; colIndex < table.Columns.Count; colIndex++)
                {
                    string fieldName = headers[colIndex];
                    if (string.IsNullOrEmpty(fieldName)) continue;
                    
                    FieldInfo field = elementType.GetField(fieldName);
                    if (field != null)
                    {
                        string cellValue = row[colIndex]?.ToString();
                        object convertedValue = ConvertValue(cellValue, field.FieldType);
                        field.SetValue(instance, convertedValue);
                    }
                }
                
                list.Add(instance);
            }
            
            return list;
        }
        
        private static object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
            
            try
            {
                // Enum 타입 처리
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, value, true);
                }
                
                // 기본 타입 변환
                if (targetType == typeof(int))
                    return int.Parse(value);
                if (targetType == typeof(float))
                    return float.Parse(value);
                if (targetType == typeof(double))
                    return double.Parse(value);
                if (targetType == typeof(bool))
                    return bool.Parse(value);
                if (targetType == typeof(string))
                    return value;
                
                // 그 외 타입은 ChangeType 사용
                return Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert '{value}' to {targetType.Name}: {ex.Message}");
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }
    }
}
