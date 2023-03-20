using AspNetCore.Aws.S3.Simple.AmazonServices;
using AspNetCore.Aws.S3.Simple.Contracts;
using AspNetCore.Aws.S3.Simple.Settings;
using Domain.Files;

namespace Infrastructure.Services.Files;

public class CvStorageS3Service : AmazonS3StorageBase, ICvStorage
{
    public CvStorageS3Service(
        S3StorageSettings configuration,
        IS3FileValidator fileValidator)
        : base(configuration, fileValidator, "candidate-cv")
    {
    }
}