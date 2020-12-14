using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using Scheduling.Models;
//http://www.nitrix-reloaded.com/2010/09/26/creating-excel-files-from-dataset-using-openxml-20-c-sharp/
namespace Scheduling.OpenXml
{
    public class Utility
    {
        public static void InsertWorksheet(string filepath)
        {
            // Open the document for editing.
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(filepath, true))
            {
                // Add a blank WorksheetPart.
                WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet(new SheetData());

                Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);

                // Get a unique ID for the new worksheet.
                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                // Give the new worksheet a name.
                string sheetName = "Sheet" + sheetId;

                // Append the new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
                sheets.Append(sheet);
            }

        }
                

        public static void CreateSpreadsheetWorkbook(string filepath,List<ProjectDisplay> PdList)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            //comment start
            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();

            //test code start

            InsertWorksheet(filepath);
            InsertWorksheet(filepath);

        }

    }
}