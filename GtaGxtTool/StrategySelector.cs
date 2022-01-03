using System;
using System.IO;
using System.Text;
using GtaGxtTool.Io;
using GtaGxtTool.Model;

namespace GtaGxtTool
{
    public class StrategySelector
    {
        public static GxtReaderBase SelectReader(FileStream stream, bool transformToSlovak)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);
            var value = Encoding.ASCII.GetString(buffer);

            if (value == "TKEY")
            {
                return new GxtReaderGta3(stream, transformToSlovak);
            }
            if (value == "TABL")
            {
                return new GxtReaderViceCity(stream, transformToSlovak);
            }

            if (buffer[0] == 0x04)
            {
                if (buffer[2] == 0x08)
                {
                    var entryNameBuffer = new byte[7];
                    stream.Seek(24, SeekOrigin.Begin);
                    stream.Read(buffer, 0, 4);
                    stream.Seek(0, SeekOrigin.Begin);

                    var entryName = Encoding.ASCII.GetString(entryNameBuffer);
                    switch (entryName)
                    {
                        case "AUTOERT":
                            return new GxtReaderGta4(stream, transformToSlovak);
                        case "AMBULAE":
                            return new GxtReaderSanAndreas(stream, transformToSlovak);
                        default:
                            return new GxtReaderSanAndreas(stream, transformToSlovak);
                    }
                }

                if (buffer[2] == 0x10)
                {
                    return new GxtReaderGta4Wide(stream, transformToSlovak);
                }
            }

            throw new NotImplementedException();
        }

        public static GxtWriterBase SelectWriter(GxtVersion version, FileStream stream, bool transformToSlovak)
        {
            switch (version)
            {
                case GxtVersion.Gta3:
                    return new GxtWriterGta3(stream, transformToSlovak);
                case GxtVersion.GtaVC:
                    return new GxtWriterViceCity(stream, transformToSlovak);
                case GxtVersion.GtaSA:
                    return new GxtWriterSanAndreas(stream, transformToSlovak);
                default:
                    throw new NotImplementedException();

            }
        }
    }
}
