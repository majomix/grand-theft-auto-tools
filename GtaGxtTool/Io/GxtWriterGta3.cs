using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtWriterGta3 : GxtWriterBase
    {
        private readonly bool _transformToSlovak;

        protected override int GetNumberOfBytesPerCharacters() => 2;
        
        public GxtWriterGta3(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        public override void Write(GxtFile gxtFile)
        {
            var section = gxtFile.TableBlocks[0];

            WriteTkeyBlock(section);
            WriteTdatBlock(section);
            
            // rewrite offsets
            BaseStream.Seek(0, SeekOrigin.Begin);
            WriteTkeyBlock(section);
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
                    { 'č', 'ç' }, { 'ď', 'Ç' }, { 'ĺ', 'ò' }, { 'ľ', 'ì' }, { 'ň', 'ñ' }, { 'ŕ', 'â' }, { 'š', 'ù' }, { 'ť', 'ü' }, { 'ý', 'û' }, { 'ž', 'ê' },
                    { 'Č', 'Ù' }, { 'Ď', 'Ê' }, { 'Ľ', 'È' }, { 'Ň', 'Ñ' }, { 'Ŕ', 'À' }, { 'Š', 'Ë' }, { 'Ť', 'Û' }, { 'Ý', 'Ö' }, { 'Ž', 'Ò' },
                    { '„', 'î' }, { '“', 'ï' },
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
