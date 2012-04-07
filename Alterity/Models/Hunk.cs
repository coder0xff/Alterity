using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    public abstract class Hunk
    {
        public int Id { get; set; }
        public abstract Hunk[] UndoPrior(Hunk hunk);
        public abstract Hunk[] RedoPrior(Hunk hunk);
    }
}