using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.DevTools.inspector
{
    
    public static class InspectorTreeUtils{
        public static readonly float iconPadding = 5.0f;
        public static readonly float chartLineStrokeWidth = 1.0f;
        public static readonly float columnWidth = 16.0f;
        public static readonly float verticalPadding = 10.0f;
        public static readonly float rowHeight = 24.0f;
    }
    
    abstract class InspectorTreeController
    {
        public float? lastContentWidth;
        public int numRows
        {
            get
            {
                return root != null ? root.subtreeSize : 0;
            }
        }
        
        public InspectorTreeNode root
        {
            get
            {
                return _root;
            }
            set {

                _root = value;
            }
        }

        InspectorTreeNode _root;
        
        public InspectorTreeRow getCachedRow(int index) {
            // _maybeClearCache();
            // while (cachedRows.length <= index) {
            //     cachedRows.add(null);
            // }
            // cachedRows[index] ??= root.getRow(index);
            // return cachedRows[index];
            return null;
        }
        
        float horizontalPadding
        {
            get
            {
                return 10.0f;
            }
        }

        public float getDepthIndent(int? depth) {
            return (depth.Value + 1) * InspectorTreeUtils.columnWidth + horizontalPadding;
        }
        
    }

    class InspectorTreeNode
    {
        bool showLinesToChildren {
            get
            {
                return _children.Count > 1 && !_children.Last().isProperty;
            }
        }
        InspectorTreeNode parent
        {
            get
            {
                return _parent;
            }
            set {
                _parent = value;
                if (_parent != null)
                {
                    _parent.isDirty = true;
                }
            }
        }

        InspectorTreeNode _parent;

        

        public bool isDirty
        {
            get
            {
                return _isDirty;
            }
            set {
                if (value) {
                    _isDirty = true;
                    _shouldShow = null;
                    if (_childrenCount == null) {
                        // Already dirty.
                        return;
                    }
                    _childrenCount = null;
                    if (parent != null) {
                        parent.isDirty = true;
                    }
                } else {
                    _isDirty = false;
                }
            }
        }

        bool _isDirty = true;
        bool? _shouldShow;
        public readonly List<InspectorTreeNode> _children;
        public bool selected = false;
        RemoteDiagnosticsNode _diagnostic;
        
        bool isProperty
        {
            get
            {
                return diagnostic == null || diagnostic.isProperty;
            }
        }

        List<InspectorTreeNode> children
        {
            get
            {
                return _children;
            }
        }

        void updateShouldShow(bool value) {
            if (value != _shouldShow) {
                _shouldShow = value;
                foreach (var child in children) {
                    child.updateShouldShow(value);
                }
            }
        }
        bool isExpanded
        {
            get
            {
                return _isExpanded;
            }
            set {
                if (value != _isExpanded) {
                    _isExpanded = value;
                    isDirty = true;
                    if (_shouldShow ?? false) {
                        foreach (var child in children) {
                            child.updateShouldShow(value);
                        }
                    }
                }
            }
        }
        
        bool _isExpanded;

        int? childrenCount {
            get
            {
                if (!isExpanded) {
                    _childrenCount = 0;
                }
                if (_childrenCount != null) {
                    return _childrenCount;
                }
                int count = 0;
                foreach (InspectorTreeNode child in _children) {
                    count += child.subtreeSize;
                }
                _childrenCount = count;
                return _childrenCount;
            }
        }
        
        int? _childrenCount;

        public int subtreeSize => childrenCount.GetValueOrDefault(0) + 1;
        RemoteDiagnosticsNode diagnostic
        {
            get
            {
                return _diagnostic;
            }
        }

        public InspectorTreeRow getRow(int index) {
            List<int> ticks = new List<int>();
            InspectorTreeNode node = this;
            if (subtreeSize <= index) {
                return null;
            }
            int current = 0;
            int depth = 0;
            while (node != null) {
                var style = node.diagnostic?.style;
                bool indented = style != DiagnosticsTreeStyle.flat &&
                                      style != DiagnosticsTreeStyle.error;
                if (current == index) {
                    return new InspectorTreeRow(
                        node: node,
                        index: index,
                        ticks: ticks,
                        depth: depth,
                        lineToParent:
                        !node.isProperty && index != 0 && node.parent.showLinesToChildren
                    );
                }
                D.assert(index > current);
                current++;
                List<InspectorTreeNode> children = node._children;
                int i;
                for (i = 0; i < children.Count; ++i) {
                    var child = children[i];
                    var subtreeSize = child.subtreeSize;
                    if (current + subtreeSize > index) {
                        node = child;
                        if (children.Count > 1 &&
                            i + 1 != children.Count &&
                            !children.Last().isProperty) {
                            if (indented) {
                                ticks.Add(depth);
                            }
                        }
                        break;
                    }
                    current += subtreeSize;
                }
                D.assert(i < children.Count);
                if (indented) {
                    depth++;
                }
            }
            D.assert(false); // internal error.
            return null;
        }
        
    }
    
    class InspectorTreeRow {
        public InspectorTreeRow(
            InspectorTreeNode node = null,
            int? index = null,
            List<int> ticks = null,
            int? depth = null,
            bool? lineToParent = null
        )
        {
            this.node = node;
            this.index = index;
            this.ticks = ticks;
            this.depth = depth;
            this.lineToParent = lineToParent;
        }

    public readonly InspectorTreeNode node;
    
    public readonly List<int> ticks;
    public readonly int? depth;
    public readonly int? index;
    public readonly bool? lineToParent;

    bool isSelected
    {
        get
        {
            return node.selected;
        }
    }
    }

}