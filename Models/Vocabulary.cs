using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Vocabulary
    {
        public string Word { get; set; }
        public string Voice { get; set; }
        public ICollection<Definition> Definitions { get; set; }
    }
}
