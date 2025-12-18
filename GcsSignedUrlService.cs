using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using System;


namespace L.R._Gcaleka__Co
{
    public class GcsSignedUrlService
    {
        private readonly UrlSigner _urlSigner;
        private readonly string _bucketName;

        public GcsSignedUrlService(IConfiguration configuration)
        {
            _bucketName = configuration["GCP:BucketName"];
            var credentialsPath = configuration["GCP:CredentialsPath"];
            var credential = GoogleCredential.FromFile(credentialsPath);
            _urlSigner = UrlSigner.FromServiceAccountCredential((ServiceAccountCredential)credential.UnderlyingCredential);
        }

        public string GenerateSignedUrl(string fileUrl, TimeSpan duration)
        {
            return _urlSigner.Sign(
                _bucketName,
               fileUrl,
                duration,
                HttpMethod.Get
                );
        }
    }
}
