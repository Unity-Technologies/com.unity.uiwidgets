namespace Unity.UIWidgets.foundation {



    public class AbstractNode  {
        public int depth {
            get { return _depth; }
        }

        int _depth = 0;

        protected void redepthChild(AbstractNode child) {
            D.assert(child.owner == owner);
            if (child._depth <= _depth) {
                child._depth = _depth + 1;
                child.redepthChildren();
            }
        }

        public virtual void redepthChildren() {
        }

        public object owner {
            get { return _owner; }
        }

        object _owner;

        public bool attached {
            get { return _owner != null; }
        }

        public virtual void attach(object owner) {
            D.assert(owner != null);
            D.assert(_owner == null);
            _owner = owner;
        }

        public virtual void detach() {
            D.assert(_owner != null);
            _owner = null;
            D.assert(parent == null || attached == parent.attached);
        }

        public AbstractNode parent {
            get { return _parent; }
        }

        AbstractNode _parent;

        protected virtual void adoptChild(AbstractNode child) {
            D.assert(child != null);
            D.assert(child._parent == null);
            D.assert(() => {
                var node = this;
                while (node.parent != null) {
                    node = node.parent;
                }
                D.assert(node != child); // indicates we are about to create a cycle
                return true;
            });

            child._parent = this;
            if (attached) {
                child.attach(_owner);
            }

            redepthChild(child);
        }

        protected virtual void dropChild(AbstractNode child) {
            D.assert(child != null);
            D.assert(child._parent == this);
            D.assert(child.attached == attached);

            child._parent = null;
            if (attached) {
                child.detach();
            }
        }
    }




    public class AbstractNodeMixinDiagnosticableTree  : DiagnosticableTree {
        public int depth {
            get { return _depth; }
        }

        int _depth = 0;

        protected void redepthChild(AbstractNodeMixinDiagnosticableTree child) {
            D.assert(child.owner == owner);
            if (child._depth <= _depth) {
                child._depth = _depth + 1;
                child.redepthChildren();
            }
        }

        public virtual void redepthChildren() {
        }

        public object owner {
            get { return _owner; }
        }

        object _owner;

        public bool attached {
            get { return _owner != null; }
        }

        public virtual void attach(object owner) {
            D.assert(owner != null);
            D.assert(_owner == null);
            _owner = owner;
        }

        public virtual void detach() {
            D.assert(_owner != null);
            _owner = null;
            D.assert(parent == null || attached == parent.attached);
        }

        public AbstractNodeMixinDiagnosticableTree parent {
            get { return _parent; }
        }

        AbstractNodeMixinDiagnosticableTree _parent;

        protected virtual void adoptChild(AbstractNodeMixinDiagnosticableTree child) {
            D.assert(child != null);
            D.assert(child._parent == null);
            D.assert(() => {
                var node = this;
                while (node.parent != null) {
                    node = node.parent;
                }
                D.assert(node != child); // indicates we are about to create a cycle
                return true;
            });

            child._parent = this;
            if (attached) {
                child.attach(_owner);
            }

            redepthChild(child);
        }

        protected virtual void dropChild(AbstractNodeMixinDiagnosticableTree child) {
            D.assert(child != null);
            D.assert(child._parent == this);
            D.assert(child.attached == attached);

            child._parent = null;
            if (attached) {
                child.detach();
            }
        }
    }
}