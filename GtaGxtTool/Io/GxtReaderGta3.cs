using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtReaderGta3 : GxtReaderBase
    {
        private readonly bool _transformToSlovak;

        public GxtReaderGta3(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        protected override int GetNumberOfBytesPerCharacters() => 2;
        protected override Encoding GetEncoding() => Encoding.Unicode;

        public override GxtFile ReadGxtFile()
        {
            var gxtFile = new GxtFile();
            gxtFile.Version = GxtVersion.Gta3;

            var defaultSection = new GxtTable { Name = "GTA3" };

            ReadTkeyBlock(defaultSection);
            ReadTdatBlock(defaultSection);

            gxtFile.TableBlocks.Add(defaultSection);

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
                { 'ç', 'č' }, { 'Ç', 'ď' }, { 'ò', 'ĺ' }, { 'ì', 'ľ' }, { 'ñ', 'ň' }, { 'â', 'ŕ' }, { 'ù', 'š' }, { 'ü', 'ť' }, { 'û', 'ý' }, { 'ê', 'ž' },
                { 'Ù', 'Č' }, { 'Ê', 'Ď' }, { 'È', 'Ľ' }, { 'Ñ', 'Ň' }, { 'À', 'Ŕ' }, { 'Ë', 'Š' }, { 'Û', 'Ť' }, { 'Ö', 'Ý' }, { 'Ò', 'Ž' },
                { 'î', '„' }, { 'ï', '“' }, 
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
