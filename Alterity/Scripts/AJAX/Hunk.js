var HunkDTO = function (documentId, type, startIndex, length, text)
{
    this.documentId = documentId;
    this.type = type;
    this.startIndex = startIndex;
    this.length = length;
    this.text = text;
}

function TransmitHunk(hunk)
{
     $.ajax({
         cache: false,
         type: "POST",
         url: ApiLocation + 'Test/ReceiveHunk',
         contentType: 'application/json',
         dataType: "json",
         data: JSON.stringify(hunk),
         success: function (response) { }
     });
}

function HunkDTOTest()
{
    TransmitHunk(new HunkDTO(0, "insert", 0, 12, "Hello world!"));
}