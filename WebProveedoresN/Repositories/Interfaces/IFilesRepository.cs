using WebProveedoresN.Models;

namespace WebProveedoresN.Repositories.Interfaces
{
    public interface IFilesRepository
    {
        Task<bool> BuscarFacturaAsync(string UUID);
        Task<List<FileModel>> GetDocumentsAsync(int orderNumber);
        Task SaveFileToDatabaseAsync(FileModel archivo);
        Task<string> SaveXmlDataInDatabaseAsync(List<LecturaXmlModel> archivos, int orderNumber, string supplierName, string idUsuario, string ipUsuario);
        List<LecturaXmlModel> GetDataFromXml(string xmlContent);
        string ConvertXmlToPdf(string xmlContent, string pdfFilePath);
        Task SaveFilesToDatabaseAsync(List<FileModel> archivos);
        Task<List<CFDIModel>> GetCFDIStatusAsync(CFDIStatusModel cFDIStatusModel);
        Task<bool> SearchInvoiceAsync(string UUID);

    }
}
