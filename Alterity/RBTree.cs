///<summary>
///A red-black tree must satisfy these properties:
///
///1. The root is black. 
///2. All leaves are black. 
///3. Red nodes can only have black children. 
///4. All paths from a node to its leaves contain the same number of black nodes.
///</summary>

using System.Linq;

namespace System.Collections.Generic
{
    public enum RBTreeColor
    {
        Black,
        Red
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class RBTree<T> : ISet<T>
    {

        /// <summary>
        /// the number of nodes contained in the tree
        /// </summary>
        private int count;
        /// <summary>
        /// the tree
        /// </summary>
        private Node root;
        /// <summary>
        /// SentinelNode is convenient way of indicating a leaf node.
        /// </summary>
        private static readonly Node SentinelNode = new Node(RBTreeColor.Black);
        private IComparer<T> _comparer;
        /// <summary>
        /// the node that was last found; used to optimize searches
        /// </summary>
        private Node lastNodeFound;

        private void Init()
        {
            root = SentinelNode;
            lastNodeFound = SentinelNode;
        }

        public RBTree()
        {
            Init();
            _comparer = Comparer<T>.Default;
        }

        public RBTree(IComparer<T> comparer)
        {
            Init();
            _comparer = comparer;
        }

        public RBTree(IEnumerable<T> initialItems)
        {
            Init();
            AddRange(initialItems);
        }

        public RBTree(IEnumerable<T> initialItems, IComparer<T> comparer)
        {
            Init();
            _comparer = comparer;
            AddRange(initialItems);
        }

        ///<summary>
        /// Rebalance the tree by rotating the nodes to the left
        ///</summary>
        private void RotateLeft(Node x)
        {
            // pushing node x down and to the Left to balance the tree. x's Right child (y)
            // replaces x (since y > x), and y's Left child becomes x's Right child 
            // (since it's < y but > x).

            Node y = x.Right;			// get x's Right node, this becomes y

            // set x's Right link
            x.Right = y.Left;					// y's Left child's becomes x's Right child

            // modify parents
            if (y.Left != SentinelNode)
                y.Left.Parent = x;				// sets y's Left Parent to x

            if (y != SentinelNode)
                y.Parent = x.Parent;			// set y's Parent to x's Parent

            if (x.Parent != null)
            {	// determine which side of it's Parent x was on
                if (x == x.Parent.Left)
                    x.Parent.Left = y;			// set Left Parent to y
                else
                    x.Parent.Right = y;			// set Right Parent to y
            }
            else
                root = y;						// at rbTree, set it to y

            // link x and y 
            y.Left = x;							// put x on y's Left 
            if (x != SentinelNode)						// set y as x's Parent
                x.Parent = y;
        }
        ///<summary>
        /// Rebalance the tree by rotating the nodes to the right
        ///</summary>
        private void RotateRight(Node x)
        {
            // pushing node x down and to the Right to balance the tree. x's Left child (y)
            // replaces x (since x < y), and y's Right child becomes x's Left child 
            // (since it's < x but > y).

            Node y = x.Left;			// get x's Left node, this becomes y

            // set x's Right link
            x.Left = y.Right;					// y's Right child becomes x's Left child

            // modify parents
            if (y.Right != SentinelNode)
                y.Right.Parent = x;				// sets y's Right Parent to x

            if (y != SentinelNode)
                y.Parent = x.Parent;			// set y's Parent to x's Parent

            if (x.Parent != null)				// null=rbTree, could also have used rbTree
            {	// determine which side of it's Parent x was on
                if (x == x.Parent.Right)
                    x.Parent.Right = y;			// set Right Parent to y
                else
                    x.Parent.Left = y;			// set Left Parent to y
            }
            else
                root = y;						// at rbTree, set it to y

            // link x and y 
            y.Right = x;						// put x on y's Right
            if (x != SentinelNode)				// set y as x's Parent
                x.Parent = y;
        }
        ///<summary>
        /// Additions to red-black trees usually destroy the red-black 
        /// properties. Examine the tree and restore. Rotations are normally 
        /// required to restore it
        ///</summary>
        private void RestoreAfterInsert(Node x)
        {
            // x and y are used as variable names for brevity, in a more formal
            // implementation, you should probably change the names

            Node y;

            // maintain red-black tree properties after adding x
            while (x != root && x.Parent.Color == RBTreeColor.Red)
            {
                // Parent node is .Colored red; 
                if (x.Parent == x.Parent.Parent.Left)	// determine traversal path			
                {										// is it on the Left or Right subtree?
                    y = x.Parent.Parent.Right;			// get uncle
                    if (y != null && y.Color == RBTreeColor.Red)
                    {	// uncle is red; change x's Parent and uncle to black
                        x.Parent.Color = RBTreeColor.Black;
                        y.Color = RBTreeColor.Black;
                        // grandparent must be red. Why? Every red node that is not 
                        // a leaf has only black children 
                        x.Parent.Parent.Color = RBTreeColor.Red;
                        x = x.Parent.Parent;	// continue loop with grandparent
                    }
                    else
                    {
                        // uncle is black; determine if x is greater than Parent
                        if (x == x.Parent.Right)
                        {	// yes, x is greater than Parent; rotate Left
                            // make x a Left child
                            x = x.Parent;
                            RotateLeft(x);
                        }
                        // no, x is less than Parent
                        x.Parent.Color = RBTreeColor.Black;	// make Parent black
                        x.Parent.Parent.Color = RBTreeColor.Red;		// make grandparent black
                        RotateRight(x.Parent.Parent);					// rotate right
                    }
                }
                else
                {	// x's Parent is on the Right subtree
                    // this code is the same as above with "Left" and "Right" swapped
                    y = x.Parent.Parent.Left;
                    if (y != null && y.Color == RBTreeColor.Red)
                    {
                        x.Parent.Color = RBTreeColor.Black;
                        y.Color = RBTreeColor.Black;
                        x.Parent.Parent.Color = RBTreeColor.Red;
                        x = x.Parent.Parent;
                    }
                    else
                    {
                        if (x == x.Parent.Left)
                        {
                            x = x.Parent;
                            RotateRight(x);
                        }
                        x.Parent.Color = RBTreeColor.Black;
                        x.Parent.Parent.Color = RBTreeColor.Red;
                        RotateLeft(x.Parent.Parent);
                    }
                }
            }
            root.Color = RBTreeColor.Black;		// rbTree should always be black
        }

        bool TryAdd(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            // traverse tree - find where node belongs
            int result = 0;
            // create new node
            Node node = new Node();
            Node temp = root; // grab the rbTree node of the tree

            while (temp != SentinelNode)
            {   // find Parent
                node.Parent = temp;
                result = _comparer.Compare(item, temp.Value);
                if (result == 0)
                    return false;
                if (result > 0)
                    temp = temp.Right;
                else
                    temp = temp.Left;
            }

            // setup node
            node.Value = item;
            node.Left = SentinelNode;
            node.Right = SentinelNode;

            // insert node into tree starting at parent's location
            if (node.Parent != null)
            {
                result = _comparer.Compare(node.Value, node.Parent.Value);
                if (result > 0)
                    node.Parent.Right = node;
                else
                    node.Parent.Left = node;
            }
            else
                root = node;					// first node added

            RestoreAfterInsert(node);           // restore red-black properties

            lastNodeFound = node;

            count = count + 1;

            return true;
        }

        bool ISet<T>.Add(T item)
        {
            return TryAdd(item);
        }
        /// <summary>
        /// Add a item and item pair
        /// </summary>
        /// <param name="item"> the item to add</param>
        /// <param name="item"> the item to associate with the item</param>
        public void Add(T item)
        {
            if (!TryAdd(item)) throw (new ArgumentException("An element with the same key already exists."));
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");
            foreach (T item in items)
                Add(item);
        }

        ///<summary>
        /// Gets the data object associated with the specified item
        ///<summary>
        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            int result;

            Node treeNode = root; // begin at root

            // traverse tree until node is found
            while (treeNode != SentinelNode)
            {
                result = _comparer.Compare(item, treeNode.Value);
                if (result == 0)
                {
                    lastNodeFound = treeNode;
                    return true;
                }
                if (result < 0)
                    treeNode = treeNode.Left;
                else
                    treeNode = treeNode.Right;
            }

            return false;
        }
        ///<summary>
        /// Deletions from red-black trees may destroy the red-black 
        /// properties. Examine the tree and restore. Rotations are normally 
        /// required to restore it
        ///</summary>
        private void RestoreAfterDelete(Node x)
        {
            // maintain Red-Black tree balance after deleting node 			

            Node y;

            while (x != root && x.Color == RBTreeColor.Black)
            {
                if (x == x.Parent.Left)			// determine sub tree from parent
                {
                    y = x.Parent.Right;			// y is x's sibling 
                    if (y.Color == RBTreeColor.Red)
                    {	// x is black, y is red - make both black and rotate
                        y.Color = RBTreeColor.Black;
                        x.Parent.Color = RBTreeColor.Red;
                        RotateLeft(x.Parent);
                        y = x.Parent.Right;
                    }
                    if (y.Left.Color == RBTreeColor.Black &&
                        y.Right.Color == RBTreeColor.Black)
                    {	// children are both black
                        y.Color = RBTreeColor.Red;		// change parent to red
                        x = x.Parent;					// move up the tree
                    }
                    else
                    {
                        if (y.Right.Color == RBTreeColor.Black)
                        {
                            y.Left.Color = RBTreeColor.Black;
                            y.Color = RBTreeColor.Red;
                            RotateRight(y);
                            y = x.Parent.Right;
                        }
                        y.Color = x.Parent.Color;
                        x.Parent.Color = RBTreeColor.Black;
                        y.Right.Color = RBTreeColor.Black;
                        RotateLeft(x.Parent);
                        x = root;
                    }
                }
                else
                {	// right subtree - same as code above with right and left swapped
                    y = x.Parent.Left;
                    if (y.Color == RBTreeColor.Red)
                    {
                        y.Color = RBTreeColor.Black;
                        x.Parent.Color = RBTreeColor.Red;
                        RotateRight(x.Parent);
                        y = x.Parent.Left;
                    }
                    if (y.Right.Color == RBTreeColor.Black &&
                        y.Left.Color == RBTreeColor.Black)
                    {
                        y.Color = RBTreeColor.Red;
                        x = x.Parent;
                    }
                    else
                    {
                        if (y.Left.Color == RBTreeColor.Black)
                        {
                            y.Right.Color = RBTreeColor.Black;
                            y.Color = RBTreeColor.Red;
                            RotateLeft(y);
                            y = x.Parent.Left;
                        }
                        y.Color = x.Parent.Color;
                        x.Parent.Color = RBTreeColor.Black;
                        y.Left.Color = RBTreeColor.Black;
                        RotateRight(x.Parent);
                        x = root;
                    }
                }
            }
            x.Color = RBTreeColor.Black;
        }

        ///<summary>
        /// Removes the item and data object (delete)
        ///<summary>
        public bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            // find node
            int result;
            Node node;

            // see if node to be deleted was the last one found
            result = _comparer.Compare(item, lastNodeFound.Value);
            if (result == 0)
                node = lastNodeFound;
            else
            {	// not found, must search		
                node = root;
                while (node != SentinelNode)
                {
                    result = _comparer.Compare(item, node.Value);
                    if (result == 0)
                        break;
                    if (result < 0)
                        node = node.Left;
                    else
                        node = node.Right;
                }

                if (node == SentinelNode)
                    return false;				// item not found
            }

            Remove(node);

            count = count - 1;

            return true;
        }
        ///<summary>
        /// Remove a node
        ///<summary>
        private void Remove(Node z)
        {
            // A node to be deleted will be: 
            //		1. a leaf with no children
            //		2. have one child
            //		3. have two children
            // If the deleted node is red, the red black properties still hold.
            // If the deleted node is black, the tree needs rebalancing

            Node x = new Node();	// work node to contain the replacement node
            Node y;					// work node 

            // find the replacement node (the successor to x) - the node one with 
            // at *most* one child. 
            if (z.Left == SentinelNode || z.Right == SentinelNode)
                y = z;						// node has sentinel as a child
            else
            {
                // z has two children, find replacement node which will 
                // be the leftmost node greater than z
                y = z.Right;				        // traverse right subtree	
                while (y.Left != SentinelNode)		// to find next node in sequence
                    y = y.Left;
            }

            // at this point, y contains the replacement node. it's content will be copied 
            // to the valules in the node to be deleted

            // x (y's only child) is the node that will be linked to y's old parent. 
            if (y.Left != SentinelNode)
                x = y.Left;
            else
                x = y.Right;

            // replace x's parent with y's parent and
            // link x to proper subtree in parent
            // this removes y from the chain
            x.Parent = y.Parent;
            if (y.Parent != null)
                if (y == y.Parent.Left)
                    y.Parent.Left = x;
                else
                    y.Parent.Right = x;
            else
                root = x;			// make x the root node

            // copy the items from y (the replacement node) to the node being deleted.
            // note: this effectively deletes the node. 
            if (y != z)
            {
                z.Value = y.Value;
            }

            if (y.Color == RBTreeColor.Black)
                RestoreAfterDelete(x);

            lastNodeFound = SentinelNode;
        }

        ///<summary>
        /// Clear
        /// Empties or clears the tree
        ///<summary>
        public void Clear()
        {
            root = SentinelNode;
            count = 0;
        }

        public IEnumerator<T> GetEnumerator(bool ascending)
        {
            return new Enumerator(root, ascending);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // elements is simply a generic name to refer to the 
            // data objects the nodes contain
            return GetEnumerator(true);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Length < arrayIndex + Count) throw new ArgumentException("The number of elements in the source ICollection<T> is greater than the available space from arrayIndex to the end of the destination array.");
            foreach (T item in this)
                array[arrayIndex++] = item;
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            foreach (T item in other)
                Remove(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            SortedSet<T> tempSet = new SortedSet<T>(other);
            foreach (T itemToRemove in this.Where(item => !tempSet.Contains(item)))
            {
                Remove(itemToRemove);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return this.IsSubsetOf(other) && other.Count() < this.Count;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return this.IsSupersetOf(other) && other.Count() > this.Count;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            SortedSet<T> tempSet = new SortedSet<T>(other);
            return this.All(item => tempSet.Contains(item));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return other.All(item => this.Contains(item));
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return other.Any(item => this.Contains(item));
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            IEnumerator<T> thisIterator = this.GetEnumerator(), thatIterator = other.GetEnumerator();
            bool hasElement;
            while ((hasElement = thisIterator.MoveNext()) == thatIterator.MoveNext())
            {
                if (hasElement)
                {
                    if (_comparer.Compare(thisIterator.Current, thatIterator.Current) != 0) return false; //found differing element
                }
                else
                {
                    return true;
                }
            }
            return false; //different number of elements
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            foreach (T item in other)
            {
                if (Contains(item))
                    Remove(item);
                else
                    Add(item);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            foreach (T item in other)
                TryAdd(item);
        }
        internal static Node NextNode(Node currentNode)
        {
            //1. if node has right child, move to right child, go to step 5
            //2. if current node has no parent, return null
            //3. if current node is right, move to parent and return to step 2
            //4. return parent
            //5. if current node has left child, move to left child and repeat step 5
            //6. return current node
            if (currentNode.Right != SentinelNode) // step 1
            {
                currentNode = currentNode.Right; // step 1
                while (currentNode.Left != SentinelNode) // step 5
                    currentNode = currentNode.Left; // step 5
                return currentNode; // step 6
            }
            while (true)
            {
                if (currentNode.Parent == null) return null; // step 2
                if (currentNode.Parent.Right == currentNode) // step 3
                    currentNode = currentNode.Parent;
                else
                    break;
            }
            return currentNode.Parent; // step 4
        }

        internal static Node PreviousNode(Node currentNode)
        {
            if (currentNode.Left != SentinelNode) // step 1
            {
                currentNode = currentNode.Left; // step 1
                while (currentNode.Right != SentinelNode) // step 5
                    currentNode = currentNode.Right; // step 5
                return currentNode; // step 6
            }
            while (true)
            {
                if (currentNode.Parent == null) return null; // step 2
                if (currentNode.Parent.Left == currentNode) // step 3
                    currentNode = currentNode.Parent;
                else
                    break;
            }
            return currentNode.Parent; // step 4
        }

        internal Node GetLessThanOrEqual(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            Node treeNode = root; // begin at root

            // traverse tree until node is found
            Node greatestLess = null;
            int result;
            while (treeNode != SentinelNode)
            {
                result = _comparer.Compare(item, treeNode.Value);
                if (result == 0)
                {
                    lastNodeFound = treeNode;
                    return treeNode;
                }
                if (result < 0)
                {
                    treeNode = treeNode.Left;
                }
                else
                {
                    greatestLess = treeNode;
                    treeNode = treeNode.Right;
                }
            }
            return greatestLess;
        }

        internal Node GetGreaterThanOrEqual(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            Node treeNode = root; // begin at root

            // traverse tree until node is found
            Node leastGreater = null;
            int result;
            while (treeNode != SentinelNode)
            {
                result = _comparer.Compare(item, treeNode.Value);
                if (result == 0)
                {
                    lastNodeFound = treeNode;
                    return treeNode;
                }
                if (result < 0)
                {
                    leastGreater = treeNode;
                    treeNode = treeNode.Left;
                }
                else
                {
                    treeNode = treeNode.Right;
                }
            }
            return leastGreater;
        }

        internal class Node
        {
            /// <summary>
            /// The item
            /// </summary>
            public T Value { get; internal set; }
            /// <summary>
            /// color - used to balance the tree
            /// </summary>
            public RBTreeColor Color { get; internal set; }
            /// <summary>
            /// left node 
            /// </summary>
            public Node Left { get; internal set; }
            /// <summary>
            /// right node 
            /// </summary>
            public Node Right { get; internal set; }
            /// <summary>
            /// parent node 
            /// </summary>
            public Node Parent { get; internal set; }

            internal Node()
            {
                Color = RBTreeColor.Red;
            }

            internal Node(RBTreeColor color)
            {
                Color = color;
            }
        }

        internal class Enumerator : IEnumerator<T>
        {
            // use a stack to order the nodes
            private Stack<Node> stack;
            // return in ascending order (true) or descending (false)
            private bool ascending;
            // the root of the tree
            private Node _root;

            private Node _currentNode;

            ///<summary>
            /// Determine order, walk the tree and push the nodes onto the stack
            ///</summary>
            internal Enumerator(Node root, bool ascending)
            {
                _root = root;
                this.ascending = ascending;
                Reset();
            }
            ///<summary>
            /// HasMoreElements
            ///</summary>
            private bool HasMoreElements()
            {
                return (stack.Count > 0);
            }
            ///<summary>
            /// NextElement
            ///</summary>
            private void NextElement()
            {
                // the top of stack will always have the next item
                // get top of stack but don't remove it as the next nodes in sequence
                // may be pushed onto the top
                // the stack will be popped after all the nodes have been returned
                _currentNode = stack.Peek();	//next node in sequence

                if (ascending)
                {
                    if (_currentNode.Right == RBTree<T>.SentinelNode)
                    {
                        // yes, top node is lowest node in subtree - pop node off stack 
                        Node tn = stack.Pop();
                        // peek at right node's parent 
                        // get rid of it if it has already been used
                        while (HasMoreElements() && ((Node)stack.Peek()).Right == tn)
                            tn = (Node)stack.Pop();
                    }
                    else
                    {
                        // find the next items in the sequence
                        // traverse to left; find lowest and push onto stack
                        Node tn = _currentNode.Right;
                        while (tn != RBTree<T>.SentinelNode)
                        {
                            stack.Push(tn);
                            tn = tn.Left;
                        }
                    }
                }
                else            // descending, same comments as above apply
                {
                    if (_currentNode.Left == RBTree<T>.SentinelNode)
                    {
                        // walk the tree
                        Node tn = (Node)stack.Pop();
                        while (HasMoreElements() && ((Node)stack.Peek()).Left == tn)
                            tn = (Node)stack.Pop();
                    }
                    else
                    {
                        // determine next node in sequence
                        // traverse to left subtree and find greatest node - push onto stack
                        Node tn = _currentNode.Left;
                        while (tn != RBTree<T>.SentinelNode)
                        {
                            stack.Push(tn);
                            tn = tn.Right;
                        }
                    }
                }
            }
            ///<summary>
            /// MoveNext
            /// For .NET compatibility
            ///</summary>
            public bool MoveNext()
            {
                if (HasMoreElements())
                {
                    NextElement();
                    return true;
                }
                _currentNode = null;
                return false;
            }

            public T Current
            {
                get { return _currentNode.Value; }
            }

            public Node CurrentNode
            {
                get { return _currentNode; }
            }

            public void Dispose()
            {
                _currentNode = null;
                stack = null;
                _root = null;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Reset()
            {
                _currentNode = null;
                stack = new Stack<Node>();
                var tnode = _root;
                // use depth-first traversal to push nodes into stack
                // the lowest node will be at the top of the stack
                if (ascending)
                {   // find the lowest node
                    while (tnode != RBTree<T>.SentinelNode)
                    {
                        stack.Push(tnode);
                        tnode = tnode.Left;
                    }
                }
                else
                {
                    // the highest node will be at top of stack
                    while (tnode != RBTree<T>.SentinelNode)
                    {
                        stack.Push(tnode);
                        tnode = tnode.Right;
                    }
                }
            }
        }

    }
}
