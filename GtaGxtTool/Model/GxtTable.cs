using System.Collections.Generic;

namespace GtaGxtTool.Model
{
    public class GxtTable
    {
        public string Name;
        public int TkeyOffset;
        public List<GxtEntry> Entries = new List<GxtEntry>();
    }
}
