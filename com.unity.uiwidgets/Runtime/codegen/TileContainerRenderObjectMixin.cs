    using System;

    public interface TileContainerRenderObjectMixin<ChildType, ParentDataType> {
        void forEachChild(Action<ChildType> f);
        
        void remove(int index);

        void _removeChild(ChildType child);

        void removeAll();
    }