using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;

namespace L.R._Gcaleka__Co
{
    public class CloudStorageService
    {
        private readonly string _localPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        //private readonly StorageClient _storageClient;
        //private readonly string _bucketName = "lrgcaleka-uploads";

        public CloudStorageService()
        {
            if (!Directory.Exists(_localPath))
            {
                Directory.CreateDirectory(_localPath);
            }
            //_storageClient = StorageClient.Create();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {

            if(file == null || file.Length == 0)
            {
                return null;
            }

            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}"; //This generates a unique name for the file so for instance if the file name is  document.pdf it would be generated to c3b2bhbd_document.pdf
            string fullPath = Path.Combine(_localPath, uniqueFileName);
            /*using var stream = file.OpenReadStream();*/ //uploads the file for reading and prepares it for uploading.

            using(var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //Uncomment when using cloud again
            //await _storageClient.UploadObjectAsync(_bucketName, uniqueFileName, file.ContentType, stream);//uploads the file and sends files to the bucket
            //return $"https://storage.googleapis.com/{_bucketName}/{uniqueFileName}"; //returns the URL with unique filename


            //try
            //{
            //    using var fileStream = File.OpenRead(filePath);
            //    await _storageClient.UploadObjectAsync(_bucketName, fileName, null, fileStream);
            //    return $"https://storage.googleapis.com/{_bucketName}/{fileName}";
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error uploading file:{ex.Message}");
            //    return null;
            //}
            return $"/Uploads/{uniqueFileName}";
        }
        public string GetFilUrl(string fileName)
        {
            return $"/Uploads/{fileName}";
            /*return $"https://storage.googleapis.com/{_bucketName}/{fileName}"; *///direct link to the file in my gcs bucket
        }

        public async Task DownloadFileAsync(string fileName, string destinationPath)
        {
            string sourcePath = Path.Combine(_localPath, fileName);

            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            //using var outputFile = System.IO.File.OpenWrite(destinantionPath);
            //await _storageClient.DownloadObjectAsync(_bucketName, fileName, outputFile);

            await sourceStream.CopyToAsync(destinationStream);
        }

    }
}
