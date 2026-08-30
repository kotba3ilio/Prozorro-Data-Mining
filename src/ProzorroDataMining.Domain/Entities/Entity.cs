using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProzorroDataMining.Domain.Entities;

public class Entity
{
    public Guid Id { get; private set;  }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }
}
