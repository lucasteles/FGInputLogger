﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FGInputLogger
{
    public class ControlMap
    {
        

        public List<int> Left { get; set; }
        public List<int> Right { get; set; }
        public List<int> Up { get; set; }
        public List<int> Down { get; set; }
               
        public List<int> LP { get; set; }
        public List<int> MP { get; set; }
        public List<int> HP { get; set; }
        public List<int> PPP { get; set; }
               
        public List<int> LK { get; set; }
        public List<int> MK { get; set; }
        public List<int> HK { get; set; }
        public List<int> KKK { get; set; }

        public List<int> Clear { get; set; }

        public ControlMap()
        {
            Left = new List<int>();
            Right = new List<int>();
            Up = new List<int>();
            Down = new List<int>();
                  
            LP = new List<int>();
            MP = new List<int>();
            HP = new List<int>();
            PPP = new List<int>();
                  
            LK = new List<int>();
            MK = new List<int>();
            HK = new List<int>();
            KKK = new List<int>();

            Clear = new List<int>();
        }

        public bool Empty()
        {
            return (Left.Count() +
                Right.Count() +
                Up.Count() +
                Down.Count() +
                LP.Count() +
                MP.Count() +
                HP.Count() +
                PPP.Count() +
                LK.Count() +
                MK.Count() +
                HK.Count() +
                KKK.Count() +
                Clear.Count()                
                ) == 0;



      }
    }
}
