namespace MovieToHLS.Services;

public class FileStorage
{
    //  /video/{slug}/{slug}.m3u8 |  /video/{slug}/{slug}0.ts
    public async Task UploadFile(Stream fileStream, string path)
    {
        using var output = File.Create(path);
        await fileStream.CopyToAsync(output);
    }
}