using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraspIT_EEG.Model
{
    public class GyroChartDataObject
    {
        //public String Time { get; set; }
        public DateTime Time { get; set; }
        public int Value { get; set; }
    }

    public class EEGChartDataObject
    {
        //public String Time { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }

    public class SequenceNumberChartDataObject
    {
        //public String Time { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }

    public class PacketLossChartDataObject
    {
        //public String Time { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }
}
