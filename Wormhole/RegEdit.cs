using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Wormhole
{
    public class RegEdit
    {
        public static readonly string[] FileExtensions = new string[] { "png", "webp", "jpg", "jpeg", "bmp" };
        internal static readonly string[] RegKeys = new string[] { "*", "Folder" };
        internal static readonly string AppName = "Wormhole";

        internal static void init() {
            bool hasRegistered = HasRegistered();
            if (hasRegistered)
            {
                Console.WriteLine($"当前已安装{AppName}，请选择操作：");
                Console.WriteLine("1-卸载.");
                Console.WriteLine("2-重新安装/修复.");
                string? input = Console.ReadLine();
                if (input == "1")
                {
                    unregister();
                } else if (input == "2")
                {
                    register();
                }
            } else {
                register();
            }
        }

        private static void register() {
            string? currentApiKey = getApiKey();
            string? apiKey = currentApiKey;
            if (currentApiKey != null) {
                Console.WriteLine("当前ApiKey为：" + currentApiKey);
                Console.WriteLine("是否更新？(Y)");
                string? input = Console.ReadLine();
                if (input == "Y")
                {
                    Console.WriteLine("请输入已激活的Tinify ApiKey:");
                    apiKey = Console.ReadLine();
                }
            }

            if (apiKey == null || apiKey == "")
            {
                Console.WriteLine("ApiKey无效！安装中断. . .");
                return;
            }

            string currentDir = Environment.CurrentDirectory;
            string docDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Process currentProcess = Process.GetCurrentProcess();
            string currentPath = currentProcess.MainModule?.FileName ?? "";

            string docAppDir = Path.Combine(docDir, AppName);
            string docPath = Path.Combine(docAppDir, $"{AppName}.exe");
            bool isRegistryToCurrentDir = false;

            if (!docDir.Equals(currentDir))
            {
                Console.WriteLine("是否在当前路径完成注册？(Y)");
                Console.WriteLine("默认注册路径为：" + docPath);
                string? input = Console.ReadLine();
                if (input == "Y") {
                    isRegistryToCurrentDir = true;
                }
            }

            string fullPath = isRegistryToCurrentDir ? currentPath : docPath;
            string command = $"\"{fullPath}\" \"%1\"";

            if (!isRegistryToCurrentDir)
            {
                if (!Directory.Exists(docAppDir)) Directory.CreateDirectory(docAppDir);
                File.Copy(currentPath, docPath, true);
            }

            foreach (string key in RegKeys)
            {
                string rootKey = key + $"\\shell\\{AppName}";
                using (RegistryKey rk = ensureSubKey(Registry.ClassesRoot, rootKey))
                {
                    RegistryKey commandKey = ensureSubKey(rk, "command");
                    commandKey.SetValue(null, command);
                    if (key == "*")
                    {
                        rk.SetValue("path", fullPath);
                        rk.SetValue("apiKey", apiKey);
                        string[] exts= new string[FileExtensions.Length];
                        for (int i = 0; i < FileExtensions.Length; i++)
                        {
                            exts[i] = $"System.FileName:\"*.{FileExtensions[i]}\"";
                        }
                        rk.SetValue("AppliesTo", String.Join(" OR ", exts));
                    }
                }
            }

            Console.WriteLine("安装完成！");
        }

        private static void unregister() {
            foreach (string key in RegKeys)
            {
                string rootKey = key + $"\\shell\\{AppName}";
                using (RegistryKey? rk = Registry.ClassesRoot.OpenSubKey(rootKey, true))
                {
                    if (rk == null) continue;
                    string? path = rk.GetValue("path")?.ToString();
                    if (path == null) continue;
                    if (File.Exists(path)) File.Delete(path);
                    string? dir = Path.GetDirectoryName(path);
                    if (dir == null || !dir.EndsWith(AppName)) continue;
                    if (Directory.Exists(dir)) Directory.Delete(dir, true);
                }
                Registry.ClassesRoot.DeleteSubKeyTree(rootKey);
            }
        }

        private static bool HasRegistered()
        {
            foreach (string key in RegKeys)
            {
                string rootKey = key + $"\\shell\\{AppName}";
                using (RegistryKey? rk = Registry.ClassesRoot.OpenSubKey(rootKey))
                {
                    if (rk == null) return false;
                }
            }

            return true;
        }

        private static RegistryKey ensureSubKey(RegistryKey root, string key) {
            RegistryKey? rk = root.OpenSubKey(key, true);
            if (rk != null) return rk;
            return root.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
        }

        public static string? getApiKey()
        {
            foreach (string key in RegKeys)
            {
                string rootKey = key + $"\\shell\\{AppName}";
                using (RegistryKey? rk = Registry.ClassesRoot.OpenSubKey(rootKey))
                {
                    if (rk == null) continue;
                    return rk.GetValue("apiKey")?.ToString();
                }
            }
            return null;
        }
    }
}
