using System;
using AspNetCore.Aws.S3.Simple.Models;
using FileService.Contracts;
using Moq;

namespace TestUtils.Mocks;

public class CvStorageMock : Mock<ICvStorage>
{
    public CvStorageMock(
        Func<IUploadFileRequest, FileUploadResult> uploadReturnFunc = null,
        Func<string, bool> deleteReturnFunc = null)
    {
        Setup(x => x.UploadFileAsync(
            It.IsAny<IUploadFileRequest>()))
            .ReturnsAsync(uploadReturnFunc ?? (request => FileUploadResult.Success(request.FileName)));

        Setup(x => x.DownloadFileAsync(
            It.IsAny<string>()));

        Setup(x => x.DeleteFileAsync(
            It.IsAny<string>()))
            .ReturnsAsync(deleteReturnFunc ?? ((request) => true));
    }

    public void VerifyUpload(Times times)
    {
        Verify(x => x.UploadFileAsync(It.IsAny<IUploadFileRequest>()), times);
    }

    public void VerifyDownload(Times times)
    {
        Verify(x => x.DownloadFileAsync(It.IsAny<string>()), times);
    }

    public void VerifyDelete(Times times)
    {
        Verify(x => x.DeleteFileAsync(It.IsAny<string>()), times);
    }
}