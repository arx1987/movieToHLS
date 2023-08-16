using System.Diagnostics;

namespace MovieToHLS.Services;

public class FFmpegHelper
{
    public static FileInfo[] RunMyProcess(FileInfo videoFile, DirectoryInfo convertedDir, string fileName)
    {
        var toFile = FolderCreator(videoFile, convertedDir, fileName);

        using (Process p = new Process())
        {
            try
            {
                var param =
                    $"D:\\Programming\\Git\\GitProjects\\MovieToHLS\\ffmpeg_exe\\bin\\ffmpeg.exe -i \"{videoFile.FullName}\"" +
                    $" -vcodec: copy -acodec: copy" +
                    $" -map 0" +
                    $" -start_number 0" +
                    $" -hls_time 15 -hls_list_size 0" +
                    $" \"{toFile.FullName}\"";

                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false; //использовать ли shell для запуска
                p.StartInfo.RedirectStandardInput = true; //принимает ввод от вызывающей программы
                p.StartInfo.RedirectStandardOutput = true; //Получить выходную информацию от вызывающей программы
                p.StartInfo.RedirectStandardError = true; //Перенаправить стандартный вывод ошибок
                p.StartInfo.CreateNoWindow = false; //если false - Не показывать окно программы
                p.Start(); //старт программы))
                           //Отправить входную информацию в окно cmd
                p.StandardInput.WriteLine(param + "&&exit");
                p.StandardInput.AutoFlush = true;
                p.StandardInput.Close();
                //Получить выходные данные окна cmd
                string output = p.StandardError.ReadToEnd(); //Вы можете вывести результат, чтобы просмотреть конкретную причину ошибки

                //ждем завершения выполнения программы и выхода из процесса
                p.WaitForExit();

                if (p.ExitCode != 0) throw new Exception("не удалось перекодировать видео");
                p.Close();

                return toFile.Directory!.EnumerateFiles().ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public static FileInfo FolderCreator(FileInfo videoFile, DirectoryInfo convertedDir, string fileName)
    {
        FileInfo toFile;
        var dirAr1 = videoFile.Directory.FullName.Replace(convertedDir.Parent.FullName, "");

        var videoFileDir = new DirectoryInfo(videoFile.Directory.FullName);
        if (videoFileDir.GetFiles("*" + videoFile.Extension).Length > 1)
        {
            //здесь не учитывается, если в директории будет лежать несколько видео файлов с разными расширениями, к примеру .avi и .mp4
            var dirAr = dirAr1 == "" ? "" : dirAr1.Remove(0, 1);//убираем первый слеш
            DirectoryInfo di = new(Path.Combine(convertedDir.FullName, dirAr, videoFile.Name.Replace(videoFile.Extension, "")));
            if (!di.Exists) di.Create();
            toFile = new(Path.Combine(di.FullName, fileName + ".m3u8"));

        }
        else
        {
            toFile = new FileInfo(Path.Combine(convertedDir.FullName, fileName + ".m3u8"));
        }
        //if (!toFile.Exists) toFile.Create();
        return toFile;
    }
}
