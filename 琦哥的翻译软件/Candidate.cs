using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 琦哥的翻译软件
{
    class Candidate
    {
        public int Number { get; set; }
        public string Chinese { get; set; }
        public string English { get; set; }
        public double Similarity { get; set; }
    }
}