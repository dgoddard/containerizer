﻿#region

using System.IO;
using Containerizer.Services.Interfaces;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;

#endregion

namespace Containerizer.Services.Implementations
{
    public class TarStreamService : ITarStreamService
    {
        public Stream WriteTarToStream(string filePath)
        {
            string tarPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".gz");

            // if (!Directory.Exists(filePath) && !File.Exists(filePath)) { throw new FileNotFoundException();  }
            if (filePath.EndsWith("output-cache")) {
                // throw new DirectoryNotFoundException();
                tarPath = @"C:\empty.tgz";
            }
            else
            {
                CreateTarFromDirectory(filePath, tarPath);
            }

            Stream stream = File.OpenRead(tarPath);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            var memStream = new MemoryStream(buffer);
            stream.Close();
            File.Delete(tarPath);
            return memStream;
        }


        public void WriteTarStreamToPath(Stream stream, string filePath)
        {
            IReader reader = ReaderFactory.Open(new BufferedStream(stream));
            reader.WriteAllToDirectory(filePath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
        }

        public void CreateTarFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            using (Stream stream = File.OpenWrite(destinationArchiveFileName))
            {
                using (
                    IWriter writer = WriterFactory.Open(stream, ArchiveType.Tar,
                        new CompressionInfo {Type = CompressionType.None}))
                {
                    if (File.Exists(sourceDirectoryName))
                    {
                        var info = new FileInfo(sourceDirectoryName);
                        writer.Write(info.Name, info);
                    }
                    else
                    {
                        try
                        {
                            writer.WriteAll(sourceDirectoryName, "*", SearchOption.AllDirectories);
                        }
                        catch (System.ArgumentException e)
                        {
                            // DUNNO !!!
                        }
                    }
                }
            }
        }

        public void CreateTarGzFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            string tarPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".tar");
            try
            {
                using (Stream stream = File.OpenWrite(tarPath))
                {
                    using (
                        IWriter writer = WriterFactory.Open(stream, ArchiveType.Tar,
                            new CompressionInfo {Type = CompressionType.None}))
                    {
                        if (File.Exists(sourceDirectoryName))
                        {
                            var info = new FileInfo(sourceDirectoryName);
                            writer.Write(info.Name, info);
                        }
                        else
                        {
                            writer.WriteAll(sourceDirectoryName, "*", SearchOption.AllDirectories);
                        }
                    }
                }
                using (Stream stream = File.OpenWrite(destinationArchiveFileName))
                {
                    using (
                        IWriter writer = WriterFactory.Open(stream, ArchiveType.GZip,
                            new CompressionInfo {Type = CompressionType.GZip}))
                    {
                        writer.Write("Tar.tar", tarPath);
                    }
                }
            }
            finally
            {
                if (File.Exists(tarPath)) File.Delete(tarPath);
            }
        }
    }
}