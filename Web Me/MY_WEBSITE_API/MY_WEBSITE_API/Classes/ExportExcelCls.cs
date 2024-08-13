using MY_WEBSITE_API.Controllers.Common;
using MY_WEBSITE_API.Models.Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Classes
{
    public class ExportExcelCls
    {
        public byte[] ExportDataTableToExcel(DataTable dt, FileInformation fi)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Color colorColumnTitle = System.Drawing.ColorTranslator.FromHtml("#8adce6");
            ApiCommonController apiCommon = new ApiCommonController();

            //Add No Column
            if (dt != null)
            {
                List<string> columnSortList = new List<string>();
                dt.Columns.Add("#", typeof(int));
                //foreach (DataRow dr in dt.Rows)
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["#"] = (i + 1);
                }
                columnSortList.Add("#");
                dt = apiCommon.DataTableSetColumnsOrder(dt, columnSortList.ToArray());
            }

            using (var memoryStream = new MemoryStream())
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var workbook = excelPackage.Workbook;
                var ws = workbook.Worksheets.Add(fi.FILE_NAME);

                int startRow = 3;
                //int endPoint = 0;
                int TotalColumn = dt.Columns.Count;
                int TotalRow = dt.Rows.Count;

                string CellTitleFrom = GetExcelColumnName(1);
                string CellTitleFromRow = "1";
                string CellTitleTo = "";
                if (TotalColumn <= 3)
                {
                    CellTitleTo = GetExcelColumnName(1);
                    string CellTitleToRow = "2";
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Value = fi.FILE_TITLE;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Bold = true;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Size = 24;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[CellTitleFrom + CellTitleFromRow + ":" + CellTitleTo + CellTitleToRow].Merge = true;
                    //string ApprovedCell = "B1";
                    //string CheckedCell = "C1";
                    //string PreparedCell = "D1";
                    //ws.Cells[ApprovedCell + CellTitleFromRow].Value = "Approved";
                    //ws.Cells[CheckedCell + CellTitleFromRow].Value = "Checked";
                    //ws.Cells[PreparedCell + CellTitleFromRow].Value = "Prepared";
                }
                else
                {
                    CellTitleTo = GetExcelColumnName(TotalColumn);
                    string CellTitleToRow = "2";
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Value = fi.FILE_TITLE;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Bold = true;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Size = 24;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[CellTitleFrom + CellTitleFromRow + ":" + CellTitleTo + CellTitleToRow].Merge = true;

                    //string ApprovedCell = GetExcelColumnName(TotalColumn - 2);
                    //string CheckedCell = GetExcelColumnName(TotalColumn - 1);
                    //string PreparedCell = GetExcelColumnName(TotalColumn);
                    //ws.Cells[ApprovedCell + CellTitleFromRow].Value = "Approved";
                    //ws.Cells[CheckedCell + CellTitleFromRow].Value = "Checked";
                    //ws.Cells[PreparedCell + CellTitleFromRow].Value = "Prepared";
                }

                for (int col = 0; col < TotalColumn; col++)
                {
                    bool ImageCol_Flag = false;
                    bool DateTimeCol_Flag = false;

                    string CellDTHeader = GetExcelColumnName(1 + col) + startRow;
                    ws.Cells[CellDTHeader].Value = dt.Columns[col].ColumnName;
                    ws.Cells[CellDTHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[CellDTHeader].Style.Font.Bold = true;
                    ws.Cells[CellDTHeader].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[CellDTHeader].Style.Fill.BackgroundColor.SetColor(colorColumnTitle);
                    ws.Cells[CellDTHeader].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[CellDTHeader].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[CellDTHeader].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[CellDTHeader].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    for (int row = 0; row < TotalRow; row++)
                    {
                        string CellData = GetExcelColumnName(1 + col) + (startRow + row + 1);
                        object value = dt.Rows[row][col];

                        if (dt.Columns[col].ColumnName.ToLower().Contains("image")) //If any column name has image
                        {
                            string pathValue = value.ToString();
                            string imageName = row.ToString() + col.ToString();
                            if (!string.IsNullOrEmpty(pathValue))
                            {
                                using (System.Drawing.Image image = System.Drawing.Image.FromFile(value.ToString()))
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        image.Save(ms, image.RawFormat);

                                        var picture = ws.Drawings.AddPicture(imageName, ms);
                                        picture.SetPosition(row + 3, 9, col, 10);
                                        picture.SetSize(300, 240);

                                        ImageCol_Flag = true;
                                        ws.Row(row + 4).Height = 211;
                                    }
                                }
                            }
                            /*else
                            {
                                value = "Image not found";
                            }
                            ws.Cells[startRow + row + 1, col + 1].Value = value;*/
                        }
                        else if (dt.Columns[col].ColumnName.ToLower().Contains("picture")) //If any column name has picture
                        {
                            string pathValue = value.ToString();
                            string pictureName = row.ToString() + col.ToString();
                            if (!string.IsNullOrEmpty(pathValue))
                            {
                                using (System.Drawing.Image image = System.Drawing.Image.FromFile(value.ToString()))
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        image.Save(ms, image.RawFormat);

                                        var picture = ws.Drawings.AddPicture(pictureName, ms);
                                        picture.SetPosition(row + 3, 9, col, 10);
                                        picture.SetSize(300, 240);

                                        ImageCol_Flag = true;
                                        ws.Row(row + 4).Height = 211;
                                    }
                                }
                            }
                            /*else
                            {                              
                                value = "Image not found";
                            }
                            ws.Cells[startRow + row + 1, col + 1].Value = value;*/
                        }
                        else
                        {
                            ws.Cells[CellData].Value = value;

                            if (value.GetType() == typeof(int))
                                ws.Cells[CellData].Style.Numberformat.Format = "0";
                            else if (value.GetType() == typeof(double) || value.GetType() == typeof(Decimal) || value.GetType() == typeof(float))
                                ws.Cells[CellData].Style.Numberformat.Format = "0.00";
                            else if (value.GetType() == typeof(DateTime))
                            {
                                ws.Cells[CellData].Style.Numberformat.Format = "DD/MM/YYYY HH:mm:ss";
                                DateTimeCol_Flag = true;
                            }
                            else
                                ws.Cells[CellData].Style.Numberformat.Format = "General";
                        }

                        ws.Cells[CellData].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Cells[CellData].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[CellData].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellData].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellData].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellData].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    string CellDTEnd = GetExcelColumnName(1 + col) + (TotalRow + startRow);
                    string ColumnStartToEnd = CellDTHeader + ":" + CellDTEnd;

                    ws.Cells[ColumnStartToEnd].AutoFitColumns();

                    if (ImageCol_Flag)
                    {
                        ws.Column(col + 1).Width = 48;
                    }

                    if (DateTimeCol_Flag)
                    {
                        double colWidth = ws.Column(col + 1).Width;
                        ws.Column(col + 1).Width = colWidth + 13;
                    }
                }

                excelPackage.Save();

                byte[] excelFileContent = memoryStream.ToArray();

                return memoryStream.ToArray();
            }
        }

        public MemoryStream ExportDataTableToExcelMemoryStream(DataTable dt, FileInformation fi)
        {
            Color colorColumnTitle = System.Drawing.ColorTranslator.FromHtml("#8adce6");
            using (var memoryStream = new MemoryStream())
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var workbook = excelPackage.Workbook;
                var ws = workbook.Worksheets.Add(fi.FILE_NAME);

                int startRow = 3;
                //int endPoint = 0;
                int TotalColumn = dt.Columns.Count;
                int TotalRow = dt.Rows.Count;

                string CellTitleFrom = GetExcelColumnName(1);
                string CellTitleFromRow = "1";
                string CellTitleTo = "";
                if (TotalColumn <= 3)
                {
                    CellTitleTo = GetExcelColumnName(1);
                    string CellTitleToRow = "2";
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Value = fi.FILE_TITLE;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Bold = true;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Size = 24;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[CellTitleFrom + CellTitleFromRow + ":" + CellTitleTo + CellTitleToRow].Merge = true;
                    //string ApprovedCell = "B1";
                    //string CheckedCell = "C1";
                    //string PreparedCell = "D1";
                    //ws.Cells[ApprovedCell + CellTitleFromRow].Value = "Approved";
                    //ws.Cells[CheckedCell + CellTitleFromRow].Value = "Checked";
                    //ws.Cells[PreparedCell + CellTitleFromRow].Value = "Prepared";
                }
                else
                {
                    CellTitleTo = GetExcelColumnName(TotalColumn);
                    string CellTitleToRow = "2";
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Value = fi.FILE_TITLE;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Bold = true;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Size = 24;
                    ws.Cells[CellTitleFrom + CellTitleFromRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[CellTitleFrom + CellTitleFromRow + ":" + CellTitleTo + CellTitleToRow].Merge = true;

                    //string ApprovedCell = GetExcelColumnName(TotalColumn - 2);
                    //string CheckedCell = GetExcelColumnName(TotalColumn - 1);
                    //string PreparedCell = GetExcelColumnName(TotalColumn);
                    //ws.Cells[ApprovedCell + CellTitleFromRow].Value = "Approved";
                    //ws.Cells[CheckedCell + CellTitleFromRow].Value = "Checked";
                    //ws.Cells[PreparedCell + CellTitleFromRow].Value = "Prepared";
                }

                for (int col = 0; col < TotalColumn; col++)
                {
                    bool ImageCol_Flag = false;
                    bool DateTimeCol_Flag = false;

                    string CellDTHeader = GetExcelColumnName(1 + col) + startRow;
                    ws.Cells[CellDTHeader].Value = dt.Columns[col].ColumnName;
                    ws.Cells[CellDTHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[CellDTHeader].Style.Font.Bold = true;
                    ws.Cells[CellDTHeader].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[CellDTHeader].Style.Fill.BackgroundColor.SetColor(colorColumnTitle);
                    ws.Cells[CellDTHeader].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[CellDTHeader].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[CellDTHeader].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[CellDTHeader].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    for (int row = 0; row < TotalRow; row++)
                    {
                        string CellData = GetExcelColumnName(1 + col) + (startRow + row + 1);
                        object value = dt.Rows[row][col];

                        if (dt.Columns[col].ColumnName.ToLower().Contains("image")) //If any column name has image
                        {
                            using (System.Drawing.Image image = System.Drawing.Image.FromFile(value.ToString()))
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    image.Save(ms, image.RawFormat);

                                    var picture = ws.Drawings.AddPicture(row.ToString(), ms);
                                    picture.SetPosition(row + 3, 9, col, 10);
                                    picture.SetSize(300, 240);

                                    ImageCol_Flag = true;
                                    ws.Row(row + 4).Height = 211;
                                }
                            }
                        }
                        else
                        {
                            ws.Cells[CellData].Value = value;

                            if (value.GetType() == typeof(int))
                                ws.Cells[CellData].Style.Numberformat.Format = "0";
                            else if (value.GetType() == typeof(double) || value.GetType() == typeof(Decimal) || value.GetType() == typeof(float))
                                ws.Cells[CellData].Style.Numberformat.Format = "0.00";
                            else if (value.GetType() == typeof(DateTime))
                            {
                                ws.Cells[CellData].Style.Numberformat.Format = "DD/MM/YYYY HH:mm:ss";
                                DateTimeCol_Flag = true;
                            }
                            else
                                ws.Cells[CellData].Style.Numberformat.Format = "General";
                        }

                        ws.Cells[CellData].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Cells[CellData].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[CellData].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellData].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellData].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellData].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    string CellDTEnd = GetExcelColumnName(1 + col) + (TotalRow + startRow);
                    string ColumnStartToEnd = CellDTHeader + ":" + CellDTEnd;

                    ws.Cells[ColumnStartToEnd].AutoFitColumns();

                    if (ImageCol_Flag)
                    {
                        ws.Column(col + 1).Width = 48;
                    }

                    if (DateTimeCol_Flag)
                    {
                        double colWidth = ws.Column(col + 1).Width;
                        ws.Column(col + 1).Width = colWidth + 13;
                    }
                }

                excelPackage.Save();

                return memoryStream;
            }
        }

        public byte[] ExportDataTableToExcel(List<DataTable> _dt, List<FileInformation> _fi)
        {
            Color colorColumnTitle = System.Drawing.ColorTranslator.FromHtml("#8adce6");
            using (var memoryStream = new MemoryStream())
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var workbook = excelPackage.Workbook;

                for (int i = 0; i < _dt.Count; i++)
                {
                    var ws = workbook.Worksheets.Add(_fi[i].FILE_NAME);

                    int startRow = 3;
                    //int endPoint = 0;
                    int TotalColumn = _dt[i].Columns.Count;
                    int TotalRow = _dt[i].Rows.Count;

                    string CellTitleFrom = GetExcelColumnName(1);
                    string CellTitleFromRow = "1";
                    string CellTitleTo = "";
                    if (TotalColumn <= 3)
                    {
                        CellTitleTo = GetExcelColumnName(1);
                        string CellTitleToRow = "2";
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Value = _fi[i].FILE_TITLE;
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Bold = true;
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Size = 24;
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[CellTitleFrom + CellTitleFromRow + ":" + CellTitleTo + CellTitleToRow].Merge = true;

                    }
                    else
                    {
                        CellTitleTo = GetExcelColumnName(TotalColumn);
                        string CellTitleToRow = "2";
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Value = _fi[i].FILE_TITLE;
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Bold = true;
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Style.Font.Size = 24;
                        ws.Cells[CellTitleFrom + CellTitleFromRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[CellTitleFrom + CellTitleFromRow + ":" + CellTitleTo + CellTitleToRow].Merge = true;

                    }

                    for (int col = 0; col < TotalColumn; col++)
                    {
                        bool DateTimeCol_Flag = false;

                        string CellDTHeader = GetExcelColumnName(1 + col) + startRow;
                        ws.Cells[CellDTHeader].Value = _dt[i].Columns[col].ColumnName;
                        ws.Cells[CellDTHeader].Style.Font.Bold = true;
                        ws.Cells[CellDTHeader].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[CellDTHeader].Style.Fill.BackgroundColor.SetColor(colorColumnTitle);
                        ws.Cells[CellDTHeader].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellDTHeader].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellDTHeader].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        ws.Cells[CellDTHeader].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        for (int row = 0; row < TotalRow; row++)
                        {
                            string CellData = GetExcelColumnName(1 + col) + (startRow + row + 1);
                            object value = _dt[i].Rows[row][col];
                            ws.Cells[CellData].Value = value;

                            if (value.GetType() == typeof(int))
                                ws.Cells[CellData].Style.Numberformat.Format = "0";
                            else if (value.GetType() == typeof(double) || value.GetType() == typeof(Decimal) || value.GetType() == typeof(float))
                                ws.Cells[CellData].Style.Numberformat.Format = "0.00";
                            else if (value.GetType() == typeof(DateTime))
                            {
                                ws.Cells[CellData].Style.Numberformat.Format = "DD/MM/YYYY HH:mm:ss";
                                DateTimeCol_Flag = true;
                            }
                            else
                                ws.Cells[CellData].Style.Numberformat.Format = "General";

                            ws.Cells[CellData].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            ws.Cells[CellData].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[CellData].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            ws.Cells[CellData].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            ws.Cells[CellData].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            ws.Cells[CellData].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }
                        string CellDTEnd = GetExcelColumnName(1 + col) + (TotalRow + startRow);
                        string ColumnStartToEnd = CellDTHeader + ":" + CellDTEnd;

                        ws.Cells[ColumnStartToEnd].AutoFitColumns();

                        if (DateTimeCol_Flag)
                        {
                            double colWidth = ws.Column(col + 1).Width;
                            ws.Column(col + 1).Width = colWidth + 13;
                        }
                    }
                }

                excelPackage.Save();

                byte[] excelFileContent = memoryStream.ToArray();

                return memoryStream.ToArray();
            }
        }

        public static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}