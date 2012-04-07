#if DEVELOPMENT_ENVIRONMENT
#warning Built with DEVELOPMENT_ENVIRONMENT. Tables will be dropped upon schema change!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Alterity.Models
{
    public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<EntityMappingContext>
    {
    }
}

#endif