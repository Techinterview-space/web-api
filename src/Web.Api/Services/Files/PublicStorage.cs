using AspNetCore.Aws.S3.Simple.AmazonServices;
using AspNetCore.Aws.S3.Simple.Contracts;
using AspNetCore.Aws.S3.Simple.Settings;
using Domain.Files;

namespace TechInterviewer.Services.Files;

public class PublicStorage : AmazonS3StorageBase, IPublicStorage
{
    public PublicStorage(
        S3StorageSettings configuration,
        IS3FileValidator fileValidator)
        : base(configuration, fileValidator, "public-storage")
    {
    }
}