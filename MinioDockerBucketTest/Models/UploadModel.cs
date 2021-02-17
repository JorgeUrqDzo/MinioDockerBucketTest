using Microsoft.AspNetCore.Http;

namespace MinioDockerBucketTest.Models
{
    public class UploadModel
    {
        public IFormFile File { get; set; }

    }
}
