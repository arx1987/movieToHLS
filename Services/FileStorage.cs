namespace MovieToHLS.Services;

public class FileStorage
{
    //  /video/{guid}/{guid}.m3u8 |  /video/{guid}/{guid}0.ts
    public async Task UploadFile(Stream fileStream, string path)
    {
        using var output = File.Create(path);
        await fileStream.CopyToAsync(output);
    }
}