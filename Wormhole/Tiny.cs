using TinyPng;

namespace Wormhole
{
    internal class Tiny
    {
        internal static readonly string ApiKeyFileName = "key.txt";

        internal static async Task run(string targetPath) {
            string? apiKey = RegEdit.getApiKey();
            if (apiKey == null)
            {
                Console.WriteLine("未找到已安装的ApiKey，请校对后重试. . .");
                return;
            }

            var tinify = new TinyPngClient(apiKey);

            if (Directory.Exists(targetPath))
            {
                string[] fe = RegEdit.FileExtensions;
                var files = Directory.EnumerateFiles(targetPath, "*.*", SearchOption.AllDirectories).Where(file => fe.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase))); ;

                foreach (var file in files)
                {
                    var result = await tinify.Compress(file);
                    await downloadFile(result.Output.Url, file);
                }
            } else
            {

                var result = await tinify.Compress(targetPath);
                await downloadFile(result.Output.Url, targetPath);
            }
        }

        internal static async Task downloadFile(string url, string filePath)
        {
            string localDirectory = Path.GetDirectoryName(filePath)!;
            string fileName = Path.GetFileName(filePath);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 检查文件是否已存在
                    if (File.Exists(filePath))
                    {
                        // 如果文件已存在，生成新的文件名
                        string newFileName = GenerateNewFileName(localDirectory, fileName);
                        filePath = Path.Combine(localDirectory, newFileName);
                    }

                    // 下载文件并保存到本地
                    // 发送 GET 请求并获取响应内容的字节数组
                    byte[] fileBytes = await client.GetByteArrayAsync(url);

                    // 将字节数组写入本地文件
                    File.WriteAllBytes(filePath, fileBytes);

                    Console.WriteLine("File downloaded successfully. At: " + filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error downloading file: " + ex.Message);
                }
            }
        }

        // 生成新的文件名，避免重复
        internal static string GenerateNewFileName(string directory, string fileName)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string newFileName = baseName + "_1" + extension;
            int counter = 1;

            while (File.Exists(Path.Combine(directory, newFileName)))
            {
                counter++;
                newFileName = $"{baseName}_{counter}{extension}";
            }

            return newFileName;
        }
    }
}
