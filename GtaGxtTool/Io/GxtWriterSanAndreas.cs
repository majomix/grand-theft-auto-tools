using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public class GxtWriterSanAndreas : GxtWriterBase
    {
        private readonly bool _transformToSlovak;

        protected override int GetNumberOfBytesPerCharacters() => 1;

        public GxtWriterSanAndreas(Stream input, bool transformToSlovak) : base(input)
        {
            _transformToSlovak = transformToSlovak;
        }

        public override void Write(GxtFile gxtFile)
        {
            Write((short)0x04);
            Write((short)0x08);

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
                WriteTkeyHashedBlock(section, HashProvider.GetSanAndreasHash);
                WriteTdatBlock(section);

                // rewrite offsets
                BaseStream.Seek(blockStart, SeekOrigin.Begin);
                WriteTkeyHashedBlock(section, HashProvider.GetSanAndreasHash);

                BaseStream.Seek(0, SeekOrigin.End);
            }

            // rewrite offsets
            BaseStream.Seek(4, SeekOrigin.Begin);
            WriteTablBlock(gxtFile);
        }

        protected override byte[] GetFinalTransformation(string input)
        {
            var bytes = new List<byte>();

            var letters = "ÀÁÂÄÆÇÈÉÊËÌÍÎÏÒÓÔÖÙÚÛÜßàáâäæçèéêëìíîïòóôöùúûüÑñ¿¡";

            foreach (var letter in input)
            {
                var index = letters.IndexOf(letter);
                if (index == -1)
                {
                    bytes.Add((byte)letter);
                }
                else
                {
                    bytes.Add((byte)(index + 0x80));
                }
            }

            return bytes.ToArray();
        }

        protected override string Transform(string input)
        {
            if (_transformToSlovak)
            {
                var slovakBuilder = new StringBuilder();

                var table = new Dictionary<char, char>
                {
                    { 'č', 'ç' }, { 'ď', 'ê' }, { 'ĺ', 'î' }, { 'ľ', 'è' }, { 'ň', 'ò' }, { 'ŕ', 'à' }, { 'š', 'û' }, { 'ť', 'ë' }, { 'ý', 'ì' }, { 'ž', 'â' },
                    { 'Č', 'Ç' }, { 'Ď', 'Ê' }, { 'Ĺ', 'Î' }, { 'Ľ', 'È' }, { 'Ň', 'Ò' }, { 'Ŕ', 'À' }, { 'Š', 'Û' }, { 'Ť', 'Ë' }, { 'Ý', 'Ì' }, { 'Ž', 'Â' },
                    { '„', 'Æ' }, { '“', 'Ù' },
                };

                foreach (var letter in input)
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

                input = slovakBuilder.ToString();
            }

            return input;
        }
    }
}
