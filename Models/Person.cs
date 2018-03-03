using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthCore2.Models
{
    public class Person
    {
        public string name { get; set; }
        public string birthday { get; set; }
        public string[] hobbies { get; set; }
    }
}
