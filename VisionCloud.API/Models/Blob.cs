using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionCloud.API.Models
{
    public class Blob
    {
        public List<string> FileNames { get; set; }

        public string SASToken { get; set; }
    }
}
