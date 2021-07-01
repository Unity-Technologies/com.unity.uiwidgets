using System;using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using uiwidgets;
using Unity.UIWidgets.async;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEditor.Graphs;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.DevTools.inspector
{
    
    public static class InspectorTreeUtils{
        public static readonly float iconPadding = 5.0f;
        public static readonly float chartLineStrokeWidth = 1.0f;
        public static readonly float columnWidth = 16.0f;
        public static readonly float verticalPadding = 10.0f;
        public static readonly float rowHeight = 24.0f;
        public static readonly Regex treeNodePrimaryDescriptionPattern = new Regex(@"^([\w ]+)(.*)$");
        public static readonly Regex assertionThrownBuildingError = new Regex(
            @"^(The following assertion was thrown building [a-zA-Z]+)(\(.*\))(:)$");
        public static bool isLight = false;
        public static Color selectedRowBackgroundColor
        {
            get
            {
                
                return isLight
                    ? Color.fromARGB(255, 202, 191, 69)
                    : Color.fromARGB(255, 99, 101, 103);
            }
        }

        public static Color hoverColor
        {
            get
            {
                return isLight ? Colors.yellowAccent : Color.fromARGB(255, 70, 73, 76);
            }
        }
    }
    
    public abstract class InspectorTreeController 
    {
        public abstract void setState(VoidCallback fn);
        public abstract InspectorTreeNode createNode();
        
        public float? lastContentWidth;
        public int numRows
        {
            get
            {
                return root != null ? root.subtreeSize : 0;
            }
        }
        
        InspectorTreeNode selection => _selection;
        InspectorTreeNode _selection;
        
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
        
        bool _disposed = false;
        InspectorObjectGroupManager _treeGroups;
        
        public InspectorTreeConfig config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
            }
        }
        
        InspectorTreeConfig _config;
        
        public InspectorTreeNode hover => _hover;
        InspectorTreeNode _hover;
        List<InspectorTreeRow> cachedRows = new List<InspectorTreeRow>();
        
        void nodeChanged(InspectorTreeNode node) {
            if (node == null) return;
            setState(() => {
                node.isDirty = true;
            });
        }
        
        
        void _maybeClearCache() {
            if (root.isDirty) {
                cachedRows.Clear();
                root.isDirty = false;
                lastContentWidth = null;
            }
        }
        
        void expandPath(InspectorTreeNode node) {
            setState(() => {
                _expandPath(node);
            });
        }

        void _expandPath(InspectorTreeNode node) {
            while (node != null) {
                if (!node.isExpanded) {
                    node.isExpanded = true;
                }
                node = node.parent;
            }
        }
        
        public InspectorTreeRow getCachedRow(int index) {
            Debug.Log("getCachedRow: " + index);
            _maybeClearCache();
            while (cachedRows.Count <= index) {
                cachedRows.Add(null);
            }

            if (cachedRows.Count <= index || cachedRows[index] == null)
            {
                cachedRows[index] = root.getRow(index);
            }
            return cachedRows[index];
        }
        
        float horizontalPadding
        {
            get
            {
                return 10.0f;
            }
        }

        public float getDepthIndent(int? depth) {
            return ((depth??0) + 1) * InspectorTreeUtils.columnWidth + horizontalPadding;
        }
        
        void removeNodeFromParent(InspectorTreeNode node) {
            setState(() => {
                node.parent?.removeChild(node);
            });
        }
        
        
        void appendChild(InspectorTreeNode node, InspectorTreeNode child) {
            setState(() => {
                node.appendChild(child);
            });
        }
        
        public InspectorTreeNode setupInspectorTreeNode(
            InspectorTreeNode node,
            RemoteDiagnosticsNode diagnosticsNode,
            bool expandChildren = false,
            bool expandProperties = false
        ) {
            D.assert(expandChildren != null);
            D.assert(expandProperties != null);
            node.diagnostic = diagnosticsNode;
            // if (config.onNodeAdded != null) {
            //     config.onNodeAdded(node, diagnosticsNode);
            // }

            if (diagnosticsNode.hasChildren ||
                diagnosticsNode.inlineProperties.isNotEmpty()) {
                if (diagnosticsNode.childrenReady || !diagnosticsNode.hasChildren) {
                    bool styleIsMultiline =
                        expandPropertiesByDefault(diagnosticsNode.style.Value);
                    setupChildren(
                        diagnosticsNode,
                        node,
                        node.diagnostic.childrenNow,
                        expandChildren: expandChildren && styleIsMultiline,
                        expandProperties: expandProperties && styleIsMultiline
                    );
                } else {
                    node.clearChildren();
                    node.appendChild(createNode());
                }
            }
            return node;
        }
        
        bool expandPropertiesByDefault(DiagnosticsTreeStyle style) {
            // This code matches the text style defaults for which styles are
            //  by default and which aren't.
            switch (style) {
                case DiagnosticsTreeStyle.none:
                case DiagnosticsTreeStyle.singleLine:
                case DiagnosticsTreeStyle.errorProperty:
                    return false;

                case DiagnosticsTreeStyle.sparse:
                case DiagnosticsTreeStyle.offstage:
                case DiagnosticsTreeStyle.dense:
                case DiagnosticsTreeStyle.transition:
                case DiagnosticsTreeStyle.error:
                case DiagnosticsTreeStyle.whitespace:
                case DiagnosticsTreeStyle.flat:
                case DiagnosticsTreeStyle.shallow:
                case DiagnosticsTreeStyle.truncateChildren:
                    return true;
            }
            return true;
        }
        
        void setupChildren(
            RemoteDiagnosticsNode parent,
            InspectorTreeNode treeNode,
            List<RemoteDiagnosticsNode> children, 
            bool expandChildren = false,
            bool expandProperties = false
        ) {
            D.assert(expandChildren != null);
            D.assert(expandProperties != null);
            treeNode.isExpanded = expandChildren;
            if (treeNode.children.isNotEmpty()) {
                // Only case supported is this is the loading node.
                D.assert(treeNode.children.Count == 1);
                removeNodeFromParent(treeNode.children.first());
            }
            var inlineProperties = parent.inlineProperties;

            if (inlineProperties != null) {
                foreach (RemoteDiagnosticsNode property in inlineProperties) {
                    appendChild(
                        treeNode,
                        setupInspectorTreeNode(
                            createNode(),
                            property,
                            // We are inside a property so only expand children if
                            // expandProperties is true.
                            expandChildren: expandProperties,
                            expandProperties: expandProperties
                        )
                    );
                }
            }
            if (children != null) {
                foreach (RemoteDiagnosticsNode child in children) {
                    appendChild(
                        treeNode,
                        setupInspectorTreeNode(
                            createNode(),
                            child,
                            expandChildren: expandChildren,
                            expandProperties: expandProperties
                        )
                    );
                }
            }
        }
        
        public void onExpandRow(InspectorTreeRow row) {
            setState(() => {
                row.node.isExpanded = true;
                if (config.onExpand != null) {
                    config.onExpand(row.node);
                }
            });
        }

        public void onCollapseRow(InspectorTreeRow row) {
            setState(() => {
                row.node.isExpanded = false;
            });
        }
        
        public void maybePopulateChildren(InspectorTreeNode treeNode) {
            RemoteDiagnosticsNode diagnostic = treeNode.diagnostic;
            if (diagnostic != null &&
                diagnostic.hasChildren &&
                (treeNode.hasPlaceholderChildren || treeNode.children.isEmpty())) {
                try {
                    var children = diagnostic.children;
                    if (treeNode.hasPlaceholderChildren || treeNode.children.isEmpty()) {
                        setupChildren(
                            diagnostic,
                            treeNode,
                            children,
                            expandChildren: true,
                            expandProperties: false
                        );
                        nodeChanged(treeNode);
                        if (treeNode == selection) {
                            expandPath(treeNode);
                        }
                    }
                } catch (Exception e) {
                    Debug.Log(e.ToString());
                }
            }
        }
        
    }

    }

    public delegate void TreeEventCallback(InspectorTreeNode node);
    public delegate void OnClientActiveChange(bool added);
    
    public class InspectorTreeConfig {
        public InspectorTreeConfig(
            bool summaryTree = false,
            FlutterTreeType treeType = FlutterTreeType.widget,
            // NodeAddedCallback onNodeAdded,
            OnClientActiveChange onClientActiveChange = null,
            VoidCallback onSelectionChange = null,
            TreeEventCallback onExpand = null,
            TreeEventCallback onHover = null
        )
        {
            this.summaryTree = summaryTree;
            this.treeType = treeType;
            this.onSelectionChange = onSelectionChange;
            this.onClientActiveChange = onClientActiveChange;
            this.onExpand = onExpand;
            this.onHover = onHover;
        }

        public readonly bool summaryTree;
        public readonly FlutterTreeType treeType;
        // public readonly NodeAddedCallback onNodeAdded;
        public readonly VoidCallback onSelectionChange;
        public readonly OnClientActiveChange onClientActiveChange;
        public readonly TreeEventCallback onExpand;
        public readonly TreeEventCallback onHover;
    }
    
    
    public class InspectorTreeNode
    {
        bool showLinesToChildren {
            get
            {
                return _children.Count > 1 && !_children.Last().isProperty;
            }
        }
        public InspectorTreeNode parent
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
        public readonly List<InspectorTreeNode> _children = new List<InspectorTreeNode>();
        public bool selected = false;
        RemoteDiagnosticsNode _diagnostic;
        bool allowExpandCollapse = true;
        
        bool isProperty
        {
            get
            {
                return diagnostic == null || diagnostic.isProperty;
            }
        }

        public List<InspectorTreeNode> children
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

        public bool isExpanded
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
        public RemoteDiagnosticsNode diagnostic
        {
            get
            {
                return _diagnostic;
            }
            set
            {
                _diagnostic = value;
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
        
        public void removeChild(InspectorTreeNode child) {
            child.parent = null;
            var removed = _children.Remove(child);
            D.assert(removed != null);
            isDirty = true;
        }
        
        public void appendChild(InspectorTreeNode child) {
            _children.Add(child);
            child.parent = this;
            isDirty = true;
        }

        public void clearChildren() {
            _children.Clear();
            isDirty = true;
        }
        
        public bool showExpandCollapse {
            get
            {
                return (diagnostic?.hasChildren == true || children.isNotEmpty()) &&
                       allowExpandCollapse;
            }
            
        }
        
        public bool? shouldShow {
            get
            {
                if (_shouldShow != null)
                {
                    return _shouldShow;
                }
                _shouldShow = (parent == null || parent.isExpanded && (parent._shouldShow != null && parent.shouldShow.Value));
                return _shouldShow;
            }
            
        }
        
        public bool hasPlaceholderChildren {
            get
            {
                return children.Count == 1 && children.First().diagnostic == null;
            }
            
        }
        
    }
    
    public class InspectorTreeRow {
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

    public bool isSelected
    {
        get
        {
            return node.selected;
        }
    }
    

}