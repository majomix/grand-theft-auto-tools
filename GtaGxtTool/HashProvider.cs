using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool
{
    public class HashProvider
    {
        private readonly Dictionary<uint, string> _sanAndreas = new Dictionary<uint, string>();
        private readonly Dictionary<string, uint> _sanAndreasReversed = new Dictionary<string, uint>();
        private readonly Dictionary<uint, string> _iv = new Dictionary<uint, string>();
        private readonly Dictionary<string, uint> _ivReversed = new Dictionary<string, uint>();

        public HashProvider()
        {
            InitializeHashList(@"Hash\gta_sa.tdc", _sanAndreas, _sanAndreasReversed);
            InitializeHashList(@"Hash\gta_iv.tdc", _iv, _ivReversed);
        }

        public string GetEntryName(uint hash, GxtVersion version)
        {
            switch (version)
            {
                case GxtVersion.GtaSA:
                    return GetSanAndreasEntryName(hash);
                case GxtVersion.Gta4:
                    return GetIvEntryName(hash);
                default:
                    throw new InvalidDataException();
            }
        }

        private string GetSanAndreasEntryName(uint hash)
        {
            _sanAndreas.TryGetValue(hash, out var result);
            return result ?? $"0x{hash:X8}";
        }

        public uint GetSanAndreasHash(string name)
        {
            if (name.StartsWith("0x"))
            {
                return Convert.ToUInt32(name, 16);
            }
            
            return _sanAndreasReversed[name];
        }

        private string GetIvEntryName(uint hash)
        {
            _iv.TryGetValue(hash, out var result);
            return result ?? $"0x{hash:X8}";
        }

        private void InitializeHashList(string input, Dictionary<uint, string> dictionary, Dictionary<string, uint> reverseDictionary)
        {
            using (var file = File.Open(input, FileMode.Open))
            using (var reader = new BinaryReader(file))
            {
                var numberOfSections = reader.ReadInt32();
                for (var i = 0; i < numberOfSections; i++)
                {
                    var sectionNameLength = reader.ReadByte();
                    var sectionNameBytes = reader.ReadBytes(sectionNameLength);
                    var sectionName = Encoding.ASCII.GetString(sectionNameBytes);

                    var entriesCount = reader.ReadInt32();

                    for (var j = 0; j < entriesCount; j++)
                    {
                        var hash = reader.ReadUInt32();
                        var entryNameLength = reader.ReadByte();
                        var entryNameBytes = reader.ReadBytes(entryNameLength);
                        var entryName = Encoding.ASCII.GetString(entryNameBytes);

                        dictionary[hash] = entryName;
                        reverseDictionary[entryName] = hash;
                    }
                }
            }
        }
    }
}
