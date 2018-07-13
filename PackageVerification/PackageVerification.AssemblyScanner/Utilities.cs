﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PackageVerification.AssemblyScanner
{
    public class Utilities
    {
        public static void ExportDotNetNukeAssembliesToCSV(List<Data.DotNetNukeAssembliesInfo> dnnAssembliesColl)
        {
            var assemblyName = new List<string>();

            foreach (var dotNetNukeAssembliesInfo in dnnAssembliesColl)
            {
                var assemblyList = dotNetNukeAssembliesInfo.AssemblyList;

                foreach (var assembly in assemblyList)
                {
                    if (!assemblyName.Contains(assembly.Name))
                    {
                        assemblyName.Add(assembly.Name);
                    }
                }
            }

            assemblyName.Sort();

            var rows = assemblyName.Count + 1;
            var columns = dnnAssembliesColl.Count + 1;

            const string filePath = @"..\..\..\DNNPackages\test.csv";
            const string delimiter = ",";
            var output = new string[rows, columns];

            output[0, 0] = "Assembly Name";

            var i = 1;

            foreach (var name in assemblyName)
            {
                output[i, 0] = name;
                i++;
            }

            var k = 1;

            foreach (var dotNetNukeAssembliesInfo in dnnAssembliesColl)
            {
                output[0, k] = dotNetNukeAssembliesInfo.DNNVersion.Name;

                for (var j = 1; j < rows; j++)
                {
                    var name = output[j, 0];

                    foreach (var assemblyItem in dotNetNukeAssembliesInfo.AssemblyList)
                    {
                        if (name == assemblyItem.Name)
                        {
                            output[j, k] = assemblyItem.Version;
                            break;
                        }

                        output[j, k] = "";
                    }
                }

                k++;
            }

            var sb = new StringBuilder();
            for (var r = 0; r < rows; r++)
            {
                var row = "";

                for (var c = 0; c < columns; c++)
                {
                    row += '"' + output[r, c] + '"' + delimiter;
                }

                sb.AppendLine(row);
            }

            File.WriteAllText(filePath, sb.ToString()); 
        }

        public static List<string> GetSubFolders(List<string> folders, string parentDir)
        {
            foreach (var directory in Directory.GetDirectories(parentDir))
            {
                folders.Add(directory);
                folders.AddRange(GetSubFolders(folders, directory));
            }

            return folders.Distinct().ToList();
        }

        public static int GetVersionSortOrder(string version)
        {
            var nameParts = version.Split('.');

            int major = 0;
            int minor = 0;
            int sub = 0;

            int.TryParse(nameParts[0], out major);
            int.TryParse(nameParts[1], out minor);
            int.TryParse(nameParts[2], out sub);

            return ((major * 100000) + (minor * 1000) + (sub * 10));
        }
    }
}
