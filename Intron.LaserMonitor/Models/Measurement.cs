using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intron.LaserMonitor.Models
{
    public class Measurement
    {
        public DateTime Timestamp { get; set; }
        public double Distance { get; set; }
        public double DistanceAbsolute { get; set; }
    }
}
