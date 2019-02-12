using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using System.ComponentModel;
using System.Windows;

namespace MovieInfo
{
    public class ImportExcel
    {
        private BackgroundWorker mWorker;
        Excel.Workbook mWorkbook;
        Excel.Application mExcelApp;
        private Range mRange;

        public ImportExcel(BackgroundWorker worker, string filePath)
        {
            this.mWorker = worker;

            Init(filePath);
        }

        private void Init(string filePath)
        {
            mExcelApp = new Excel.Application();
            mWorkbook = mExcelApp.Workbooks.Open(filePath);
            mRange = ((Excel.Worksheet)mWorkbook.Worksheets.get_Item(1)).UsedRange;
        }

        public ImportExcel(string filePath)
        {
            Init(filePath);
        }

        public int GetRowCount() { return mRange.Rows.Count;}

        public System.Data.DataTable Import()
        {
            string strCellData = "";
            double douCellData;
            int rowCnt = 0;
            int colCnt = 0;
            int progress = 0;

            System.Data.DataTable dt = new System.Data.DataTable();

            for (colCnt = 1; colCnt <= mRange.Columns.Count; colCnt++)
            {
                string strColumn = "";
                strColumn = (string)(mRange.Cells[1, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                dt.Columns.Add(strColumn, typeof(string));
            }

            mWorker.ReportProgress(++progress);

            for (rowCnt = 2; rowCnt <= mRange.Rows.Count; rowCnt++)
            {
                if (mWorker.CancellationPending) break;
                mWorker.ReportProgress(++progress);

                string strData = "";
                for (colCnt = 1; colCnt <= mRange.Columns.Count; colCnt++)
                {
                    try
                    {
                        var cellData = (mRange.Cells[rowCnt, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;

                        if (cellData != null)
                        {
                            strCellData = cellData.ToString();
                            strData += strCellData + "|";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[INFO] "+ex.Message);
                        douCellData = (mRange.Cells[rowCnt, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                        strData += douCellData.ToString() + "|";
                    }
                }
                strData = strData.Remove(strData.Length - 1, 1);
                dt.Rows.Add(strData.Split('|'));
            }

            mWorkbook.Close(true, Missing.Value, Missing.Value);
            mExcelApp.Quit();

            return dt;
        }
    }

    /// <summary>
    /// Class for generator of Excel file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class ExportToExcel<T, U>
        where T : class
        where U : List<T>
    {
        public List<T> dataToPrint;
        // Excel object references.
        private Microsoft.Office.Interop.Excel.Application _excelApp = null;
        private Microsoft.Office.Interop.Excel.Workbooks _books = null;
        private Microsoft.Office.Interop.Excel._Workbook _book = null;
        private Microsoft.Office.Interop.Excel.Sheets _sheets = null;
        private Microsoft.Office.Interop.Excel._Worksheet _sheet = null;
        private Microsoft.Office.Interop.Excel.Range _range = null;
        private Microsoft.Office.Interop.Excel.Font _font = null;

        // Optional argument variable
        private object _optionalValue = Missing.Value;

        /// <summary>
        /// Generate report and sub functions
        /// </summary>
        public void GenerateReport()
        {
            try
            {
                if (dataToPrint != null)
                {
                    if (dataToPrint.Count != 0)
                    {
                        Mouse.SetCursor(System.Windows.Input.Cursors.Wait);
                        CreateExcelRef();
                        FillSheet();
                        OpenReport();
                        Mouse.SetCursor(System.Windows.Input.Cursors.Arrow);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                MetroMessageBox.ShowErrorMessage(null, "Error while generating Excel report", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }
            finally
            {
                ReleaseObject(_sheet);
                ReleaseObject(_sheets);
                ReleaseObject(_book);
                ReleaseObject(_books);
                ReleaseObject(_excelApp);
            }
        }
        /// <summary>
        /// Make Microsoft Excel application visible
        /// </summary>
        private void OpenReport()
        {
            _excelApp.Visible = true;
        }
        /// <summary>
        /// Populate the Excel sheet
        /// </summary>
        private void FillSheet()
        {
            object[] header = CreateHeader();
            WriteData(header);
        }
        /// <summary>
        /// Write data into the Excel sheet
        /// </summary>
        /// <param name="header"></param>
        private void WriteData(object[] header)
        {
            object[,] objData = new object[dataToPrint.Count, header.Length];

            for (int j = 0; j < dataToPrint.Count; j++)
            {
                var item = dataToPrint[j];
                for (int i = 0; i < header.Length; i++)
                {
                    var y = typeof(T).InvokeMember
            (header[i].ToString(), BindingFlags.GetProperty, null, item, null);
                    objData[j, i] = (y == null) ? "" : y.ToString();
                }
            }
            AddExcelRows("A2", dataToPrint.Count, header.Length, objData);
            AutoFitColumns("A1", dataToPrint.Count + 1, header.Length);
        }
        /// <summary>
        /// Method to make columns auto fit according to data
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.Columns.AutoFit();
        }
        /// <summary>
        /// Create header from the properties
        /// </summary>
        /// <returns></returns>
        private object[] CreateHeader()
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            // Create an array for the headers and add it to the
            // worksheet starting at cell A1.
            List<object> objHeaders = new List<object>();
            for (int n = 0; n < headerInfo.Length; n++)
            {
                objHeaders.Add(headerInfo[n].Name);
            }

            var headerToAdd = objHeaders.ToArray();
            AddExcelRows("A1", 1, headerToAdd.Length, headerToAdd);
            SetHeaderStyle();

            return headerToAdd;
        }
        /// <summary>
        /// Set Header style as bold
        /// </summary>
        private void SetHeaderStyle()
        {
            _font = _range.Font;
            _font.Bold = true;
        }
        /// <summary>
        /// Method to add an excel rows
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="values"></param>
        private void AddExcelRows
        (string startRange, int rowCount, int colCount, object values)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.set_Value(_optionalValue, values);
        }
        /// <summary>
        /// Create Excel application parameters instances
        /// </summary>
        private void CreateExcelRef()
        {
            _excelApp = new Microsoft.Office.Interop.Excel.Application();
            _books = (Microsoft.Office.Interop.Excel.Workbooks)_excelApp.Workbooks;
            _book = (Microsoft.Office.Interop.Excel._Workbook)(_books.Add(_optionalValue));
            _sheets = (Microsoft.Office.Interop.Excel.Sheets)_book.Worksheets;
            _sheet = (Microsoft.Office.Interop.Excel._Worksheet)(_sheets.get_Item(1));
        }
        /// <summary>
        /// Release unused COM objects
        /// </summary>
        /// <param name="obj"></param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                obj = null;
                MetroMessageBox.ShowErrorMessage(null, ex.Message.ToString(), "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
