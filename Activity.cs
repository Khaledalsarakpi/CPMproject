using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPMproject.model
{
    class Activity
    {
        public int id;
        public string description;
        public int duration;
        public int es;
        public int ef;
        public int ls;
        public int lf;
        public List<Activity> successors=new List<Activity>();
        public List<Activity> predecessors=new List<Activity>();
    }
}
