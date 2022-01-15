using System.IO;
using NDesk.Options;

namespace GtaGxtTool
{
    public class Program
    {
        enum Action
        {
            Export,
            BatchExport,
            Import
        }

        static void Main(string[] args)
        {
            var action = Action.Export;
            var useSlovakTransform = false;
            var txtPath = Directory.GetCurrentDirectory();
            string gxtPath = null;

            var options = new OptionSet()
                .Add("batch", value => action = Action.BatchExport)
                .Add("import", value => action = Action.Import)
                .Add("gxt=", value => gxtPath = value)
                .Add("txt=", value => txtPath = value)
                .Add("slovak", value => useSlovakTransform = true);

            options.Parse(args);

            var processFiles = new MergeGTASKFiles();
            processFiles.LoadTxtFile("MAIN.txt");
            processFiles.LoadTxtFile("MI1AUD.txt");
            processFiles.WriteTxtFile("test.txt");

            return;

            var editor = new GxtEditor();

            //gxtPath = @"D:\Preklady\Grand Theft Auto - San Andreas\slovak.gxt";
            //txtPath = @"D:\Preklady\Grand Theft Auto - San Andreas\backup\text\naetoo_slovak.txt";


            switch (action)
            {
                case Action.Export:
                    editor.LoadGxtFile(gxtPath, useSlovakTransform);
                    editor.WriteTxtFile(txtPath);
                    break;
                case Action.BatchExport:
                    editor.LoadGxtFiles(gxtPath);
                    editor.WriteBatchTxtFile(txtPath);
                    break;
                case Action.Import:
                    editor.LoadTxtFile(txtPath);
                    editor.WriteGxtFile(gxtPath, useSlovakTransform);
                    break;
            }
        }
    }
}
