using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Heat_Production_Optimization.Services;

public interface IFilesService
{
    public Task<IStorageFile?> UploadFileAsync();
}