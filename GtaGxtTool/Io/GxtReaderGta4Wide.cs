using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtReaderGta4Wide : GxtReaderBase
    {
        private readonly bool _transformToSlovak;

        public GxtReaderGta4Wide(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        protected override int GetNumberOfBytesPerCharacters() => 2;

        protected override Encoding GetEncoding() => Encoding.Unicode;

        public override GxtFile ReadGxtFile()
        {
            var gxtFile = new GxtFile();
            gxtFile.Version = GxtVersion.Gta4;

            var version = ReadInt16();
            if (version != 0x04)
            {
                throw new InvalidDataException("Version 4 expected.");
            }

            var bitsPerCharacter = ReadInt16();
            if (bitsPerCharacter != 0x10)
            {
                throw new InvalidDataException("16 bits per character expected.");
            }

            ReadTablBlock(gxtFile);

            for (var i = 0; i < gxtFile.TableBlocks.Count; i++)
            {
                var section = gxtFile.TableBlocks[i];
                BaseStream.Seek(section.TkeyOffset, SeekOrigin.Begin);

                if (i > 0)
                {
                    var sectionName = ReadMetaString(8);
                    if (sectionName != section.Name)
                    {
                        throw new InvalidDataException();
                    }
                }

                ReadTkeyBlockHashed(section, GxtVersion.Gta4);
                ReadTdatBlock(section);
            }

            return gxtFile;
        }

        protected override string Transform(string input)
        {
            return input;
        }
    }
}
