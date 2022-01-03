using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtReaderViceCity : GxtReaderBase
    {
        private readonly bool _transformToSlovak;

        public GxtReaderViceCity(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        protected override int GetNumberOfBytesPerCharacters() => 2;

        protected override Encoding GetEncoding() => Encoding.Unicode;

        public override GxtFile ReadGxtFile()
        {
            var gxtFile = new GxtFile();
            gxtFile.Version = GxtVersion.GtaVC;

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

                ReadTkeyBlock(section);
                ReadTdatBlock(section);
            }

            return gxtFile;
        }

        protected override string Transform(string input)
        {
            var initial = base.Transform(input);

            if (!_transformToSlovak)
            {
                return initial;
            }

            var builder = new StringBuilder();

            var table = new Dictionary<char, char>
            {
                { 'ç', 'č' }, { 'è', 'ď' }, { 'Â', 'ď' }, { 'Î', 'ĺ' }, { 'ì', 'ľ' }, { 'ñ', 'ň' }, { 'î', 'ŕ' }, { 'ù', 'š' }, { 'ï', 'ť' }, { 'û', 'ý' }, { 'ê', 'ž' },
                { 'Ç', 'Č' }, { 'Ê', 'Ď' }, { 'Ù', 'Ď' }, { 'ß', 'Ĺ' }, { 'È', 'Ľ' }, { 'Ì', 'Í' }, { 'Ñ', 'Ň' }, { 'Ö', 'Ŕ' }, { 'Ë', 'Š' }, { 'Û', 'Ť' }, { 'À', 'Ý' }, { 'Ò', 'Ž' },
                { 'Æ', '„' }, { 'Ü', '“' },
            };

            foreach (var letter in initial)
            {
                if (table.TryGetValue(letter, out var value))
                {
                    builder.Append(value);
                }
                else
                {
                    builder.Append(letter);
                }
            }

            return builder.ToString();
        }
    }
}
