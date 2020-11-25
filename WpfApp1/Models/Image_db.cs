using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WpfApp1.Models
{
    public class Image_db
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public float Confidence { get; set; }
//        public byte[] ByteRepresent;
        public Detail Det { get; set; }
        public int Counter { get; set; }
    }
    public class Detail
    {
        public int Id { get; set; }
        public byte[] ByteRepresent { get; set; }
    }
}
