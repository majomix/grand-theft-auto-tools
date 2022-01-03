using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtReaderSanAndreas : GxtReaderBase
    {
        private readonly bool _transformToSlovak;

        public GxtReaderSanAndreas(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        protected override int GetNumberOfBytesPerCharacters() => 1;

        protected override Encoding GetEncoding() => Encoding.GetEncoding(1252);

        public override GxtFile ReadGxtFile()
        {
            var gxtFile = new GxtFile();
            gxtFile.Version = GxtVersion.GtaSA;

            var version = ReadInt16();
            if (version != 0x04)
            {
                throw new InvalidDataException("Version 4 expected.");
            }

            var bitsPerCharacter = ReadInt16();
            if (bitsPerCharacter != 0x08)
            {
                throw new InvalidDataException("8 bits per character expected.");
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
                        // some editors input different data
                        //throw new InvalidDataException();
                    }
                }

                ReadTkeyBlockHashed(section);
                ReadTdatBlock(section);
            }

            return gxtFile;
        }

        protected override string Transform(string input)
        {
            var builder = new StringBuilder();

            var letters = "ÀÁÂÄÆÇÈÉÊËÌÍÎÏÒÓÔÖÙÚÛÜßàáâäæçèéêëìíîïòóôöùúûüÑñ¿¡123456789:abcdefghijklmnopqrstuvwxyzàáâäæçèéêëìíîïòóôöùúûüßñ¿'.                         ";

            foreach (var letter in GetEncoding().GetBytes(input))
            {
                if (letter >= 0x80)
                {
                    var index = letter - 0x80;
                    builder.Append(letters[index]);
                }
                else
                {
                    builder.Append(GetEncoding().GetString(new[] { letter }));
                }
            }

            var initialTransform = builder.ToString();

            if (!_transformToSlovak)
            {
                return initialTransform;
            }

            var slovakBuilder = new StringBuilder();

            var table = new Dictionary<char, char>
            {
                { 'ë', 'č' }, { 'à', 'ď' }, { 'û', 'ĺ' }, { 'ü', 'ľ' }, { 'ò', 'ň' }, { 'ç', 'ŕ' }, { 'è', 'š' }, { 'ê', 'ť' }, { 'â', 'ý' }, { 'ù', 'ž' },
                { 'Ë', 'Č' }, { 'À', 'Ď' }, { 'Û', 'Ĺ' }, { 'Ü', 'Ľ' }, { 'Ò', 'Ň' }, { 'Ç', 'Ŕ' }, { 'È', 'Š' }, { 'Ê', 'Ť' }, { 'Â', 'Ý' }, { 'Ù', 'Ž' },
                { 'î', '„' }, { 'ï', '“' },
            };

            foreach (var letter in initialTransform)
            {
                if (table.TryGetValue(letter, out var value))
                {
                    slovakBuilder.Append(value);
                }
                else
                {
                    slovakBuilder.Append(letter);
                }
            }

            return slovakBuilder.ToString();
        }
    }
}
