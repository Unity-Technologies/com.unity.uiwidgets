using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.external;


public abstract class TileContainerRenderObjectMixinRenderSliver<ChildType, ParentDataType>: RenderSliver, TileContainerRenderObjectMixin<ChildType, ParentDataType>
    where ChildType : RenderObject
    where ParentDataType : ParentData
{
    // need to make SplayTree public
    SplayTree<int, ChildType> _childRenderObjects = new SplayTree<int, ChildType>();

    /// The number of children.
    protected int childCount
    {
        get => _childRenderObjects.Count;
    }

    protected ICollection<ChildType> children
    {
        get => _childRenderObjects.Values;
    }

    protected ICollection<int> indices
    {
        get => _childRenderObjects.Keys;
    }

    /// Checks whether the given render object has the correct [runtimeType] to be
    /// a child of this render object.
    ///
    /// Does nothing if assertions are disabled.
    ///
    /// Always returns true.
    public bool debugValidateChild(RenderObject child)
    {
        D.assert(() =>
        {
            if (!(child is ChildType))
            {
                throw new UIWidgetsError(
                    "A $runtimeType expected a child of type $ChildType but received a " +
                    "child of type ${child.runtimeType}.\n" +
                    "RenderObjects expect specific types of children because they " +
                    "coordinate with their children during layout and paint. For " +
                    "example, a RenderSliver cannot be the child of a RenderBox because " +
                    "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                    "\n" +
                    "The $runtimeType that expected a $ChildType child was created by:\n" +
                    "  $debugCreator\n" +
                    "\n" +
                    "The ${child.runtimeType} that did not match the expected child type " +
                    "was created by:\n" +
                    "  ${child.debugCreator}\n");
            }

            return true;
        });
        return true;
    }

    public ChildType this[int index]
    {
        get => _childRenderObjects[index];
        set

        {
            if (index < 0)
            {
                throw new ArgumentException($"index {index}");
            }

            _removeChild(_childRenderObjects.getOrDefault(index, null));
            adoptChild(value);
            _childRenderObjects[index] = value;
        }
    }

    public virtual void forEachChild(Action<ChildType> f)
    {
        _childRenderObjects.Values.ToList().ForEach(f);
    }

    /// Remove the child at the specified index from the child list.
    public virtual void remove(int index)
    {
        var child = _childRenderObjects[index];
        _childRenderObjects.Remove(index);
        _removeChild(child);
    }

    public virtual void _removeChild(ChildType child)
    {
        if (child != null)
        {
            // Remove the old child.
            dropChild(child);
        }
    }

    /// Remove all their children from this render object's child list.
    ///
    /// More efficient than removing them individually.
    public virtual void removeAll()
    {
        _childRenderObjects.Values.ToList().ForEach(dropChild);
        _childRenderObjects.Clear();
    }

    public override void attach(object owner)
    {
        base.attach(owner);
        _childRenderObjects.Values.ToList().ForEach((child) => child.attach(owner));
    }

    public override void detach()
    {
        base.detach();
        _childRenderObjects.Values.ToList().ForEach((child) => child.detach());
    }

    public override void redepthChildren()
    {
        _childRenderObjects.Values.ToList().ForEach(redepthChild);
    }

    public override void visitChildren(RenderObjectVisitor visitor)
    {
        _childRenderObjects.Values.ToList().ForEach(r => visitor(r));
    }

    public override List<DiagnosticsNode> debugDescribeChildren()
    {
        List<DiagnosticsNode> children = new List<DiagnosticsNode>();
        _childRenderObjects.ToList().ForEach(pair =>
            children.Add(pair.Value.toDiagnosticsNode(name: $"child {pair.Key}")));
        return children;
    }
}

