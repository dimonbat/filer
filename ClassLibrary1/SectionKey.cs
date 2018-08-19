using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umpo.Common.IniFileLib
{
    public class SectionKey
    {
        public List<SectionKey> Keys { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
