using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UnityExtractor
{
    class Utilities
    {
        public static XmlDocument GetXmlFile(string file)
        {
            var fileContents = string.Empty;

            try
            {
                fileContents = File.ReadAllText(file);

                if (fileContents.Contains("\n<?xml"))
                {
                    fileContents = fileContents.Replace("\n<?xml", "<?xml");
                    File.WriteAllText(file, fileContents);
                }

                if (fileContents.Contains("<graphics>"))
                {
                    fileContents = fileContents.Replace("<graphics>", "");
                    fileContents = fileContents.Replace("</graphics>", "");
                    File.WriteAllText(file, fileContents);
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);

                return xmlDoc;
            }
            catch
            {
                return null;
            }
        }
    }
}
