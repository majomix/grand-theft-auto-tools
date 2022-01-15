using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GtaGxtTool.Model;

namespace GtaGxtTool
{
    public class MergeGTASKFiles

    {
        private readonly List<TxtFile> _files = new List<TxtFile>();
        //private readonly TxtFile txtFile;

        //private readonly List<TxtFile> txtFile = new List<TxtFile>();
        //private readonly object txtFile;

        public void LoadTxtFile(string fileName)
        //public void ReadTxtFile(string fileName)
        {
            //using (var file = File.Open(fileName, FileMode.Open))
            {
                var lines = File.ReadAllLines(fileName);
                //if (!Enum.TryParse(lines[0], out GxtVersion version))
                //{
                //    throw new InvalidDataException("Version expected.");
                //}


                for (var i = 1; i < lines.Length; i++)
                {
                    // var line = lines[i].Trim();
                    var entry = new TxtEntry();
                    var txtTable = new TxtTable();
                    var txtFile = new TxtFile();

                    txtTable.Entries.Add(entry);

                    //if (line.StartsWith("[") && line.EndsWith("]"))
                    //{
                    //    var sectionName = line.Split('[', ']')[1];
                    //    currentSection = new GxtTable { Name = sectionName };
                    //    gxtFile.TableBlocks.Add(currentSection);
                    //}
                    //else
                    //{
                    //    var tokens = line.Split(new[] { '\t' }, 2);
                    //    if (tokens.Length == 2)
                    //    {
                    //        var id = tokens[0];
                    //        var value = tokens[1];

                    //        var entry = new GxtEntry();
                    //        entry.Key = new GxtKeyEntry();
                    //        entry.Key.KeyName = id;
                    //        entry.Value = value;

                    //        currentSection?.Entries.Add(entry);
                    //    }
                    //}
                }

                //_files.Add(txtFile);

            }
        }

        public void WriteTxtFile(string fileName)
        {
            //var txtFile = _files[0];

            var output = new List<string>();
            //output.Add($"{gxtFile.Version}");

            foreach (var section in txtFile.TableBlocks)
            {
            //    if (gxtFile.Version != GxtVersion.Gta3)
            //    {
            //        output.Add($"\n[{section.Name}]");
            //    }

                var entries = section.Entries;
            //    if (gxtFile.Version == GxtVersion.GtaSA || gxtFile.Version == GxtVersion.Gta4)
            //    {
            //        entries = entries.OrderBy(e => e.Key.KeyName).ToList();
            //    }
                foreach (var entry in entries)
                {
                    //output.Add($"{entry.Key.KeyName}\t{entry.Value}");
                    output.Add("{entry.Value}");
                }
            }

            File.WriteAllLines(fileName, output, Encoding.UTF8);


            //string[] file1 = File.ReadAllLines("*DIRECTORY*\FIRSTFILE.txt");
            //string[] file2 = File.ReadAllLines("*DIRECTORY*\SECONDFILE.txt");

            //using (StreamWriter writer = File.CreateText(@"*DIRECTORY*\FINALOUTPUT.txt"))
            //{
            //    int lineNum = 0;
            //    while (lineNum < file1.Length || lineNum < file2.Length)
            //    {
            //        if (lineNum < file1.Length)
            //            writer.WriteLine(file1[lineNum]);
            //        if (lineNum < file2.Length)
            //            writer.WriteLine(file2[lineNum]);
            //        lineNum++;
            //    }
            //}

        }
    }
}
