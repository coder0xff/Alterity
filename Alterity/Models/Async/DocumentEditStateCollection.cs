using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class DocumentEditStateCollection
    {
        dynamic dataObject;

        public static implicit operator DocumentEditStateCollection(Doredis.DynamicDataObject dataObject)
        {
            DocumentEditStateCollection result = new DocumentEditStateCollection();
            result.dataObject = dataObject;
            return result;
        }

        public DocumentEditState this[Document document]
        {
            get
            {
                return dataObject[document.Id.ToString()];
            }
        }
    }
}