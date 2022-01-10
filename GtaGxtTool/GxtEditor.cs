using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GtaGxtTool.Model;

namespace GtaGxtTool
{
    public class GxtEditor
    {
        private readonly List<GxtFile> _files = new List<GxtFile>();

        public void LoadGxtFile(string gxtPath, bool transformToSlovak)
        {
            using (var file = File.Open(gxtPath, FileMode.Open))
            using (var reader = StrategySelector.SelectReader(file, transformToSlovak))
            {
                var gxtFile = reader.ReadGxtFile();
                gxtFile.Language = Path.GetFileNameWithoutExtension(gxtPath);

                _files.Add(gxtFile);
            }
        }

        public void LoadGxtFiles(string gxtPath)
        {
            foreach (var gxtFile in Directory.GetFiles(gxtPath, "*.gxt"))
            {
                LoadGxtFile(gxtFile, gxtFile.ToLower().EndsWith("slovak.gxt"));
            }
        }

        public void WriteTxtFile(string txtFile)
        {
            var gxtFile = _files[0];

            var output = new List<string>();
            output.Add($"{gxtFile.Version}");
            
            foreach (var section in gxtFile.TableBlocks)
            {
                if (gxtFile.Version != GxtVersion.Gta3)
                {
                    output.Add($"\n[{section.Name}]");
                }

                var entries = section.Entries;
                if (gxtFile.Version == GxtVersion.GtaSA || gxtFile.Version == GxtVersion.Gta4)
                {
                    entries = entries.OrderBy(e => e.Key.KeyName).ToList();
                }
                foreach (var entry in entries)
                {
                    output.Add($"{entry.Key.KeyName}\t{entry.Value}");
                }
            }

            File.WriteAllLines(txtFile, output, Encoding.UTF8);
        }

        public void WriteBatchTxtFile(string txtFile)
        {
            var dtos = new List<BatchSectionDto>();
            var output = new List<string>();

            var order = new[] { "american", "american", "polish", "german" };
            foreach (var language in order)
            {
                var gxtFile = _files.SingleOrDefault(f => f.Language.ToLower() == language);
                if (gxtFile == null)
                    continue;

                foreach (var section in gxtFile.TableBlocks)
                {
                    var dto = dtos.SingleOrDefault(d => d.SectionName == section.Name);
                    if (dto == null)
                    {
                        dto = new BatchSectionDto { SectionName = section.Name };
                        dtos.Add(dto);
                    }

                    foreach (var entry in section.Entries)
                    {
                        var dtoEntry = dto.Entries.SingleOrDefault(e => e.Key == entry.Key.KeyName);
                        if (dtoEntry == null)
                        {
                            dtoEntry = new BatchEntryDto { Key = entry.Key.KeyName };
                            dto.Entries.Add(dtoEntry);
                        }

                        dtoEntry.Values[language] = entry.Value;
                    }
                }
            }

            foreach (var dto in dtos)
            {
                if (_files[0].Version != GxtVersion.Gta3)
                {
                    output.Add($"[{dto.SectionName}]");
                }

                foreach (var entry in dto.Entries.OrderBy(e => e.Key))
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append($"{entry.Key}");

                    foreach (var language in order)
                    {
                        stringBuilder.Append("\t");

                        if (entry.Values.ContainsKey(language))
                        {
                            stringBuilder.Append($"{entry.Values[language]}");
                        }
                    }
                    output.Add(stringBuilder.ToString());
                }

                output.Add(string.Empty);
            }

            File.WriteAllLines(txtFile, output);
        }

        public void LoadTxtFile(string txtPath)
        {
            var gxtFile = new GxtFile();
            gxtFile.Language = Path.GetFileNameWithoutExtension(txtPath);

            var lines = File.ReadAllLines(txtPath);
            if (!Enum.TryParse(lines[0], out GxtVersion version))
            {
                throw new InvalidDataException("Version expected.");
            }

            gxtFile.Version = version;

            GxtTable currentSection = null;

            if (version == GxtVersion.Gta3)
            {
                currentSection = new GxtTable { Name = "GTA3" };
                gxtFile.TableBlocks.Add(currentSection);
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var sectionName = line.Split('[', ']')[1];
                    currentSection = new GxtTable { Name = sectionName };
                    gxtFile.TableBlocks.Add(currentSection);
                }
                else
                {
                    var tokens = line.Split(new[] { '\t' }, 2);
                    if (tokens.Length == 2)
                    {
                        var id = tokens[0];
                        var value = tokens[1];

                        var entry = new GxtEntry();
                        entry.Key = new GxtKeyEntry();
                        entry.Key.KeyName = id;
                        entry.Value = value;

                        currentSection?.Entries.Add(entry);
                    }
                }
            }
            
            _files.Add(gxtFile);
        }

        public void WriteGxtFile(string gxtPath, bool transformToSlovak)
        {
            var gxtFile = _files[0];

            using (var file = File.Create(gxtPath))
            using (var writer = StrategySelector.SelectWriter(gxtFile.Version, file, transformToSlovak))
            {
                writer.Write(gxtFile);
            }
        }
    }

    public class BatchSectionDto
    {
        public string SectionName;
        public List<BatchEntryDto> Entries = new List<BatchEntryDto>();
    }

    public class BatchEntryDto
    {
        public string Key;
        public Dictionary<string, string> Values = new Dictionary<string, string>();
    }
}
