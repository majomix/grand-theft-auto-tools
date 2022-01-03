using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool.Io
{
    public abstract class GxtReaderBase : BinaryReader
    {
        private readonly HashProvider _hashProvider;

        protected GxtReaderBase(Stream input) : base(input)
        {
            _hashProvider = new HashProvider();
        }

        protected abstract int GetNumberOfBytesPerCharacters();
        protected abstract Encoding GetEncoding();

        public abstract GxtFile ReadGxtFile();

        protected virtual string Transform(string input)
        {
            var builder = new StringBuilder();

            var letters = "ÀÁÂÄÆÇÈÉÊËÌÍÎÏÒÓÔÖÙÚÛÜßàáâäæçèéêëìíîïòóôöùúûüÑñ¿¡.";

            if (GetNumberOfBytesPerCharacters() == 2)
            {
                foreach (var letter in input)
                {
                    if (letter >= 0x80)
                    {
                        var index = letter - 0x80;
                        builder.Append(letters[index]);
                    }
                    else
                    {
                        builder.Append(letter);
                    }
                }
            }

            return builder.ToString();
        }

        protected void ReadTablBlock(GxtFile gxtFile)
        {
            var sectionId = ReadMetaString(4);
            if (sectionId != "TABL")
            {
                throw new InvalidDataException("TABL block expected");
            }

            var contentSize = ReadInt32();
            var contentStart = BaseStream.Position;

            while (BaseStream.Position != contentStart + contentSize)
            {
                var section = new GxtTable();

                section.Name = ReadMetaString(8);
                section.TkeyOffset = ReadInt32();

                gxtFile.TableBlocks.Add(section);
            }
        }

        protected void ReadTdatBlock(GxtTable gxtTable)
        {
            var sectionId = ReadMetaString(4);
            if (sectionId != "TDAT")
            {
                throw new InvalidDataException("TDAT block expected");
            }

            var sectionLength = ReadInt32();
            var data = ReadBytes(sectionLength);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                for (var i = 0; i < gxtTable.Entries.Count; i++)
                {
                    var startPosition = gxtTable.Entries[i].Key.Offset;
                    stream.Seek(startPosition, SeekOrigin.Begin);

                    var foundEnd = false;
                    var endPosition = -1L;
                    while (!foundEnd)
                    {
                        var character = reader.ReadBytes(GetNumberOfBytesPerCharacters());
                        if (character[0] == '\0')
                        {
                            foundEnd = true;
                            endPosition = stream.Position;
                        }
                    }

                    var count = endPosition - startPosition;

                    stream.Seek(startPosition, SeekOrigin.Begin);
                    var stringBytes = reader.ReadBytes((int)count);
                    var encoding = GetEncoding();
                    var stringValue = encoding.GetString(stringBytes).Trim('\0');
                    
                    gxtTable.Entries[i].Value = Transform(stringValue);
                }
            }
        }

        protected void ReadTkeyBlock(GxtTable gxtTable)
        {
            var sectionId = ReadMetaString(4);
            if (sectionId != "TKEY")
            {
                throw new InvalidDataException("TKEY block expected");
            }

            var contentSize = ReadInt32();
            var contentStart = BaseStream.Position;

            while (BaseStream.Position != contentStart + contentSize)
            {
                var entry = new GxtEntry();
                var keyEntry = new GxtKeyEntry();

                keyEntry.Offset = ReadInt32();
                keyEntry.KeyName = ReadMetaString(8);

                entry.Key = keyEntry;
                gxtTable.Entries.Add(entry);
            }
        }

        protected void ReadTkeyBlockHashed(GxtTable gxtTable)
        {
            var sectionId = ReadMetaString(4);
            if (sectionId != "TKEY")
            {
                throw new InvalidDataException("TKEY block expected");
            }

            var contentSize = ReadInt32();
            var contentStart = BaseStream.Position;

            while (BaseStream.Position != contentStart + contentSize)
            {
                var entry = new GxtEntry();
                var keyEntry = new GxtKeyEntry();

                keyEntry.Offset = ReadInt32();
                keyEntry.KeyHash = ReadUInt32();
                keyEntry.KeyName = _hashProvider.GetSanAndreasEntryName(keyEntry.KeyHash);

                entry.Key = keyEntry;
                gxtTable.Entries.Add(entry);
            }
        }

        protected string ReadMetaString(int length)
        {
            var bytes = ReadBytes(length);
            return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
        }
    }
}
