using System.Collections.Generic;

namespace GtaGxtTool.Model
{
    public class GxtFile
    {
        public GxtVersion Version;
        public string Language;
        public List<GxtTable> TableBlocks = new List<GxtTable>();
    }
}
