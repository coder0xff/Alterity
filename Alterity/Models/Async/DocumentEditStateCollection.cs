using System.Globalization;

namespace Alterity.Models.Async
{
    public class DocumentEditStateCollection
    {
        dynamic _dataObject;

        public static implicit operator DocumentEditStateCollection(System.Dynamic.DynamicObject dataObject)
        {
            return new DocumentEditStateCollection {_dataObject = dataObject};
        }

        public DocumentEditState this[Document document]
        {
            get
            {
                return _dataObject[document.Id.ToString(CultureInfo.InvariantCulture)];
            }
        }
    }
}