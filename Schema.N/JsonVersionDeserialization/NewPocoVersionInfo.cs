﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schema.N
{
    public class NewPocoVersionInfo<TPoco>
    {
        // TODO Assumes sequencial int versions.
        public int NextVersion { get; set; }

        public IVersionMatcher NextVersionMatcher { get; set; }

        public IEntityVersionDeserialization NextVersionDeserializer { get; set; }

        public IVersionNextConverterTypeless ToNextVersionConverter { get; set; }
    }
}
