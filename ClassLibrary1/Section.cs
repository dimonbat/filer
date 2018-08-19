using System;
using System.Collections.Generic;


namespace Umpo.Common.IniFileLib
{
    public class Section
    {
        public string Name { get; set; }

        public List<SectionKey> Keys { get; set; }

    }
}
