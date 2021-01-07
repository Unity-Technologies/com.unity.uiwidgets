// AUTO-GENERATED, DO NOT EDIT BY HAND

using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.rendering {

    public abstract class RenderObjectWithChildMixinRenderObject<ChildType> : RenderObject, RenderObjectWithChildMixin<ChildType>, RenderObjectWithChildMixin where ChildType : RenderObject {
        public virtual bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is ChildType)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(ChildType) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(ChildType) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        internal ChildType _child;

        public ChildType child {
            get { return _child; }
            set {
                if (_child != null) {
                    dropChild(_child);
                }

                _child = value;
                if (_child != null) {
                    adoptChild(_child);
                }
            }
        }

        RenderObject RenderObjectWithChildMixin.child {
            get { return child; }
            set { child = (ChildType) value; }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (_child != null) {
                _child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (_child != null) {
                _child.detach();
            }
        }

        public override void redepthChildren() {
            if (_child != null) {
                redepthChild(_child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return child != null
                ? new List<DiagnosticsNode>{child.toDiagnosticsNode(name: "child")}
                : new List<DiagnosticsNode>();
        }
    }


    public abstract class RenderObjectWithChildMixinRenderBox<ChildType> : RenderBox, RenderObjectWithChildMixin<ChildType>, RenderObjectWithChildMixin where ChildType : RenderObject {
        public virtual bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is ChildType)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(ChildType) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(ChildType) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        internal ChildType _child;

        public ChildType child {
            get { return _child; }
            set {
                if (_child != null) {
                    dropChild(_child);
                }

                _child = value;
                if (_child != null) {
                    adoptChild(_child);
                }
            }
        }

        RenderObject RenderObjectWithChildMixin.child {
            get { return child; }
            set { child = (ChildType) value; }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (_child != null) {
                _child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (_child != null) {
                _child.detach();
            }
        }

        public override void redepthChildren() {
            if (_child != null) {
                redepthChild(_child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return child != null
                ? new List<DiagnosticsNode>{child.toDiagnosticsNode(name: "child")}
                : new List<DiagnosticsNode>();
        }
    }


    public abstract class RenderObjectWithChildMixinRenderSliver<ChildType> : RenderSliver, RenderObjectWithChildMixin<ChildType>, RenderObjectWithChildMixin where ChildType : RenderObject {
        public bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is ChildType)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(ChildType) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(ChildType) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        internal ChildType _child;

        public ChildType child {
            get { return _child; }
            set {
                if (_child != null) {
                    dropChild(_child);
                }

                _child = value;
                if (_child != null) {
                    adoptChild(_child);
                }
            }
        }

        RenderObject RenderObjectWithChildMixin.child {
            get { return child; }
            set { child = (ChildType) value; }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (_child != null) {
                _child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (_child != null) {
                _child.detach();
            }
        }

        public override void redepthChildren() {
            if (_child != null) {
                redepthChild(_child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return child != null
                ? new List<DiagnosticsNode>{child.toDiagnosticsNode(name: "child")}
                : new List<DiagnosticsNode>();
        }
    }



    public abstract class ContainerParentDataMixinParentData<ChildType> : ParentData, ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        public ChildType previousSibling { get; set; }

        public ChildType nextSibling { get; set; }

        public override void detach() {
            base.detach();

            D.assert(previousSibling == null);
            D.assert(nextSibling == null);

            // if (this.previousSibling != null) {
            //     var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
            //     previousSiblingParentData.nextSibling = this.nextSibling;
            // }

            // if (this.nextSibling != null) {
            //     var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
            //     nextSiblingParentData.previousSibling = this.previousSibling;
            // }

            // this.previousSibling = null;
            // this.nextSibling = null;
        }
    }



    public abstract class ContainerParentDataMixinBoxParentData<ChildType> : BoxParentData, ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        public ChildType previousSibling { get; set; }

        public ChildType nextSibling { get; set; }

        public override void detach() {
            base.detach();

            D.assert(previousSibling == null);
            D.assert(nextSibling == null);

            // if (this.previousSibling != null) {
            //     var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
            //     previousSiblingParentData.nextSibling = this.nextSibling;
            // }

            // if (this.nextSibling != null) {
            //     var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
            //     nextSiblingParentData.previousSibling = this.previousSibling;
            // }

            // this.previousSibling = null;
            // this.nextSibling = null;
        }
    }



    public abstract class ContainerParentDataMixinSliverPhysicalParentData<ChildType> : SliverPhysicalParentData, ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        public ChildType previousSibling { get; set; }

        public ChildType nextSibling { get; set; }

        public override void detach() {
            base.detach();

            D.assert(previousSibling == null);
            D.assert(nextSibling == null);

            // if (this.previousSibling != null) {
            //     var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
            //     previousSiblingParentData.nextSibling = this.nextSibling;
            // }

            // if (this.nextSibling != null) {
            //     var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
            //     nextSiblingParentData.previousSibling = this.previousSibling;
            // }

            // this.previousSibling = null;
            // this.nextSibling = null;
        }
    }



    public abstract class ContainerParentDataMixinSliverLogicalParentData<ChildType> : SliverLogicalParentData, ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        public ChildType previousSibling { get; set; }

        public ChildType nextSibling { get; set; }

        public  void detach() {

            D.assert(previousSibling == null);
            D.assert(nextSibling == null);

            // if (this.previousSibling != null) {
            //     var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
            //     previousSiblingParentData.nextSibling = this.nextSibling;
            // }

            // if (this.nextSibling != null) {
            //     var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
            //     nextSiblingParentData.previousSibling = this.previousSibling;
            // }

            // this.previousSibling = null;
            // this.nextSibling = null;
        }
    }





    public abstract class ContainerRenderObjectMixinRenderBox<ChildType, ParentDataType> : RenderBox, ContainerRenderObjectMixin
        where ChildType : RenderObject
        where ParentDataType : ParentData, ContainerParentDataMixin<ChildType> {

        bool _debugUltimatePreviousSiblingOf(ChildType child, ChildType equals = null) {
            ParentDataType childParentData = (ParentDataType) child.parentData;
            while (childParentData.previousSibling != null) {
                D.assert(childParentData.previousSibling != child);
                child = childParentData.previousSibling;
                childParentData = (ParentDataType) child.parentData;
            }

            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(ChildType child, ChildType equals = null) {
            ParentDataType childParentData = (ParentDataType) child.parentData;
            while (childParentData.nextSibling != null) {
                D.assert(childParentData.nextSibling != child);
                child = childParentData.nextSibling;
                childParentData = (ParentDataType) child.parentData;
            }

            return child == equals;
        }

        int _childCount = 0;

        public int childCount {
            get { return _childCount; }
        }

        public bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is ChildType)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(ChildType) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(ChildType) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        ChildType _firstChild;

        ChildType _lastChild;

        void _insertIntoChildList(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            D.assert(childParentData.nextSibling == null);
            D.assert(childParentData.previousSibling == null);

            _childCount++;
            D.assert(_childCount > 0);

            if (after == null) {
                childParentData.nextSibling = _firstChild;
                if (_firstChild != null) {
                    var firstChildParentData = (ParentDataType) _firstChild.parentData;
                    firstChildParentData.previousSibling = child;
                }

                _firstChild = child;
                _lastChild = _lastChild ?? child;
            } else {
                D.assert(_firstChild != null);
                D.assert(_lastChild != null);
                D.assert(_debugUltimatePreviousSiblingOf(after, equals: _firstChild));
                D.assert(_debugUltimateNextSiblingOf(after, equals: _lastChild));
                var afterParentData = (ParentDataType) after.parentData;
                if (afterParentData.nextSibling == null) {
                    D.assert(after == _lastChild);
                    childParentData.previousSibling = after;
                    afterParentData.nextSibling = child;
                    _lastChild = child;
                } else {
                    childParentData.nextSibling = afterParentData.nextSibling;
                    childParentData.previousSibling = after;
                    var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                    var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                    childPreviousSiblingParentData.nextSibling = child;
                    childNextSiblingParentData.previousSibling = child;
                    D.assert(afterParentData.nextSibling == child);
                }
            }
        }

        public virtual void insert(ChildType child, ChildType after = null) {
            D.assert(child != this, () => "A RenderObject cannot be inserted into itself.");
            D.assert(after != this,
                () => "A RenderObject cannot simultaneously be both the parent and the sibling of another RenderObject.");
            D.assert(child != after, () => "A RenderObject cannot be inserted after itself.");
            D.assert(child != _firstChild);
            D.assert(child != _lastChild);

            adoptChild(child);
            _insertIntoChildList(child, after);
        }

        public virtual void add(ChildType child) {
            insert(child, _lastChild);
        }

        public virtual void addAll(List<ChildType> children) {
            if (children != null) {
                children.ForEach(add);
            }
        }

        public void _removeFromChildList(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            D.assert(_debugUltimatePreviousSiblingOf(child, equals: _firstChild));
            D.assert(_debugUltimateNextSiblingOf(child, equals: _lastChild));
            D.assert(_childCount >= 0);

            if (childParentData.previousSibling == null) {
                D.assert(_firstChild == child);
                _firstChild = childParentData.nextSibling;
            } else {
                var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                childPreviousSiblingParentData.nextSibling = childParentData.nextSibling;
            }

            if (childParentData.nextSibling == null) {
                D.assert(_lastChild == child);
                _lastChild = childParentData.previousSibling;
            } else {
                var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                childNextSiblingParentData.previousSibling = childParentData.previousSibling;
            }

            childParentData.previousSibling = null;
            childParentData.nextSibling = null;
            _childCount--;
        }

        public virtual void remove(ChildType child) {
            _removeFromChildList(child);
            dropChild(child);
        }

        public virtual void removeAll() {
            ChildType child = _firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                var next = childParentData.nextSibling;
                childParentData.previousSibling = null;
                childParentData.nextSibling = null;
                dropChild(child);
                child = next;
            }

            _firstChild = null;
            _lastChild = null;
            _childCount = 0;
        }

        public void move(ChildType child, ChildType after = null) {
            D.assert(child != this);
            D.assert(after != this);
            D.assert(child != after);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            if (childParentData.previousSibling == after) {
                return;
            }

            _removeFromChildList(child);
            _insertIntoChildList(child, after);
            markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            ChildType child = _firstChild;
            while (child != null) {
                child.attach(owner);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void detach() {
            base.detach();
            ChildType child = _firstChild;
            while (child != null) {
                child.detach();
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void redepthChildren() {
            ChildType child = _firstChild;
            while (child != null) {
                redepthChild(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            ChildType child = _firstChild;
            while (child != null) {
                visitor(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public ChildType firstChild {
            get { return _firstChild; }
        }

        public ChildType lastChild {
            get { return _lastChild; }
        }

        public ChildType childBefore(ChildType child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            return childParentData.previousSibling;
        }

        public ChildType childAfter(ChildType child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            return childParentData.nextSibling;
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (firstChild != null) {
                ChildType child = firstChild;
                int count = 1;
                while (true) {
                    children.Add(child.toDiagnosticsNode(name: "child " + count));
                    if (child == lastChild) {
                        break;
                    }

                    count += 1;
                    var childParentData = (ParentDataType) child.parentData;
                    child = childParentData.nextSibling;
                }
            }

            return children;
        }

        void ContainerRenderObjectMixin.insert(RenderObject child, RenderObject after) {
            insert((ChildType) child, (ChildType) after);
        }

        void ContainerRenderObjectMixin.remove(RenderObject child) {
            remove((ChildType) child);
        }

        void ContainerRenderObjectMixin.move(RenderObject child, RenderObject after) {
            move((ChildType) child, (ChildType) after);
        }

        RenderObject ContainerRenderObjectMixin.firstChild {
            get { return firstChild; }
        }

        RenderObject ContainerRenderObjectMixin.lastChild {
            get { return lastChild; }
        }

        RenderObject ContainerRenderObjectMixin.childBefore(RenderObject child) {
            return childBefore((ChildType) child);
        }

        RenderObject ContainerRenderObjectMixin.childAfter(RenderObject child) {
            return childAfter((ChildType) child);
        }
    }


    public abstract class ContainerRenderObjectMixinRenderSliver<ChildType, ParentDataType> : RenderSliver, ContainerRenderObjectMixin
        where ChildType : RenderObject
        where ParentDataType : ParentData, ContainerParentDataMixin<ChildType> {

        bool _debugUltimatePreviousSiblingOf(ChildType child, ChildType equals = null) {
            ParentDataType childParentData = (ParentDataType) child.parentData;
            while (childParentData.previousSibling != null) {
                D.assert(childParentData.previousSibling != child);
                child = childParentData.previousSibling;
                childParentData = (ParentDataType) child.parentData;
            }

            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(ChildType child, ChildType equals = null) {
            ParentDataType childParentData = (ParentDataType) child.parentData;
            while (childParentData.nextSibling != null) {
                D.assert(childParentData.nextSibling != child);
                child = childParentData.nextSibling;
                childParentData = (ParentDataType) child.parentData;
            }

            return child == equals;
        }

        int _childCount = 0;

        public int childCount {
            get { return _childCount; }
        }

        public bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is ChildType)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(ChildType) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(ChildType) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        ChildType _firstChild;

        ChildType _lastChild;

        void _insertIntoChildList(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            D.assert(childParentData.nextSibling == null);
            D.assert(childParentData.previousSibling == null);

            _childCount++;
            D.assert(_childCount > 0);

            if (after == null) {
                childParentData.nextSibling = _firstChild;
                if (_firstChild != null) {
                    var firstChildParentData = (ParentDataType) _firstChild.parentData;
                    firstChildParentData.previousSibling = child;
                }

                _firstChild = child;
                _lastChild = _lastChild ?? child;
            } else {
                D.assert(_firstChild != null);
                D.assert(_lastChild != null);
                D.assert(_debugUltimatePreviousSiblingOf(after, equals: _firstChild));
                D.assert(_debugUltimateNextSiblingOf(after, equals: _lastChild));
                var afterParentData = (ParentDataType) after.parentData;
                if (afterParentData.nextSibling == null) {
                    D.assert(after == _lastChild);
                    childParentData.previousSibling = after;
                    afterParentData.nextSibling = child;
                    _lastChild = child;
                } else {
                    childParentData.nextSibling = afterParentData.nextSibling;
                    childParentData.previousSibling = after;
                    var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                    var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                    childPreviousSiblingParentData.nextSibling = child;
                    childNextSiblingParentData.previousSibling = child;
                    D.assert(afterParentData.nextSibling == child);
                }
            }
        }

        public virtual void insert(ChildType child, ChildType after = null) {
            D.assert(child != this, () => "A RenderObject cannot be inserted into itself.");
            D.assert(after != this,
                () => "A RenderObject cannot simultaneously be both the parent and the sibling of another RenderObject.");
            D.assert(child != after, () => "A RenderObject cannot be inserted after itself.");
            D.assert(child != _firstChild);
            D.assert(child != _lastChild);

            adoptChild(child);
            _insertIntoChildList(child, after);
        }

        public virtual void add(ChildType child) {
            insert(child, _lastChild);
        }

        public virtual void addAll(List<ChildType> children) {
            if (children != null) {
                children.ForEach(add);
            }
        }

        public void _removeFromChildList(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            D.assert(_debugUltimatePreviousSiblingOf(child, equals: _firstChild));
            D.assert(_debugUltimateNextSiblingOf(child, equals: _lastChild));
            D.assert(_childCount >= 0);

            if (childParentData.previousSibling == null) {
                D.assert(_firstChild == child);
                _firstChild = childParentData.nextSibling;
            } else {
                var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                childPreviousSiblingParentData.nextSibling = childParentData.nextSibling;
            }

            if (childParentData.nextSibling == null) {
                D.assert(_lastChild == child);
                _lastChild = childParentData.previousSibling;
            } else {
                var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                childNextSiblingParentData.previousSibling = childParentData.previousSibling;
            }

            childParentData.previousSibling = null;
            childParentData.nextSibling = null;
            _childCount--;
        }

        public virtual void remove(ChildType child) {
            _removeFromChildList(child);
            dropChild(child);
        }

        public virtual void removeAll() {
            ChildType child = _firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                var next = childParentData.nextSibling;
                childParentData.previousSibling = null;
                childParentData.nextSibling = null;
                dropChild(child);
                child = next;
            }

            _firstChild = null;
            _lastChild = null;
            _childCount = 0;
        }

        public void move(ChildType child, ChildType after = null) {
            D.assert(child != this);
            D.assert(after != this);
            D.assert(child != after);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            if (childParentData.previousSibling == after) {
                return;
            }

            _removeFromChildList(child);
            _insertIntoChildList(child, after);
            markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            ChildType child = _firstChild;
            while (child != null) {
                child.attach(owner);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void detach() {
            base.detach();
            ChildType child = _firstChild;
            while (child != null) {
                child.detach();
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void redepthChildren() {
            ChildType child = _firstChild;
            while (child != null) {
                redepthChild(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            ChildType child = _firstChild;
            while (child != null) {
                visitor(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public ChildType firstChild {
            get { return _firstChild; }
        }

        public ChildType lastChild {
            get { return _lastChild; }
        }

        public ChildType childBefore(ChildType child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            return childParentData.previousSibling;
        }

        public ChildType childAfter(ChildType child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            return childParentData.nextSibling;
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (firstChild != null) {
                ChildType child = firstChild;
                int count = 1;
                while (true) {
                    children.Add(child.toDiagnosticsNode(name: "child " + count));
                    if (child == lastChild) {
                        break;
                    }

                    count += 1;
                    var childParentData = (ParentDataType) child.parentData;
                    child = childParentData.nextSibling;
                }
            }

            return children;
        }

        void ContainerRenderObjectMixin.insert(RenderObject child, RenderObject after) {
            insert((ChildType) child, (ChildType) after);
        }

        void ContainerRenderObjectMixin.remove(RenderObject child) {
            remove((ChildType) child);
        }

        void ContainerRenderObjectMixin.move(RenderObject child, RenderObject after) {
            move((ChildType) child, (ChildType) after);
        }

        RenderObject ContainerRenderObjectMixin.firstChild {
            get { return firstChild; }
        }

        RenderObject ContainerRenderObjectMixin.lastChild {
            get { return lastChild; }
        }

        RenderObject ContainerRenderObjectMixin.childBefore(RenderObject child) {
            return childBefore((ChildType) child);
        }

        RenderObject ContainerRenderObjectMixin.childAfter(RenderObject child) {
            return childAfter((ChildType) child);
        }
    }


    public abstract class ContainerRenderObjectMixinRenderProxyBoxMixinRenderObjectWithChildMixinRenderBoxRenderStack<ChildType, ParentDataType> : RenderProxyBoxMixinRenderObjectWithChildMixinRenderBoxRenderStack, ContainerRenderObjectMixin
        where ChildType : RenderObject
        where ParentDataType : ParentData, ContainerParentDataMixin<ChildType> {

        bool _debugUltimatePreviousSiblingOf(ChildType child, ChildType equals = null) {
            ParentDataType childParentData = (ParentDataType) child.parentData;
            while (childParentData.previousSibling != null) {
                D.assert(childParentData.previousSibling != child);
                child = childParentData.previousSibling;
                childParentData = (ParentDataType) child.parentData;
            }

            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(ChildType child, ChildType equals = null) {
            ParentDataType childParentData = (ParentDataType) child.parentData;
            while (childParentData.nextSibling != null) {
                D.assert(childParentData.nextSibling != child);
                child = childParentData.nextSibling;
                childParentData = (ParentDataType) child.parentData;
            }

            return child == equals;
        }

        int _childCount = 0;

        public int childCount {
            get { return _childCount; }
        }

        public override bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is ChildType)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(ChildType) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(ChildType) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        ChildType _firstChild;

        ChildType _lastChild;

        void _insertIntoChildList(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            D.assert(childParentData.nextSibling == null);
            D.assert(childParentData.previousSibling == null);

            _childCount++;
            D.assert(_childCount > 0);

            if (after == null) {
                childParentData.nextSibling = _firstChild;
                if (_firstChild != null) {
                    var firstChildParentData = (ParentDataType) _firstChild.parentData;
                    firstChildParentData.previousSibling = child;
                }

                _firstChild = child;
                _lastChild = _lastChild ?? child;
            } else {
                D.assert(_firstChild != null);
                D.assert(_lastChild != null);
                D.assert(_debugUltimatePreviousSiblingOf(after, equals: _firstChild));
                D.assert(_debugUltimateNextSiblingOf(after, equals: _lastChild));
                var afterParentData = (ParentDataType) after.parentData;
                if (afterParentData.nextSibling == null) {
                    D.assert(after == _lastChild);
                    childParentData.previousSibling = after;
                    afterParentData.nextSibling = child;
                    _lastChild = child;
                } else {
                    childParentData.nextSibling = afterParentData.nextSibling;
                    childParentData.previousSibling = after;
                    var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                    var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                    childPreviousSiblingParentData.nextSibling = child;
                    childNextSiblingParentData.previousSibling = child;
                    D.assert(afterParentData.nextSibling == child);
                }
            }
        }

        public virtual void insert(ChildType child, ChildType after = null) {
            D.assert(child != this, () => "A RenderObject cannot be inserted into itself.");
            D.assert(after != this,
                () => "A RenderObject cannot simultaneously be both the parent and the sibling of another RenderObject.");
            D.assert(child != after, () => "A RenderObject cannot be inserted after itself.");
            D.assert(child != _firstChild);
            D.assert(child != _lastChild);

            adoptChild(child);
            _insertIntoChildList(child, after);
        }

        public virtual void add(ChildType child) {
            insert(child, _lastChild);
        }

        public virtual void addAll(List<ChildType> children) {
            if (children != null) {
                children.ForEach(add);
            }
        }

        public void _removeFromChildList(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            D.assert(_debugUltimatePreviousSiblingOf(child, equals: _firstChild));
            D.assert(_debugUltimateNextSiblingOf(child, equals: _lastChild));
            D.assert(_childCount >= 0);

            if (childParentData.previousSibling == null) {
                D.assert(_firstChild == child);
                _firstChild = childParentData.nextSibling;
            } else {
                var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                childPreviousSiblingParentData.nextSibling = childParentData.nextSibling;
            }

            if (childParentData.nextSibling == null) {
                D.assert(_lastChild == child);
                _lastChild = childParentData.previousSibling;
            } else {
                var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                childNextSiblingParentData.previousSibling = childParentData.previousSibling;
            }

            childParentData.previousSibling = null;
            childParentData.nextSibling = null;
            _childCount--;
        }

        public virtual void remove(ChildType child) {
            _removeFromChildList(child);
            dropChild(child);
        }

        public virtual void removeAll() {
            ChildType child = _firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                var next = childParentData.nextSibling;
                childParentData.previousSibling = null;
                childParentData.nextSibling = null;
                dropChild(child);
                child = next;
            }

            _firstChild = null;
            _lastChild = null;
            _childCount = 0;
        }

        public void move(ChildType child, ChildType after = null) {
            D.assert(child != this);
            D.assert(after != this);
            D.assert(child != after);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            if (childParentData.previousSibling == after) {
                return;
            }

            _removeFromChildList(child);
            _insertIntoChildList(child, after);
            markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            ChildType child = _firstChild;
            while (child != null) {
                child.attach(owner);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void detach() {
            base.detach();
            ChildType child = _firstChild;
            while (child != null) {
                child.detach();
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void redepthChildren() {
            ChildType child = _firstChild;
            while (child != null) {
                redepthChild(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            ChildType child = _firstChild;
            while (child != null) {
                visitor(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public ChildType firstChild {
            get { return _firstChild; }
        }

        public ChildType lastChild {
            get { return _lastChild; }
        }

        public ChildType childBefore(ChildType child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            return childParentData.previousSibling;
        }

        public ChildType childAfter(ChildType child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (ParentDataType) child.parentData;
            return childParentData.nextSibling;
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (firstChild != null) {
                ChildType child = firstChild;
                int count = 1;
                while (true) {
                    children.Add(child.toDiagnosticsNode(name: "child " + count));
                    if (child == lastChild) {
                        break;
                    }

                    count += 1;
                    var childParentData = (ParentDataType) child.parentData;
                    child = childParentData.nextSibling;
                }
            }

            return children;
        }

        void ContainerRenderObjectMixin.insert(RenderObject child, RenderObject after) {
            insert((ChildType) child, (ChildType) after);
        }

        void ContainerRenderObjectMixin.remove(RenderObject child) {
            remove((ChildType) child);
        }

        void ContainerRenderObjectMixin.move(RenderObject child, RenderObject after) {
            move((ChildType) child, (ChildType) after);
        }

        RenderObject ContainerRenderObjectMixin.firstChild {
            get { return firstChild; }
        }

        RenderObject ContainerRenderObjectMixin.lastChild {
            get { return lastChild; }
        }

        RenderObject ContainerRenderObjectMixin.childBefore(RenderObject child) {
            return childBefore((ChildType) child);
        }

        RenderObject ContainerRenderObjectMixin.childAfter(RenderObject child) {
            return childAfter((ChildType) child);
        }
    }


}