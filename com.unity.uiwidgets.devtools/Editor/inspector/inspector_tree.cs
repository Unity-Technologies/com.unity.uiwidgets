using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using uiwidgets;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.DevTools.inspector
{
    public class InspectorTreeUtils
    {
      public static readonly Regex treeNodePrimaryDescriptionPattern = new Regex(@"^([\w ]+)(.*)$");
      // TODO(jacobr): temporary workaround for missing structure from assertion thrown building
      // widget errors.
      public static readonly Regex assertionThrownBuildingError = new Regex(
        @"^(The following assertion was thrown building [a-zA-Z]+)(\(.*\))(:)$");

      public delegate void TreeEventCallback(InspectorTreeNode node);
      
      
      public static readonly float iconPadding = 5.0f;
      public static readonly float chartLineStrokeWidth = 1.0f;
      public static readonly float columnWidth = 16.0f;
      public static readonly float verticalPadding = 10.0f;
      public static readonly float rowHeight = 24.0f;
      
      
      // TODO(jacobr): merge this scheme with other color schemes in DevTools.
      public static Color selectedRowBackgroundColor
      {
        get
        {
      
          return CommonThemeUtils.isLight
            ? Color.fromARGB(255, 202, 191, 69)
            :  Color.fromARGB(255, 99, 101, 103);
        }
      }
      
      public static Color hoverColor
      {
        get
        {
          return CommonThemeUtils.isLight ? Colors.yellowAccent : Color.fromARGB(255, 70, 73, 76);
        }
      }
      
    }
    
// TODO(kenz): extend TreeNode class to share tree logic.
public class InspectorTreeNode {
  public InspectorTreeNode(
    InspectorTreeNode parent = null,
    bool expandChildren = true
  )
  {
    _children = new List<InspectorTreeNode>();
    _parent = parent;
    _isExpanded = expandChildren;
  }

  bool showLinesToChildren {
    get
    {
      return _children.Count > 1 && !_children.Last().isProperty;
    }
    
  }

  public bool isDirty
  {
    get
    {
      return _isDirty;
    }
    set 
    {
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

  

  /// Returns whether the node is currently visible in the tree.
  void updateShouldShow(bool value) {
    if (value != _shouldShow) {
      _shouldShow = value;
      foreach (var child in children) {
        child.updateShouldShow(value);
      }
    }
  }

  bool? shouldShow {
    get
    {
      _shouldShow = _shouldShow ?? parent == null || parent.isExpanded && parent.shouldShow.Value;
      return _shouldShow;
    }
    
  }

  bool? _shouldShow;

  public bool selected = false;

  RemoteDiagnosticsNode _diagnostic;
  public readonly List<InspectorTreeNode> _children;

  public IEnumerable<InspectorTreeNode> children
  {
    get
    {
      return _children;
    }
  }

  bool isCreatedByLocalProject
  {
    get
    {
      return _diagnostic.isCreatedByLocalProject;
    }
  }

  bool isProperty
  {
    get
    {
      return diagnostic == null || diagnostic.isProperty;
    }
  }

  public bool isExpanded
  {
    get
    {
      return _isExpanded;
    }
    set 
    {
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

  bool allowExpandCollapse = true;

  bool showExpandCollapse {
    get
    {
      return (diagnostic?.hasChildren == true || children.Any()) &&
             allowExpandCollapse;
    }
    
  }

  

  public InspectorTreeNode parent
  {
    get
    {
      return _parent;
    }
    set
    {
    _parent = value;
    _parent.isDirty = true;
  }
    
  }

  InspectorTreeNode _parent;

  

  public RemoteDiagnosticsNode diagnostic
  {
    get
    {
      return _diagnostic;
    }
    
    set
    {
    _diagnostic = value;
    _isExpanded = value.childrenReady;
    //isDirty = true;
  }
  }

  

  public int?  childrenCount {
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
        count += (child.subtreeSize?? 0);
      }
      _childrenCount = count;
      return _childrenCount;
    }
    
  }

  public bool hasPlaceholderChildren {
    get
    {
      return children.Count() == 1 && children.First().diagnostic == null;
    }
    
  }

  int? _childrenCount;

  public int? subtreeSize
  {
    get
    {
      return childrenCount + 1;
    }
  }

  bool isLeaf
  {
    get
    {
      return _children.isEmpty();
    }
  }

  // TODO(jacobr): move getRowIndex to the InspectorTree class.
  public int getRowIndex(InspectorTreeNode node) {
    int index = 0;
    while (true) {
      InspectorTreeNode parent = node.parent;
      if (parent == null) {
        break;
      }
      foreach (InspectorTreeNode sibling in parent._children) {
        if (sibling == node) {
          break;
        }
        index += (sibling.subtreeSize?? 0);
      }
      index += 1;
      node = parent;
    }
    return index;
  }

  // TODO(jacobr): move this method to the InspectorTree class.
  // TODO: optimize this method.
  /// Use [getCachedRow] wherever possible, as [getRow] is slow and can cause
  /// performance problems.
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
        current += (subtreeSize?? 0);
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
}

/// A row in the tree with all information required to render it.
public class InspectorTreeRow {
  public InspectorTreeRow(
    InspectorTreeNode node,
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

  /// Column indexes of ticks to draw lines from parents to children.
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

public delegate void NodeAddedCallback(InspectorTreeNode node, RemoteDiagnosticsNode diagnosticsNode);

public class InspectorTreeConfig {
  public delegate void  OnClientActiveChange(bool added);
  public InspectorTreeConfig(
    bool? summaryTree = null,
    FlutterTreeType? treeType = null,
    NodeAddedCallback onNodeAdded = null,
    OnClientActiveChange onClientActiveChange = null,
    VoidCallback onSelectionChange = null,
    InspectorTreeUtils.TreeEventCallback onExpand = null,
    InspectorTreeUtils.TreeEventCallback onHover = null
  )
  {
    this.summaryTree = summaryTree;
    this.treeType = treeType;
    this.onNodeAdded = onNodeAdded;
    this.onSelectionChange = onSelectionChange;
    this.onExpand = onExpand;
    this.onHover = onHover;
    
  }

  public readonly bool? summaryTree;
  public readonly FlutterTreeType? treeType;
  public readonly NodeAddedCallback onNodeAdded;
  public readonly VoidCallback onSelectionChange;
  public readonly OnClientActiveChange ONClientActiveChange;
  public readonly InspectorTreeUtils.TreeEventCallback onExpand;
  public readonly InspectorTreeUtils.TreeEventCallback onHover;
}

public abstract class InspectorTreeController
{

  protected  abstract void setState(VoidCallback fn);

  public InspectorTreeNode root
  {
    get
    {
      return _root;
    }
    set 
    {
    setState(() => {
      _root = value;
    });
  }
    
  }

  InspectorTreeNode _root;
  

  RemoteDiagnosticsNode subtreeRoot; // Optional.

  public InspectorTreeNode selection
  {
    get
    {
      return _selection;
    }
    set 
    {
    if (value == _selection) return;

    setState(() => {
      _selection.selected = false;
      _selection = value;
      _selection.selected = true;
      if (config.onSelectionChange != null) {
        config.onSelectionChange();
      }
    });
  }
    
  }

  InspectorTreeNode _selection;

  InspectorTreeConfig config
  {
    get
    {
      return _config;
    }
    set 
    {
    // Only allow setting config once.
    D.assert(_config == null);
    _config = value;
  }
    
  }

  InspectorTreeConfig _config;

  InspectorTreeNode hover
  {
    get
    {
      return _hover;
    }
    set
    {
    if (value == _hover) {
      return;
    }
    setState(() => {
      _hover = value;
      // TODO(jacobr): we could choose to repaint only a portion of the UI
    });
  }
    
  }

  InspectorTreeNode _hover;

  float? lastContentWidth;

  public abstract InspectorTreeNode createNode();

  public readonly List<InspectorTreeRow> cachedRows = new List<InspectorTreeRow>();

  // TODO: we should add a listener instead that clears the cache when the
  // root is marked as dirty.
  void _maybeClearCache() {
    if (root.isDirty) {
      cachedRows.Clear();
      root.isDirty = false;
      lastContentWidth = null;
    }
  }

  public InspectorTreeRow getCachedRow(int index) {
    _maybeClearCache();
    while (cachedRows.Count <= index) {
      cachedRows.Add(null);
    }
    cachedRows[index] = cachedRows[index] ??root.getRow(index);
    return cachedRows[index];
  }

  double getRowOffset(int index) {
    return (getCachedRow(index)?.depth ?? 0) * InspectorTreeUtils.columnWidth;
  }

  

  RemoteDiagnosticsNode currentHoverDiagnostic;

  void navigateUp() {
    _navigateHelper(-1);
  }

  void navigateDown() {
    _navigateHelper(1);
  }

  void navigateLeft() {
    // This logic is consistent with how IntelliJ handles tree navigation on
    // on left arrow key press.
    if (selection == null) {
      _navigateHelper(-1);
      return;
    }

    if (selection.isExpanded) {
      setState(() => {
        selection.isExpanded = false;
      });
      return;
    }
    if (selection.parent != null) {
      selection = selection.parent;
    }
  }

  void navigateRight() {
    // This logic is consistent with how IntelliJ handles tree navigation on
    // on right arrow key press.

    if (selection == null || selection.isExpanded) {
      _navigateHelper(1);
      return;
    }

    setState(() => {
      selection.isExpanded = true;
    });
  }

  void _navigateHelper(int indexOffset) {
    if (numRows == 0) return;

    if (selection == null) {
      selection = root;
      return;
    }

    selection = root
        .getRow(
            (root.getRowIndex(selection) + indexOffset).clamp(0, numRows.Value - 1))
        ?.node;
  }

  float horizontalPadding
  {
    get
    {
      return 10.0f;
    }
  }

  double getDepthIndent(int depth) {
    return (depth + 1) * InspectorTreeUtils.columnWidth + horizontalPadding;
  }

  double getRowY(int index) {
    return InspectorTreeUtils.rowHeight * index + InspectorTreeUtils.verticalPadding;
  }

  void nodeChanged(InspectorTreeNode node) {
    if (node == null) return;
    setState(() => {
      node.isDirty = true;
    });
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

  public void collapseToSelected() {
    setState(() => {
      _collapseAllNodes(root);
      if (selection == null) return;
      _expandPath(selection);
    });
  }

  void _collapseAllNodes(InspectorTreeNode root) {
    root.isExpanded = false;
    foreach (var child in root.children)
    {
        _collapseAllNodes(child);
    }
  }

  int? numRows
  {
    get
    {
      return root != null ? root.subtreeSize : 0;
    }
  }

  int getRowIndex(float y) => (int)((y - InspectorTreeUtils.verticalPadding) / InspectorTreeUtils.rowHeight);

  public InspectorTreeRow getRowForNode(InspectorTreeNode node) {
    return getCachedRow(root.getRowIndex(node));
  }

  InspectorTreeRow getRow(Offset offset) {
    if (root == null) return null;
    int row = getRowIndex(offset.dy);
    return row < root.subtreeSize ? getCachedRow(row) : null;
  }

  public abstract void animateToTargets(List<InspectorTreeNode> targets);

  void onExpandRow(InspectorTreeRow row) {
    setState(() => {
      row.node.isExpanded = true;
      if (config.onExpand != null) {
        config.onExpand(row.node);
      }
    });
  }

  void onCollapseRow(InspectorTreeRow row) {
    setState(() => {
      row.node.isExpanded = false;
    });
  }

  void onSelectRow(InspectorTreeRow row) {
    selection = row.node;
    expandPath(row.node);
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

  InspectorTreeNode setupInspectorTreeNode(
    InspectorTreeNode node,
    RemoteDiagnosticsNode diagnosticsNode,
    bool expandChildren = true,
    bool expandProperties = true
  ) {
    D.assert(expandChildren != null);
    D.assert(expandProperties != null);
    node.diagnostic = diagnosticsNode;
    if (config.onNodeAdded != null) {
      config.onNodeAdded(node, diagnosticsNode);
    }

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

  void setupChildren(
    RemoteDiagnosticsNode parent,
    InspectorTreeNode treeNode,
    List<RemoteDiagnosticsNode> children,
    bool expandChildren = true,
    bool expandProperties = true
  ) {
    D.assert(expandChildren != null);
    D.assert(expandProperties != null);
    treeNode.isExpanded = expandChildren;
    if (treeNode.children.Any()) {
      // Only case supported is this is the loading node.
      D.assert(treeNode.children.Count() == 1);
      removeNodeFromParent(treeNode.children.First());
    }
    var inlineProperties = parent.inlineProperties;

    if (inlineProperties != null) {
      foreach (RemoteDiagnosticsNode property in inlineProperties) {
        appendChild(
          treeNode,
          setupInspectorTreeNode(
            createNode(),
            property,
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

  public Future maybePopulateChildren(InspectorTreeNode treeNode) {
    RemoteDiagnosticsNode diagnostic = treeNode.diagnostic;
    if (diagnostic != null &&
        diagnostic.hasChildren &&
        (treeNode.hasPlaceholderChildren || !treeNode.children.Any())) {
      try
      {
        diagnostic.children.then((children) =>
        {
          if (treeNode.hasPlaceholderChildren || !treeNode.children.Any()) {
            setupChildren(
              diagnostic,
              treeNode,
              children as List<RemoteDiagnosticsNode>,
              expandChildren: true,
              expandProperties: false
            );
            nodeChanged(treeNode);
            if (treeNode == selection) {
              expandPath(treeNode);
            }
          }
        });

      } catch (Exception e) {
        Debug.Log(e.ToString());
      }
    }

    return null;
  }
}

mixin InspectorTreeFixedRowHeightController on InspectorTreeController {
  Rect getBoundingBox(InspectorTreeRow row);

  void scrollToRect(Rect targetRect);

  public override void animateToTargets(List<InspectorTreeNode> targets) {
    Rect targetRect;

    foreach (InspectorTreeNode target in targets) {
      var row = InspectorTreeController.getRowForNode(target);
      if (row != null) {
        var rowRect = getBoundingBox(row);
        targetRect =
            targetRect == null ? rowRect : targetRect.expandToInclude(rowRect);
      }
    }

    if (targetRect == null || targetRect.isEmpty) return;

    targetRect = targetRect.inflate(20.0f);
    scrollToRect(targetRect);
  }
}

}