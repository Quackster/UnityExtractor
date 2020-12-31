using AssetStudio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace UnityExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var file in Directory.GetFiles("C:/unity_furni/", "*"))
            {
                AssetBundle assetBundle = null;

                var assetsManager = new AssetsManager();
                assetsManager.LoadFiles(file);

                // Find asset bundle first
                foreach (var asset in assetsManager.assetsFileList)
                {
                    foreach (var obj in asset.Objects)
                    {
                        if (obj is AssetBundle rm)
                            assetBundle = rm;
                    }
                }

                // Extract assets
                foreach (var asset in assetsManager.assetsFileList)
                {
                    foreach (var obj in asset.Objects)
                    {
                        var name = Path.GetRandomFileName();

                        if (obj is NamedObject namedObject)
                            name = namedObject.m_Name;

                        var assetItem = new AssetItem(obj);
                        assetItem.AssetName = name;

                        var container = assetBundle.m_Container.Where(x => Path.GetFileNameWithoutExtension(x.Key) == name).Select(x => Tuple.Create(x.Key, x.Value)).FirstOrDefault();

                        if (container != null)
                            assetItem.Container = container.Item1;

                        Exporter.ExportConvertFile(assetItem, "export/" + Path.GetFileNameWithoutExtension(file));
                    }
                }
            }

            Console.WriteLine("Done!");
            Console.Read();
        }
    }

    public class Exporter
    {
        public static bool ExportConvertFile(AssetItem item, string exportPath)
        {
            switch (item.Type)
            {
                case ClassIDType.Texture2D:
                    return ExportTexture2D(item, exportPath);
                case ClassIDType.TextAsset:
                    return ExportTextAsset(item, exportPath);
                default:
                    return false;
            }
        }

        public static bool ExportTextAsset(AssetItem item, string exportPath)
        {
            var m_TextAsset = (TextAsset)(item.Asset);
            var extension = ".txt";

            // Restore extension
            if (!string.IsNullOrEmpty(item.Container))
            {
                extension = Path.GetExtension(item.Container);
            }

            if (!TryExportFile(exportPath, item, extension, out var exportFullPath))
                return false;
            File.WriteAllBytes(exportFullPath, m_TextAsset.m_Script);
            return true;
        }

        public static bool ExportTexture2D(AssetItem item, string exportPath)
        {
            var m_Texture2D = (Texture2D)item.Asset;

            var bitmap = m_Texture2D.ConvertToBitmap(true);
            if (bitmap == null)
                return false;

            var ext = ".png";

            // Restore extension
            if (!string.IsNullOrEmpty(item.Container))
            {
                ext = Path.GetExtension(item.Container);
            }

            ImageFormat format = ImageFormat.Png;

            if (!TryExportFile(exportPath, item, ext, out var exportFullPath))
                return false;

            bitmap.Save(exportFullPath, format);
            bitmap.Dispose();
            return true;
        }

        private static bool TryExportFile(string dir, AssetItem item, string extension, out string fullPath)
        {
            var fileName = FixFileName(item.AssetName);
            fullPath = Path.Combine(dir, fileName + extension);
            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            fullPath = Path.Combine(dir, fileName + item.UniqueID + extension);
            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            return false;
        }

        public static string FixFileName(string str)
        {
            if (str.Length >= 260) return Path.GetRandomFileName();
            return Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));
        }
    }

    public static class Texture2DExtensions
    {
        public static Bitmap ConvertToBitmap(this Texture2D m_Texture2D, bool flip)
        {
            var converter = new Texture2DConverter(m_Texture2D);
            return converter.ConvertToBitmap(flip);
        }
    }

    public class AssetItem
    {
        public AssetStudio.Object Asset;
        public SerializedFile SourceFile;
        public string Container = string.Empty;
        public string TypeString;
        public long m_PathID;
        public long FullSize;
        public ClassIDType Type;
        public string InfoText;
        public string UniqueID;
        public string FileName;
        public string AssetName;

        public AssetItem(AssetStudio.Object asset)
        {
            Asset = asset;
            SourceFile = asset.assetsFile;
            Type = asset.type;
            TypeString = Type.ToString();
            m_PathID = asset.m_PathID;
            FullSize = asset.byteSize;
        }
    }
}
