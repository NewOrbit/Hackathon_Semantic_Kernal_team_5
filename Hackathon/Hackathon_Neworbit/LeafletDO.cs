using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackathon_Neworbit
{
    public class LeafletDO
    {
        // fields[0] => Category
        // fields[1] => Product Name
        // fields[2] => Manufacturer Name
        // fields[3] => Intended use
        // fields[4] => Active ingredients
        // fields[5] => Dosage
        // fields[6] => Leaflet
        [Index(0)]
        public string Category { get; set; }
        
        [Index(1)]
        public string ProductName { get; set; }

        [Index(2)]
        public string ManufacturerName { get; set; }

        [Index(3)]
        public string IntendedUse { get; set; }

        [Index(4)]
        public string ActiveIngredients { get; set; }

        [Index(5)]
        public string Dosage { get; set; }

        [Index(6)]
        public string Leaflet { get; set; }
    }
}
