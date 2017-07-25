using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Schema.N
{
    public class VersionResponseWrapper<TEntity> : IVersionResponseWrapper<TEntity>
    {
        public TEntity Entity { get; set; }

        public int EntityVersion { get; set; }

        public Type EntityType { get; set; }
    }
}
