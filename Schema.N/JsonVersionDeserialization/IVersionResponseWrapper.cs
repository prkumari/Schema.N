using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schema.N
{
    public interface IVersionResponseWrapper<TEntity>
    {
        TEntity Entity { get; set; }

        int EntityVersion { get; set; }

        Type EntityType { get; set; }
    }
}
