    using System;
    using System.Collections.Generic;
    using Unity.UIWidgets.foundation;
    using Unity.UIWidgets.rendering;

    public interface TileContainerRenderObjectMixin<ChildType, ParentDataType> {
        void forEachChild(Action<ChildType> f);
        void remove(int index);

        void _removeChild(ChildType child);

        void removeAll();
        // void detach();
        // void attach(object owner);
        // void redepthChildren();
        // void visitChildren(RenderObjectVisitor visitor);
        // List<DiagnosticsNode> debugDescribeChildren();
    }