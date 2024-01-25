using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CraftSynth.BuildingBlocks.IO
{
    public class EmbededResources
    {
        /// Source: https://www.geekality.net/2008/12/27/how-to-use-assembly-embedded-resources/
        public static string GetEmbeddedResourceAsString(string filePathToEmbededResourceRelativeToProjectFolder, Assembly assembly)
        {
            string r = null;

            filePathToEmbededResourceRelativeToProjectFolder = FormatResourceName(assembly, filePathToEmbededResourceRelativeToProjectFolder);
            using (Stream resourceStream = assembly.GetManifestResourceStream(filePathToEmbededResourceRelativeToProjectFolder))
            {
                if (resourceStream == null)
                    return null;

                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    r = reader.ReadToEnd();
                }
            }

            return r;
        }

        /// Source: https://www.geekality.net/2008/12/27/how-to-use-assembly-embedded-resources/
        public static Stream GetEmbeddedResourceAsStream(string filePathToEmbededResourceRelativeToProjectFolder, Assembly assembly = null)
        {
            Stream r = null;

            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            filePathToEmbededResourceRelativeToProjectFolder = FormatResourceName(assembly, filePathToEmbededResourceRelativeToProjectFolder);
            r = assembly.GetManifestResourceStream(filePathToEmbededResourceRelativeToProjectFolder);

            return r;
        }

        /// <summary>
        /// Notice that spaces are replaced with underscores and path separators are replaced with periods. 
        /// This is would make a resource's name with path "Folder/file one.txt" be "Folder.file_one.txt".
        /// 
        /// Source: https://www.geekality.net/2008/12/27/how-to-use-assembly-embedded-resources/
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        private static string FormatResourceName(Assembly assembly, string resourceName)
        {
            //debug info for developer:
            var rns = assembly.GetManifestResourceNames().ToList();
            foreach (string rn in rns)
            {
                System.Diagnostics.Debug.WriteLine(rn);
            }

            return assembly.GetName().Name + "." + resourceName.Replace(" ", "_")
                                                               .Replace("\\", ".")
                                                               .Replace("/", ".");
        }

    }
}
