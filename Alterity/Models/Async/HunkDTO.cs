using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class HunkDTO
    {
        public String type; //may be insertion, deletion, cut, copy, or paste. 
        public int startIndex;
        public int length;
        public String text;

        public Hunk Convert()
        {
            if (type == "insertion")
            {
                return new InsertionHunk(startIndex, text);
            }
            else if (type == "deletion")
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
            type = "insertion";
            startIndex = hunk.StartIndex;
            length = hunk.Length;
            text = hunk.Text;
        }

        public HunkDTO(DeletionHunk hunk)
        {
            type = "deletion";
            startIndex = hunk.StartIndex;
            length = hunk.Length;
            text = "";
        }

        public HunkDTO(NoOperationHunk hunk, String type)
        {
            this.type = type;
            startIndex = hunk.StartIndex;
            length = hunk.Length;
            text = "";
        }
    }
}