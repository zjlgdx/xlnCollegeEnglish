﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Unit
    {
        public string UnitTitle { get; set; }
        public ICollection<Vocabulary> Vocabularies { get; set; }
    }
}