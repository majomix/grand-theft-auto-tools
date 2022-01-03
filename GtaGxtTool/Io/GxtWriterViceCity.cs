using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtWriterViceCity : GxtWriterBase
    {
        private readonly bool _transformToSlovak;

        protected override int GetNumberOfBytesPerCharacters() => 2;
        
        public GxtWriterViceCity(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        public override void Write(GxtFile gxtFile)
        {
            WriteTablBlock(gxtFile);

            for (var i = 0; i < gxtFile.TableBlocks.Count; i++)
            {
                var section = gxtFile.TableBlocks[i];
                section.TkeyOffset = (int)BaseStream.Position;

                if (i != 0)
                {
                    WriteMetaString(section.Name, 8);
                }

                var blockStart = BaseStream.Position;
                WriteTkeyBlock(section);
                WriteTdatBlock(section);

                // rewrite offsets
                BaseStream.Seek(blockStart, SeekOrigin.Begin);
                WriteTkeyBlock(section);
                
                BaseStream.Seek(0, SeekOrigin.End);
            }

            // rewrite offsets
            BaseStream.Seek(0, SeekOrigin.Begin);
            WriteTablBlock(gxtFile);
        }

        protected override byte[] GetFinalTransformation(string input)
        {
            return Encoding.Unicode.GetBytes(input);
        }

        protected override string Transform(string input)
        {
            if (_transformToSlovak)
            {
                var builder = new StringBuilder();

                var table = new Dictionary<char, char>
                {
                    { 'č', 'ç' }, { 'ď', 'Â' }, { 'ĺ', 'Î' }, { 'ľ', 'ì' }, { 'ň', 'ñ' }, { 'ŕ', 'î' }, { 'š', 'ù' }, { 'ť', 'ï' }, { 'ý', 'û' }, { 'ž', 'ê' },
                    { 'Č', 'Ç' }, { 'Ď', 'Ê' }, { 'Í', 'Ì' }, { 'Ĺ', 'ß' }, { 'Ľ', 'È' }, { 'Ň', 'Ñ' }, { 'Ŕ', 'Ö' }, { 'Š', 'Ë' }, { 'Ť', 'Û' }, { 'Ý', 'À' }, { 'Ž', 'Ò' },
                    { '„', 'Æ' }, { '“', 'Ü' },
                };

                foreach (var letter in input)
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

                input = builder.ToString();
            }

            return base.Transform(input);
        }
    }
}
