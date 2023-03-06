using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace AppSample.Admin.Helpers;

public static class ExcelFileExportHelper
{
    public static FileResult Export(string fileName, ExcelPackage package)
    {
        var fileContent = package.GetAsByteArray();
        var result = new FileContentResult(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        result.FileDownloadName = fileName;
        return result;
    }
}