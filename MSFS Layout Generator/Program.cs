using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Unicode;
using System.Collections.Generic;

namespace MSFSLayoutGenerator
{
    class Program
    {
        static void Main(string[] paths)
        {
            if(paths.Count() == 0)
            {
                Utilities.alertBox("No paths specified." + Environment.NewLine + "Usage: MSFSLayoutGenerator.exe <layout.json>|<dir_path> ..." );
            }
            else
            {
                foreach (string path in paths)
                {
                    if (File.Exists(path))
                    {
                        FileInfo fi = new FileInfo(path);
                        if (string.Equals(fi.Name, "layout.json", StringComparison.OrdinalIgnoreCase))
                        {
                            processOneLayoutPath(path);
                        }
                        else
                        {
                            Utilities.alertBox("The file \"" + path + "\" is not named layout.json and will not be updated.");
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        DirectoryInfo di = new DirectoryInfo(path);
                        processDir(di);
                    }
                    else
                    {
                        Utilities.alertBox("Path not found: " + path);
                    }
                }
    
                if (processedFiles.Count > 1)
                {
                    string t = "Layout Files Processed:" + Environment.NewLine + Environment.NewLine;
                    foreach (string path in processedFiles)
                    {
                        t = t + path + Environment.NewLine;
                    }
                    Utilities.infoBox(t);
                }
            }
        }

        static void processDir(DirectoryInfo dirInf)
        {
            FileInfo[] files = dirInf.GetFiles("*.json");
            foreach (FileInfo fi in files)
            {
                if (string.Equals(fi.Name, "layout.json", StringComparison.OrdinalIgnoreCase))
                {
                    processOneLayoutPath(fi.FullName);
                    return;
                }
            }

            DirectoryInfo[] dirs = dirInf.GetDirectories();
            foreach (DirectoryInfo di in dirs)
            {
                processDir(di);
            }
        }

        static void processOneLayoutPath(string layoutPath)
        {
            Layout layout = new Layout();
            string json;

            try
            {
                cleanDirs(layoutPath);
            }
            catch (Exception ex)
            {
                Utilities.errorBox("Error: " + ex.Message);
            }

            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(layoutPath), "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Utilities.GetRelativePath(file, Path.GetDirectoryName(layoutPath));

                Content content = new Content();
                content.Path = relativePath;
                content.Size = new FileInfo(file).Length;
                content.Date = new FileInfo(file).LastWriteTimeUtc.ToFileTimeUtc();

                if (isok(relativePath))
                {
                    layout.Content.Add(content);
                }
            }

            if (layout.Content.Count() == 0)
            {
                Utilities.alertBox("No files were found in the folder containing \"" + layoutPath + "\". The layout.json will not be updated.");
            }
            else
            {
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                jsonOptions.WriteIndented = true;
                jsonOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All);

                json = JsonSerializer.Serialize(layout, jsonOptions);

                try
                {
                    File.WriteAllText(layoutPath, json.Replace("\r\n", "\n"));
                    processedFiles.Add(layoutPath);
                }
                catch (Exception ex)
                {
                    Utilities.errorBox("Error: " + ex.Message);
                }
            }
        }

        static void cleanDirs(string layoutPath)
        {
            FileInfo fi = new FileInfo(layoutPath);
            DirectoryInfo di = fi.Directory;
            string randFileName = Path.GetRandomFileName();

            foreach (DirectoryInfo tdi in di.GetDirectories())
            {
                if (tdi.Name.ToLower() == "scenery" || tdi.Name.ToLower() == "materiallibs")
                {
                    string dest = tdi.FullName + "_" + randFileName;
                    if (Directory.Exists(dest)) throw new Exception("Directory already exists: " + dest);
                    Directory.Move(tdi.FullName, dest);
                }
            }
        }

        static bool isok(string relativePath)
        {
            if (relativePath.StartsWith("_CVT_", StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.Equals(relativePath, "business.json"))
                return false;

            if (string.Equals(relativePath, "layout.json"))
                return false;

            if (string.Equals(relativePath, "manifest.json"))
                return false;

            if (string.Equals(relativePath, "holdingdata.xml"))
                return false;

            if (string.Equals(relativePath.ToLower(), "readme.md"))
                return false;

            return true;
        }

        static List<string> processedFiles = new List<string>();
    }
}