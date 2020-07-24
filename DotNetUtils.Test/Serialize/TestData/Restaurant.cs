using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Test.Serialize.TestData
{
    public class Restaurant
    {
        public Guid Identifier { get; set; }

        public string Name { get; set; }

        public string Street { get; set; }

        public int ZipCode { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string Phone { get; set; }

        public string GoogleMapsUrl { get; set; }

        public string WebAddress { get; set; }
    }
}
