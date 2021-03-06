using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Box.Security.Data;
using Box.Security.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartGeoIot.ViewModels;
using System.IO;
using System.Globalization;

// for excel
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace SmartGeoIot.Services
{
    public partial class ExcelUtils
    {
        /// <summary>
        /// Exports a collection of results to Excel.
        /// </summary>
        /// <param name="reports">The calc reports</param>
        /// <returns></returns>
        public byte[] ExportReports(DashboardViewModels[] reports, string id, string startDate, string endDate)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
                {
                    workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    // adds style
                    AddStyle();

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Relatório" };
                    sheets.Append(sheet);

                    sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                    // Title
                    if (string.IsNullOrWhiteSpace(startDate))
                        AddTitle($"Relatório do dispositivo {id}.");
                    else
                    {
                        startDate = Convert.ToDateTime(startDate).ToShortDateString();
                        endDate = Convert.ToDateTime(endDate).ToShortDateString();
                        AddTitle($"Relatório do dispositivo {id}, período {startDate} - {endDate}");
                    }
                        

                    // table headers       
                    AddReportTableHeader();

                    // exports the reports
                    foreach (var report in reports)
                    {
                        var row = new Row();

                        AddCell(report.Date.ToShortDateString().ToString(culture), row, style: SGICellStyles.Border);
                        AddCell($"{report.Level.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.Light.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.Temperature.ToString(culture)}°C", row, style: SGICellStyles.Border);
                        AddCell($"{report.Moisture.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.OxigenioDissolvido.ToString(culture)}mg/L", row, style: SGICellStyles.Border);
                        AddCell(report.Ph.ToString(culture), row, style: SGICellStyles.Border);
                        AddCell($"{report.Condutividade.ToString(culture)}μS", row, style: SGICellStyles.Border);

                        sheetData.AppendChild(row);
                    }

                    // adds the footer
                    AddFooter();

                    // create a MergeCells class to hold each MergeCell
                    MergeCells mergeCells = new MergeCells();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:H1") });
                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                    workbookPart.Workbook.Save();
                }
                return mem.ToArray();
            }

        }

        private void AddReportTableHeader()
        {
            //2º Linha
            var row = new Row();
            AddCell("Data", row, style: SGICellStyles.TableHeader);
            AddCell("Nível", row, style: SGICellStyles.TableHeader);
            AddCell("Luz", row, style: SGICellStyles.TableHeader);

            AddCell("Temperatura", row, style: SGICellStyles.TableHeader);
            AddCell("Umidade", row, style: SGICellStyles.TableHeader);
            AddCell("Oxigênio", row, style: SGICellStyles.TableHeader);

            AddCell("Ph", row, style: SGICellStyles.TableHeader);
            AddCell("Condutividade", row, style: SGICellStyles.TableHeader);

            sheetData.AppendChild(row);
        }


        /// <summary>
        /// Exports a collection of results to Excel.
        /// </summary>
        /// <param name="reports">The calc reports</param>
        /// <returns></returns>
        public byte[] ExportReportsPQA(DashboardViewModels[] reports, string id, string startDate, string endDate)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
                {
                    workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    // adds style
                    AddStyle();

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Relatório" };
                    sheets.Append(sheet);

                    sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                    // Title
                    if (string.IsNullOrWhiteSpace(startDate) && startDate.Equals("null"))
                        AddTitle($"Relatório do dispositivo {id}.");
                    else
                    {
                        startDate = startDate.Equals("null") ? "ꝏ" : Convert.ToDateTime(startDate).ToShortDateString();
                        endDate = endDate.Equals("null") ? "ꝏ" : Convert.ToDateTime(endDate).ToShortDateString();
                        AddTitle($"Relatório do dispositivo {id}, período {startDate} - {endDate}");
                    }
                        

                    // table headers
                    AddReportPQATableHeader();

                    // exports the reports
                    foreach (var report in reports)
                    {
                        var row = new Row();

                        AddCell(report.Date.ToShortDateString().ToString(culture), row, style: SGICellStyles.Border);
                        AddCell($"{report.Temperature.ToString(culture)}°C", row, style: SGICellStyles.Border);
                        AddCell(report.Ph.ToString(culture), row, style: SGICellStyles.Border);
                        AddCell($"{report.Fluor.ToString(culture)}ppm", row, style: SGICellStyles.Border);
                        AddCell($"{report.Cloro.ToString(culture)}ppm", row, style: SGICellStyles.Border);
                        AddCell($"{report.Turbidez.ToString(culture)}NTU", row, style: SGICellStyles.Border);

                        AddCell($"{report.Rele?.Rele1.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.Rele?.Rele2.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.Rele?.Rele3.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.Rele?.Rele4.ToString(culture)}%", row, style: SGICellStyles.Border);
                        AddCell($"{report.Rele?.Rele5.ToString(culture)}%", row, style: SGICellStyles.Border);

                        sheetData.AppendChild(row);
                    }

                    // adds the footer
                    AddFooter();

                    // create a MergeCells class to hold each MergeCell
                    MergeCells mergeCells = new MergeCells();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:K1") });
                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                    workbookPart.Workbook.Save();
                }
                return mem.ToArray();
            }

        }

        private void AddReportPQATableHeader()
        {
            //2º Linha
            var row = new Row();
            AddCell("Data", row, style: SGICellStyles.TableHeader);
            AddCell("Temperatura", row, style: SGICellStyles.TableHeader);
            AddCell("Ph", row, style: SGICellStyles.TableHeader);
            AddCell("Flúor", row, style: SGICellStyles.TableHeader);
            AddCell("Cloro", row, style: SGICellStyles.TableHeader);
            AddCell("Turbidez", row, style: SGICellStyles.TableHeader);

            AddCell("Rele 1", row, style: SGICellStyles.TableHeader);
            AddCell("Rele 2", row, style: SGICellStyles.TableHeader);
            AddCell("Rele 3", row, style: SGICellStyles.TableHeader);
            AddCell("Rele 4", row, style: SGICellStyles.TableHeader);
            AddCell("Rele 5", row, style: SGICellStyles.TableHeader);

            sheetData.AppendChild(row);
        }



        /// <summary>
        /// Exports a collection of results to Excel.
        /// </summary>
        /// <param name="reports">The calc reports</param>
        /// <returns></returns>
        public byte[] ExportReportsTRM(DashboardViewModels[] reports, string id, string startDate, string endDate)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
                {
                    workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    // adds style
                    AddStyle();

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Relatório" };
                    sheets.Append(sheet);

                    sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                    // Title
                    if (string.IsNullOrWhiteSpace(startDate) && startDate.Equals("null"))
                        AddTitle($"Relatório do dispositivo {id}.");
                    else
                    {
                        startDate = startDate.Equals("null") ? "ꝏ" : Convert.ToDateTime(startDate).ToShortDateString();
                        endDate = endDate.Equals("null") ? "ꝏ" : Convert.ToDateTime(endDate).ToShortDateString();
                        AddTitle($"Relatório do dispositivo {id}, período {startDate} - {endDate}");
                    }
                        

                    // table headers
                    AddReportTRMTableHeader();

                    // exports the reports
                    foreach (var report in reports)
                    {
                        var row = new Row();

                        AddCell(report.Date.ToString("dd/MM/yyyy HH:mm:ss"), row, style: SGICellStyles.Border);

                        AddCell((bool)report.Bits?.Bed1 ? "On" : "Off", row, style: SGICellStyles.Border);
                        AddCell((bool)report.Bits?.Bed2 ? "On" : "Off", row, style: SGICellStyles.Border);
                        AddCell((bool)report.Bits?.Bed3 ? "On" : "Off", row, style: SGICellStyles.Border);
                        AddCell((bool)report.Bits?.Bed4 ? "On" : "Off", row, style: SGICellStyles.Border);
                        AddCell((bool)report.Bits?.Bsd1 ? "On" : "Off", row, style: SGICellStyles.Border);
                        AddCell((bool)report.Bits?.Bsd2 ? "On" : "Off", row, style: SGICellStyles.Border);

                        AddCell((bool)report.Bits?.BAlertaMin ? "On" : "Off", row, style: SGICellStyles.Border);
                        AddCell((bool)report.Bits?.BAlertaMax ? "On" : "Off", row, style: SGICellStyles.Border);

                        AddCell(report.EntradaAnalogica, row, style: SGICellStyles.Border);
                        AddCell(report.SaidaAnalogica, row, style: SGICellStyles.Border);

                        sheetData.AppendChild(row);
                    }

                    // adds the footer
                    AddFooter();

                    // create a MergeCells class to hold each MergeCell
                    MergeCells mergeCells = new MergeCells();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:K1") });
                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                    workbookPart.Workbook.Save();
                }
                return mem.ToArray();
            }

        }
        private void AddReportTRMTableHeader()
        {
            //2º Linha
            var row = new Row();
            AddCell("Data", row, style: SGICellStyles.TableHeader);
            AddCell("ED1", row, style: SGICellStyles.TableHeader);
            AddCell("ED2", row, style: SGICellStyles.TableHeader);
            AddCell("ED3", row, style: SGICellStyles.TableHeader);
            AddCell("ED4", row, style: SGICellStyles.TableHeader);
            AddCell("SD1", row, style: SGICellStyles.TableHeader);
            AddCell("SD2", row, style: SGICellStyles.TableHeader);
            AddCell("Alerta Mín", row, style: SGICellStyles.TableHeader);
            AddCell("Alerta Max", row, style: SGICellStyles.TableHeader);
            AddCell("Entrada EA10", row, style: SGICellStyles.TableHeader);
            AddCell("Saída SA3", row, style: SGICellStyles.TableHeader);

            sheetData.AppendChild(row);
        }


        /// <summary>
        /// Exports a collection of results to Excel.
        /// </summary>
        /// <param name="reports">The calc reports</param>
        /// <returns></returns>
        public byte[] ExportReportsTRM10(DashboardViewModels[] reports, string id, string startDate, string endDate)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
                {
                    workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    // adds style
                    AddStyle();

                    // create columns
                    Columns columns = new Columns();
                    columns.Append(CreateColumnData(1, 1, 15));
                    columns.Append(CreateColumnData(2, 2, 10));
                    columns.Append(CreateColumnData(3, 5, 15));
                    columns.Append(CreateColumnData(6, 6, 20));
                    worksheetPart.Worksheet.Append(columns);

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Relatório" };
                    sheets.Append(sheet);

                    sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                    // Title
                    if (string.IsNullOrWhiteSpace(startDate) && startDate.Equals("null"))
                        AddTitle($"Relatório do dispositivo {id}.");
                    else
                    {
                        startDate = startDate.Equals("null") ? "ꝏ" : Convert.ToDateTime(startDate).ToShortDateString();
                        endDate = endDate.Equals("null") ? "ꝏ" : Convert.ToDateTime(endDate).ToShortDateString();
                        AddTitle($"Relatório do dispositivo {id}, período {startDate} - {endDate}");
                    }
                        

                    // table headers
                    AddReportTRM10TableHeader();

                    // exports the reports
                    foreach (var report in reports)
                    {
                        var row = new Row();

                        AddCell(report.Date.ToString("dd/MM/yyyy"), row, style: SGICellStyles.Border);
                        AddCell(report.Date.ToString("HH:mm:ss"), row, style: SGICellStyles.Border);

                        AddCell(report.ConsumoAgua, row, style: SGICellStyles.Border);
                        AddCell(report.FluxoAgua, row, style: SGICellStyles.Border);
                        AddCell(report.Modo, row, style: SGICellStyles.Border);
                        AddCell(report.Estado, row, style: SGICellStyles.Border);

                        sheetData.AppendChild(row);
                    }

                    // adds the footer
                    AddFooter();

                    // create a MergeCells class to hold each MergeCell
                    MergeCells mergeCells = new MergeCells();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:F1") });
                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                    workbookPart.Workbook.Save();
                }
                return mem.ToArray();
            }

        }
        private void AddReportTRM10TableHeader()
        {
            //2º Linha
            var row = new Row();
            AddCell("Data", row, style: SGICellStyles.TableHeader);
            AddCell("Hora", row, style: SGICellStyles.TableHeader);
            AddCell("Consumo", row, style: SGICellStyles.TableHeader);
            AddCell("Média", row, style: SGICellStyles.TableHeader);
            AddCell("Modo", row, style: SGICellStyles.TableHeader);
            AddCell("Válvula", row, style: SGICellStyles.TableHeader);

            sheetData.AppendChild(row);
        }
    }
}