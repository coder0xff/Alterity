using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class HunkDTO
    {
        public int documentId;
        public String type; //may be insert, delete, cut, copy, or paste. 
        public int startIndex;
        public int length;
        public String text;

        public Hunk Convert()
        {
            if (type == "insert")
            {
                return new InsertionHunk(startIndex, text);
            }
            else if (type == "delete")
            {
                return new DeletionHunk(startIndex, length);
            }
            else
            {
                return new NoOperationHunk(startIndex, length);
            }
        }

        public HunkDTO() {}

        public HunkDTO(InsertionHunk hunk)
        {
            type = "insert";
            startIndex = hunk.StartIndex;
            length = hunk.Length;
            text = hunk.Text;
            documentId = hunk.Document.Id;
        }

        public HunkDTO(DeletionHunk hunk)
        {
            type = "delete";
            startIndex = hunk.StartIndex;
            length = hunk.Length;
            text = "";
            documentId = hunk.Document.Id;
        }

        public HunkDTO(NoOperationHunk hunk, String type)
        {
            this.type = type;
            startIndex = hunk.StartIndex;
            length = hunk.Length;
            text = "";
            documentId = hunk.Document.Id;
        }
    }
}