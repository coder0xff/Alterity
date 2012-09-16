function NodeList()
{
}

NodeList.prototype = new Array;
NodeList.item = function (index) { return this[index]; }