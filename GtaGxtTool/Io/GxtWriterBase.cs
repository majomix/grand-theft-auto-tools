using System;
using System.IO;
using System.Linq;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public abstract class GxtWriterBase : BinaryWriter
    {
        protected readonly HashProvider HashProvider;

        public abstract void Write(GxtFile gxtFile);

        protected GxtWriterBase(Stream input) : base(input)
        {
            HashProvider = new HashProvider();
        }

        protected abstract int GetNumberOfBytesPerCharacters();

        protected abstract byte[] GetFinalTransformation(string input);

        protected void WriteTablBlock(GxtFile gxtFile)
        {
            WriteMetaString("TABL", 4);

            var contentSize = gxtFile.TableBlocks.Count * 12;

            Write(contentSize);

            foreach (var block in gxtFile.TableBlocks)
            {
                WriteMetaString(block.Name, 8);
                Write(block.TkeyOffset);
            }
        }

        protected void WriteTkeyBlock(GxtTable gxtTable)
        {
            WriteMetaString("TKEY", 4);

            var contentSize = gxtTable.Entries.Count * 12;

            Write(contentSize);

            foreach (var entry in gxtTable.Entries)
            {
                Write(entry.Key.Offset);
                WriteMetaString(entry.Key.KeyName, 8);
            }
        }

        protected void WriteTkeyHashedBlock(GxtTable gxtTable, Func<string, uint> hashFunc)
        {
            WriteMetaString("TKEY", 4);

            var contentSize = gxtTable.Entries.Count * 8;

            Write(contentSize);

            foreach (var entry in gxtTable.Entries)
            {
                entry.Key.KeyHash = hashFunc(entry.Key.KeyName);
            }

            foreach (var entry in gxtTable.Entries.OrderBy(e => e.Key.KeyHash))
            {
                Write(entry.Key.Offset);
                Write(entry.Key.KeyHash);
            }
        }

        protected void WriteTdatBlock(GxtTable gxtTable)
        {
            WriteMetaString("TDAT", 4);

            var offsetToWriteLength = BaseStream.Position;
            Write(0);
            var sectionStartOffset = BaseStream.Position;

            foreach (var entry in gxtTable.Entries)
            {
                entry.Key.Offset = (int)(BaseStream.Position - sectionStartOffset);
                var bytes = GetFinalTransformation(Transform(entry.Value) + '\0');
                Write(bytes);
            }

            var sectionEndOffset = BaseStream.Position;
            var sectionLength = sectionEndOffset - sectionStartOffset;

            var padding = sectionLength % 4;
            if (padding != 0)
            {
                for (var y = 0; y < 4 - padding; y++)
                {
                    Write((byte)0);
                }
            }

            Seek((int)offsetToWriteLength, SeekOrigin.Begin);
            Write((int)sectionLength);
        }

        protected virtual string Transform(string input)
        {
            var builder = new StringBuilder();

            var letters = "ÀÁÂÄÆÇÈÉÊËÌÍÎÏÒÓÔÖÙÚÛÜßàáâäæçèéêëìíîïòóôöùúûüÑñ¿¡";

            if (GetNumberOfBytesPerCharacters() == 2)
            {
                foreach (var letter in input)
                {
                    var index = letters.IndexOf(letter);
                    if (index == -1)
                    {
                        builder.Append(letter);
                    }
                    else
                    {
                        builder.Append((char)(index + 0x80));
                    }
                }
            }

            return builder.ToString();
        }

        protected void WriteMetaString(string text, int length)
        {
            Write(Encoding.ASCII.GetBytes(text));

            for (var i = 0; i < length - text.Length; i++)
            {
                Write('\0');
            }
        }
    }
}
