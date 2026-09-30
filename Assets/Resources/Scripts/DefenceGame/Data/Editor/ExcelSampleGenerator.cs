using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using UnityEditor;
using UnityEngine;
using DefenceGame.Data;

namespace DefenceGame.Data.Editor
{
    public static class ExcelSampleGenerator
    {
        [MenuItem("Tools/DefenceGame/Create Sample Excel", priority = 10)]
        public static void CreateSampleExcel()
        {
            string excelPath = GameDataEditorMenu.GetExcelPath();
            string directory = Path.GetDirectoryName(excelPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Create(excelPath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = doc.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    Sheets sheets = new Sheets();
                    workbookPart.Workbook.Append(sheets);

                    // Create each sheet
                    CreateEnemiesSheet(workbookPart, sheets);
                    CreateTowersSheet(workbookPart, sheets);
                    CreateWavesSheet(workbookPart, sheets);
                    CreateGachaSheet(workbookPart, sheets);
                    CreateUnitGradesSheet(workbookPart, sheets);

                    workbookPart.Workbook.Save();
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Sample Excel file created at:\n{excelPath}", "OK");
                Debug.Log($"Sample Excel created: {excelPath}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create Excel file:\n{ex.Message}", "OK");
                Debug.LogError($"Failed to create Excel: {ex}");
            }
        }

        private static void CreateEnemiesSheet(WorkbookPart workbookPart, Sheets sheets)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Enemies" };
            sheets.Append(sheet);

            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header
            AddRow(sheetData, "Id", "Name", "Health", "Speed", "RewardGold");

            // Sample data
            AddRow(sheetData, "1", "Slime", "100", "2.0", "10");
            AddRow(sheetData, "2", "Goblin", "200", "3.0", "20");
            AddRow(sheetData, "3", "Orc", "500", "2.5", "50");
            AddRow(sheetData, "4", "DarkKnight", "1500", "2.0", "100");
            AddRow(sheetData, "5", "Dragon", "5000", "1.5", "500");
        }

        private static void CreateTowersSheet(WorkbookPart workbookPart, Sheets sheets)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 2, Name = "Towers" };
            sheets.Append(sheet);

            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header
            AddRow(sheetData, "Id", "Name", "AttackPower", "AttackSpeed", "Range", "Grade");

            // Sample data
            AddRow(sheetData, "1", "Archer", "10", "1.0", "5.0", "Common");
            AddRow(sheetData, "2", "Mage", "25", "0.8", "6.0", "Rare");
            AddRow(sheetData, "3", "Cannon", "50", "0.5", "4.0", "Epic");
            AddRow(sheetData, "4", "Laser", "100", "2.0", "8.0", "Legendary");
        }

        private static void CreateWavesSheet(WorkbookPart workbookPart, Sheets sheets)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 3, Name = "Waves" };
            sheets.Append(sheet);

            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header
            AddRow(sheetData, "WaveNumber", "EnemyId", "Count", "SpawnInterval", "HealthMultiplier");

            // Sample data
            AddRow(sheetData, "1", "1", "5", "2.0", "1.0");
            AddRow(sheetData, "2", "1", "8", "1.8", "1.1");
            AddRow(sheetData, "3", "2", "5", "1.5", "1.2");
            AddRow(sheetData, "5", "3", "3", "2.0", "1.5");
            AddRow(sheetData, "10", "4", "2", "3.0", "2.0");
            AddRow(sheetData, "20", "5", "1", "5.0", "3.0");
        }

        private static void CreateGachaSheet(WorkbookPart workbookPart, Sheets sheets)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 4, Name = "GachaProbabilities" };
            sheets.Append(sheet);

            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header
            AddRow(sheetData, "Grade", "Probability", "Cost");

            // Sample data (60%, 30%, 9%, 1%)
            AddRow(sheetData, "Common", "60", "100");
            AddRow(sheetData, "Rare", "30", "300");
            AddRow(sheetData, "Epic", "9", "1000");
            AddRow(sheetData, "Legendary", "1", "5000");
        }

        private static void CreateUnitGradesSheet(WorkbookPart workbookPart, Sheets sheets)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 5, Name = "UnitGrades" };
            sheets.Append(sheet);

            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header
            AddRow(sheetData, "Grade", "DamageMultiplier", "RangeMultiplier", "AttackSpeedMultiplier");

            // Sample data
            AddRow(sheetData, "Common", "1.0", "1.0", "1.0");
            AddRow(sheetData, "Rare", "1.5", "1.2", "1.1");
            AddRow(sheetData, "Epic", "2.5", "1.5", "1.3");
            AddRow(sheetData, "Legendary", "5.0", "2.0", "1.5");
        }

        private static void AddRow(SheetData sheetData, params string[] values)
        {
            Row row = new Row();
            foreach (string value in values)
            {
                Cell cell = new Cell()
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(value)
                };
                row.Append(cell);
            }
            sheetData.Append(row);
        }
    }
}
