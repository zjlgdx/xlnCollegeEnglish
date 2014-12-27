using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipe5_2
{
    class Unit
    {
        public string Word { get; set; }
        public string Voice { get; set; }
        public ICollection<Definition> Definitions { get; set; }
    }
}
