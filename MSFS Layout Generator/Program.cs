using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Unicode;

namespace MSFSLayoutGenerator
{
    class Program
    {
        static void Main(string[] layoutPaths)
        {
            if(layoutPaths.Count() == 0)
            {
                Utilities.Log(new  string[] { "No layout.json paths specified.", "Usage: MSFSLayoutGenerator.exe <layout.json> ..." });
            }
            else
            {
                foreach (string path in layoutPaths)
                {
                    Layout layout = new Layout();
                    string layoutPath = Path.GetFullPath(path);
                    string json;

                    cleanDirs(layoutPath);

                    if (string.Equals(Path.GetFileName(layoutPath), "layout.json", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string file in Directory.GetFiles(Path.GetDirectoryName(layoutPath), "*.*", SearchOption.AllDirectories))
                        {
                            string relativePath = Utilities.GetRelativePath(file, Path.GetDirectoryName(layoutPath));

                            Content content = new Content();
                            content.Path = relativePath;
                            content.Size = new FileInfo(file).Length;
                            content.Date = new FileInfo(file).LastWriteTimeUtc.ToFileTimeUtc();

                            //if (!relativePath.StartsWith("_CVT_", StringComparison.OrdinalIgnoreCase) && !string.Equals(relativePath, "business.json") && !string.Equals(relativePath, "layout.json") && !string.Equals(relativePath, "manifest.json"))
                            if (isok(relativePath))
                            {
                                layout.Content.Add(content);
                            }
                        }

                        if (layout.Content.Count() == 0)
                        {
                            Utilities.Log("No files were found in the folder containing \"" + layoutPath + "\". The layout.json will not be updated.");
                        }

                        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                        jsonOptions.WriteIndented = true;
                        jsonOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All);

                        json = JsonSerializer.Serialize(layout, jsonOptions);

                        try
                        {
                            File.WriteAllText(layoutPath, json.Replace("\r\n", "\n"));
                        }
                        catch (Exception ex)
                        {
                            Utilities.Log("Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        Utilities.Log("The file \"" + layoutPath + "\" is not named layout.json and will not be updated.");
                    }
                }
            }
        }

        static void cleanDirs(string layoutPath)
        {
            FileInfo fi = new FileInfo(layoutPath);
            DirectoryInfo di = fi.Directory;

            foreach (DirectoryInfo tdi in di.GetDirectories())
            {
                if (tdi.Name.ToLower() == "scenery")
                {
                    foreach (DirectoryInfo tdi2 in tdi.GetDirectories())
                    {
                        if (tdi2.Name.ToLower() == "global")
                        {
                            string dest = Path.Combine(tdi.FullName, di.Name + "_global");
                            if (Directory.Exists(dest)) throw new Exception("Directory already exists: " + dest);
                            Directory.Move(tdi2.FullName, dest);
                        }
                        if (tdi2.Name.ToLower() == "world")
                        {
                            string dest = Path.Combine(tdi.FullName, di.Name + "_world");
                            if (Directory.Exists(dest)) throw new Exception("Directory already exists: " + dest);
                            Directory.Move(tdi2.FullName, dest);
                        }
                    }
                }

                if (tdi.Name.ToLower() == "materiallibs")
                {
                    foreach (DirectoryInfo tdi2 in tdi.GetDirectories())
                    {
                        if (tdi2.Name.ToLower() == "mymaterials")
                        {
                            string dest = Path.Combine(tdi.FullName, di.Name);
                            if (Directory.Exists(dest)) throw new Exception("Directory already exists: " + dest);
                            Directory.Move(tdi2.FullName, dest);
                        }
                    }
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

    }
}