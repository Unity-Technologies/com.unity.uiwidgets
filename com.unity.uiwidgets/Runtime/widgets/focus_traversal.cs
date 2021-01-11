using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;


namespace Unity.UIWidgets.widgets {
    public enum TraversalDirection {
        up,
        right,
        down,
        left,
        // TODO(gspencer): Add diagonal traversal directions used by TV remotes and
        // game controllers when we support them.
    }

    
    public class FocusTravesalUtils {
        public static void _focusAndEnsureVisible(
            FocusNode node,
            ScrollPositionAlignmentPolicy alignmentPolicy = ScrollPositionAlignmentPolicy.explicitPolicy
        ) {
            node.requestFocus();
            Scrollable.ensureVisible(node.context, alignment: 1.0f, alignmentPolicy: alignmentPolicy);
        }
        public static BuildContext _getAncestor(BuildContext context, int count = 1) {
            BuildContext target = null;
            context.visitAncestorElements((Element ancestor)=> {
                count--;
                if (count == 0) {
                    target = ancestor;
                    return false;
                }
                return true;
            });
            return target;
        }

        public static HashSet<T> difference<T>(HashSet<T> aSet, HashSet<T> bSet) {
            HashSet<T> result = new HashSet<T>();
            foreach (var a in aSet) {
                if (!bSet.Contains(a)) {
                    result.Add(a);
                }
            }
            return result;
        }
        public static HashSet<T> intersaction<T>(HashSet<T> aSet, HashSet<T> bSet) {
            HashSet<T> result = new HashSet<T>();
            result = aSet;
            foreach (var b in bSet) {
                if (!aSet.Contains(b)) {
                    result.Add(b);
                }
            }
            return result;
        }
        



    }

    public class FocusTraversalGroup : StatefulWidget {
        public FocusTraversalGroup(
            Key key = null,
            FocusTraversalPolicy policy = null,
            Widget child = null
        ) : base(key: key) {
            policy = policy ;//?? new ReadingOrderTraversalPolicy();
            this.child = child;
        }

        public readonly Widget child;
        public readonly FocusTraversalPolicy policy;
        public static FocusTraversalPolicy of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            _FocusTraversalGroupMarker inherited = context?.dependOnInheritedWidgetOfExactType<_FocusTraversalGroupMarker>();
            D.assert(() =>{
                if (nullOk) {
                    return true;
                }
                if (inherited == null) {
                    throw new UIWidgetsError(
                        "Unable to find a FocusTraversalGroup widget in the context.\n" + 
                        "FocusTraversalGroup.of() was called with a context that does not contain a " +
                        "FocusTraversalGroup.\n" +
                        "No FocusTraversalGroup ancestor could be found starting from the context that was " +
                        "passed to FocusTraversalGroup.of(). This can happen because there is not a " +
                        "WidgetsApp or MaterialApp widget (those widgets introduce a FocusTraversalGroup), " +
                        "or it can happen if the context comes from a widget above those widgets.\n" +
                        "The context used was:\n" + 
                        $"  {context}"
                    );
                }
                return true;
            });
            return inherited?.policy;
        }

        public override State createState() {
            return new _FocusTraversalGroupState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<FocusTraversalPolicy>("policy", policy));
        }
    }
    public class _FocusTraversalGroupState : State<FocusTraversalGroup> {
        FocusNode focusNode;
        public override void initState() {
            base.initState();
            focusNode = new FocusNode(
              canRequestFocus: false,
              skipTraversal: true,
              debugLabel: "FocusTraversalGroup"
            );
        }
        public override void dispose() { 
            focusNode?.dispose(); 
            base.dispose(); 
        }
        public override Widget build(BuildContext context) { 
            return new _FocusTraversalGroupMarker(
                policy: widget.policy,
                focusNode: focusNode,
                child: new Focus(
                    focusNode: focusNode,
                    canRequestFocus: false,
                    skipTraversal: true,
                    includeSemantics: false,
                    child: widget.child
                )
            );
        }
    }
    public class _FocusTraversalGroupMarker : InheritedWidget { 
        public _FocusTraversalGroupMarker(
            FocusTraversalPolicy policy = null,
            FocusNode focusNode = null, 
            Widget child = null
            )  : base(child: child) {
                D.assert(policy != null);
                D.assert(focusNode != null);
                this.policy = policy;
                this.focusNode = focusNode;
        }
        public readonly FocusTraversalPolicy policy; 
        public readonly FocusNode focusNode;
        public override bool updateShouldNotify(InheritedWidget oldWidget) => false;
    }

    public abstract class FocusTraversalPolicy : Diagnosticable {
        public FocusTraversalPolicy() {
        }
        public class _FocusTraversalGroupInfo {
            public _FocusTraversalGroupInfo(
                _FocusTraversalGroupMarker marker,
                FocusTraversalPolicy defaultPolicy = null,
                List<FocusNode> members = null
            ) {
                groupNode = marker?.focusNode;
                policy = marker?.policy ?? defaultPolicy ;//?? new ReadingOrderTraversalPolicy();
                members = members ?? new List<FocusNode>();
            }
            public readonly FocusNode groupNode;
            public readonly FocusTraversalPolicy policy;
            public readonly List<FocusNode> members;
        }

        public virtual FocusNode findFirstFocus(FocusNode currentNode) {
            D.assert(currentNode != null);
            FocusScopeNode scope = currentNode.nearestScope;
            FocusNode candidate = scope.focusedChild;
            if (candidate == null && scope.descendants.Count() != 0) {
                List<FocusNode> sorted = _sortAllDescendants(scope);
                candidate = sorted.isNotEmpty() ? sorted.First() : null;
            }

            candidate ??= currentNode;
            return candidate;
        }

        public abstract FocusNode findFirstFocusInDirection(FocusNode currentNode, TraversalDirection direction);

        public abstract void invalidateScopeData(FocusScopeNode node);

        public abstract void changedScope(FocusNode node = null, FocusScopeNode oldScope = null);
        bool next(FocusNode currentNode) => _moveFocus(currentNode, forward: true);

        bool previous(FocusNode currentNode) => _moveFocus(currentNode, forward: false);

        public abstract bool inDirection(FocusNode currentNode, TraversalDirection direction);

        public abstract IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants);
        public _FocusTraversalGroupMarker _getMarker(BuildContext context) {
            return context?.getElementForInheritedWidgetOfExactType<_FocusTraversalGroupMarker>()?.widget as _FocusTraversalGroupMarker;
        }
        public List<FocusNode> _sortAllDescendants(FocusScopeNode scope) { 
            D.assert(scope != null); 
            _FocusTraversalGroupMarker scopeGroupMarker = _getMarker(scope.context);
            FocusTraversalPolicy defaultPolicy = scopeGroupMarker?.policy ;//?? new ReadingOrderTraversalPolicy();
            Dictionary<FocusNode, _FocusTraversalGroupInfo> groups = new Dictionary<FocusNode, _FocusTraversalGroupInfo>();
            foreach(FocusNode node in scope.descendants) { 
                _FocusTraversalGroupMarker groupMarker = _getMarker(node.context);
                FocusNode groupNode = groupMarker?.focusNode;
                if (node == groupNode) {
                    BuildContext parentContext =FocusTravesalUtils._getAncestor(groupNode.context, count: 2); 
                    _FocusTraversalGroupMarker parentMarker = _getMarker(parentContext); 
                    FocusNode parentNode = parentMarker?.focusNode; 
                    groups[parentNode] ??= new _FocusTraversalGroupInfo(parentMarker, members: new List<FocusNode>(), defaultPolicy: defaultPolicy);
                    D.assert( !groups[parentNode].members.Contains(node) );
                    groups[parentNode].members.Add(groupNode);
                    continue;
                }
                if (node.canRequestFocus && !node.skipTraversal) { 
                    groups[groupNode] ??= new _FocusTraversalGroupInfo(groupMarker, members: new List<FocusNode>(), defaultPolicy: defaultPolicy); 
                    D.assert(!groups[groupNode].members.Contains(node)); 
                    groups[groupNode].members.Add(node);
                }
            }
            HashSet<FocusNode> groupKeys = new HashSet<FocusNode>(groups.Keys);
            foreach ( FocusNode key in groups.Keys) { 
                List<FocusNode> sortedMembers = groups[key].policy.sortDescendants(groups[key].members).ToList(); 
                groups[key].members.Clear(); 
                groups[key].members.AddRange(sortedMembers); 
            }

            List<FocusNode> sortedDescendants = new List<FocusNode>(); 
            void visitGroups(_FocusTraversalGroupInfo info) { 
                foreach ( FocusNode node in info.members) { 
                    if (groupKeys.Contains(node)) {
                        visitGroups(groups[node]); 
                    } else { 
                        sortedDescendants.Add(node); 
                    } 
                } 
            }
            visitGroups(groups[scopeGroupMarker?.focusNode]); 
            D.assert(
                FocusTravesalUtils.difference(new HashSet<FocusNode>(sortedDescendants),(new HashSet<FocusNode>(scope.traversalDescendants))).isEmpty(), 
                ()=>$"sorted descendants contains more nodes than it should: ({FocusTravesalUtils.difference(new HashSet<FocusNode>(sortedDescendants),(new HashSet<FocusNode>(scope.traversalDescendants)))})"
                ); 
            D.assert(
                FocusTravesalUtils.difference(new HashSet<FocusNode>(scope.traversalDescendants),new HashSet<FocusNode>(sortedDescendants)).isEmpty(), 
                ()=>$"sorted descendants are missing some nodes: ({FocusTravesalUtils.difference(new HashSet<FocusNode>(scope.traversalDescendants),new HashSet<FocusNode>(sortedDescendants))})"
                ); 
            return sortedDescendants; 
        }
        protected bool _moveFocus(FocusNode currentNode,  bool forward = false) { 
            D.assert(forward != null); 
            if (currentNode == null) { 
                return false; 
            } 
            FocusScopeNode nearestScope = currentNode.nearestScope; 
            invalidateScopeData(nearestScope); 
            FocusNode focusedChild = nearestScope.focusedChild; 
            if (focusedChild == null) { 
                FocusNode firstFocus = findFirstFocus(currentNode); 
                if (firstFocus != null) { 
                    FocusTravesalUtils._focusAndEnsureVisible(
                        firstFocus, 
                        alignmentPolicy: forward ? ScrollPositionAlignmentPolicy.keepVisibleAtEnd : ScrollPositionAlignmentPolicy.keepVisibleAtStart
                        ); 
                    return true; 
                } 
            } 
            List<FocusNode> sortedNodes = _sortAllDescendants(nearestScope); 
            if (forward && focusedChild == sortedNodes.Last()) { 
                FocusTravesalUtils._focusAndEnsureVisible(sortedNodes.First(), alignmentPolicy: ScrollPositionAlignmentPolicy.keepVisibleAtEnd); 
                return true; 
            } 
            if (!forward && focusedChild == sortedNodes.First()) { 
                FocusTravesalUtils._focusAndEnsureVisible(sortedNodes.Last(), alignmentPolicy: ScrollPositionAlignmentPolicy.keepVisibleAtStart); 
                return true; 
            }

            IEnumerable<FocusNode> maybeFlipped = new List<FocusNode>();
            if (forward) {
                maybeFlipped = sortedNodes;
            }
            else {
                sortedNodes.Reverse();
                maybeFlipped = sortedNodes;
            }
            FocusNode previousNode = null; 
            foreach ( FocusNode node in maybeFlipped) { 
                if (previousNode == focusedChild) { 
                    FocusTravesalUtils._focusAndEnsureVisible(
                        node, 
                        alignmentPolicy: forward ? ScrollPositionAlignmentPolicy.keepVisibleAtEnd : ScrollPositionAlignmentPolicy.keepVisibleAtStart
                        ); 
                    return true; 
                } 
                previousNode = node; 
            } 
            return false; 
        } 
    }
    public class _DirectionalPolicyDataEntry {
        public _DirectionalPolicyDataEntry(
            TraversalDirection direction ,
            FocusNode node ) {
            this.direction = direction;
            this.node = node;

        } 
        public readonly TraversalDirection direction;
        public readonly FocusNode node;
    }
    public class _ReadingOrderSortData : Diagnosticable {
        public _ReadingOrderSortData(FocusNode node) {
            D.assert(node != null);
            this.node = node;
            rect = node.rect;
            directionality = _findDirectionality(node.context);
        }

        public readonly TextDirection directionality;
        public readonly Rect rect;
        public readonly FocusNode node;

        public static TextDirection _findDirectionality(BuildContext context) {
            return (context.getElementForInheritedWidgetOfExactType<Directionality>().widget as Directionality).textDirection;
        }
        public static TextDirection commonDirectionalityOf(List<_ReadingOrderSortData> list) { 
            IEnumerable<HashSet<Directionality>> allAncestors = list.Select((_ReadingOrderSortData member) => new HashSet<Directionality>(member.directionalAncestors)); 
            HashSet<Directionality> common = null; 
            foreach ( HashSet<Directionality> ancestorSet in allAncestors) { 
                common ??= ancestorSet; 
                common = FocusTravesalUtils.intersaction(common,ancestorSet); 
            } 
            if (common.isEmpty()) {
                return list.First().directionality; 
            }
            foreach (var com in list.First().directionalAncestors) {
                if (common.Contains(com)) {
                    return com.textDirection;
                }
            }
            return common.First().textDirection;
        }
        /*public static void sortWithDirectionality(List<_ReadingOrderSortData> list, TextDirection directionality) { 
            mergeSort<_ReadingOrderSortData>(list, compare: (_ReadingOrderSortData a, _ReadingOrderSortData b)=> { 
                switch (directionality) { 
                    case TextDirection.ltr: 
                        return a.rect.left.CompareTo(b.rect.left); 
                    case TextDirection.rtl: 
                        return b.rect.right.CompareTo(a.rect.right); 
                } 
                D.assert(false, ()=>"Unhandled directionality $directionality"); 
                return 0; 
            }); 
        }*/

        public IEnumerable<Directionality>  directionalAncestors { 
            get { 
                List<Directionality> getDirectionalityAncestors(BuildContext context) { 
                    List<Directionality> result = new List<Directionality>(); 
                    InheritedElement directionalityElement = context.getElementForInheritedWidgetOfExactType<Directionality>(); 
                    while (directionalityElement != null) { 
                        result.Add(directionalityElement.widget as Directionality); 
                        directionalityElement = FocusTravesalUtils._getAncestor(directionalityElement)?.getElementForInheritedWidgetOfExactType<Directionality>(); 
                    } 
                    return result; 
                }
                _directionalAncestors ??= getDirectionalityAncestors(node.context); 
                return _directionalAncestors; 
            } 
        }
        List<Directionality> _directionalAncestors;
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TextDirection>("directionality", directionality));
            properties.add(new StringProperty("name", node.debugLabel, defaultValue: null));
            properties.add(new DiagnosticsProperty<Rect>("rect", rect));
        }
    }

    public class _ReadingOrderDirectionalGroupData : Diagnosticable {
        public _ReadingOrderDirectionalGroupData(List<_ReadingOrderSortData> members) {
            this.members = members;
        }

        public readonly List<_ReadingOrderSortData> members;

        public TextDirection directionality {
            get {
                return members.First().directionality;
            }
        }

        Rect _rect; 
        Rect  rect {
            get {if (_rect == null) {
                    foreach(Rect rect in members.Select(
                        (_ReadingOrderSortData data) => data.rect)){
                        _rect ??= rect;
                        _rect = _rect.expandToInclude(rect);
                    }
                }
                return _rect; 
            }
        }
        List<Directionality>  memberAncestors {
            get { if (_memberAncestors == null) {
                    _memberAncestors = new List<Directionality>();
                    foreach (_ReadingOrderSortData member in members) {
                        _memberAncestors.AddRange(member.directionalAncestors);
                    }
                }
                return _memberAncestors; }

        }
        List<Directionality> _memberAncestors;
        /*public static void sortWithDirectionality(List<_ReadingOrderDirectionalGroupData> list, TextDirection directionality) {
            mergeSort<_ReadingOrderDirectionalGroupData>(list, compare: (_ReadingOrderDirectionalGroupData a, _ReadingOrderDirectionalGroupData b) =>{
                switch (directionality) {
                    case TextDirection.ltr:
                        return a.rect.left.CompareTo(b.rect.left);
                    case TextDirection.rtl:
                        return b.rect.right.CompareTo(a.rect.right);
                }
                D.assert(false, ()=>"Unhandled directionality $directionality");
                return 0;
            });
         }*/
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TextDirection>("directionality", directionality));
            properties.add(new DiagnosticsProperty<Rect>("rect", rect));
            //properties.add(new IterableProperty<string>("members", members.map<String>((_ReadingOrderSortData member) {
            //    return ""${member.node.debugLabel}"(${member.rect})";
            //})));
        }
    }
    /*public class ReadingOrderTraversalPolicy : FocusTraversalPolicy , DirectionalFocusTraversalPolicyMixin 
    { 
        public List<_ReadingOrderDirectionalGroupData> _collectDirectionalityGroups(IEnumerable<_ReadingOrderSortData> candidates) { 
            TextDirection currentDirection = candidates.First().directionality;
            List<_ReadingOrderSortData> currentGroup = new List<_ReadingOrderSortData>();
            List<_ReadingOrderDirectionalGroupData> result = new List<_ReadingOrderDirectionalGroupData>();
            foreach ( _ReadingOrderSortData candidate in candidates) { 
                if (candidate.directionality == currentDirection) { 
                    currentGroup.Add(candidate); 
                    continue; 
                } 
                currentDirection = candidate.directionality; 
                result.Add(new _ReadingOrderDirectionalGroupData(currentGroup)); 
                currentGroup = new List<_ReadingOrderSortData>(){candidate};
            } 
            if (currentGroup.isNotEmpty()) { 
                result.Add(new _ReadingOrderDirectionalGroupData(currentGroup));
            }
            
            foreach ( _ReadingOrderDirectionalGroupData bandGroup in result) { 
                if (bandGroup.members.Count == 1) { 
                    continue; 
                } 
               // _ReadingOrderSortData.sortWithDirectionality(bandGroup.members, bandGroup.directionality); 
            } 
            return result; 
        }*/
        /*public _ReadingOrderSortData _pickNext(List<_ReadingOrderSortData> candidates) {
            
            MERGESORT<_ReadingOrderSortData>(candidates, compare: (_ReadingOrderSortData a, _ReadingOrderSortData b) => a.rect.top.CompareTo(b.rect.top)); 
            _ReadingOrderSortData topmost = candidates.First();

            List<_ReadingOrderSortData> inBand(_ReadingOrderSortData current, IEnumerable<_ReadingOrderSortData> candidates) { 
                Rect band = Rect.fromLTRB(float.NegativeInfinity, current.rect.top, float.PositiveInfinity, current.rect.bottom);
                return candidates.Where((_ReadingOrderSortData item)=> {
                    return !item.rect.intersect(band).isEmpty;
                }).ToList();
            }
            List<_ReadingOrderSortData> inBandOfTop = inBand(topmost, candidates);
            D.assert(topmost.rect.isEmpty || inBandOfTop.isNotEmpty());
            if (inBandOfTop.Count <= 1) {
                return topmost;
            }
            TextDirection nearestCommonDirectionality = _ReadingOrderSortData.commonDirectionalityOf(inBandOfTop);
            _ReadingOrderSortData.sortWithDirectionality(inBandOfTop, nearestCommonDirectionality);
            List<_ReadingOrderDirectionalGroupData> bandGroups = _collectDirectionalityGroups(inBandOfTop); 
            if (bandGroups.Count == 1) {
                return bandGroups.First().members.First();
            }
            _ReadingOrderDirectionalGroupData.sortWithDirectionality(bandGroups, nearestCommonDirectionality);
            return bandGroups.First().members.First();
        }*/

        /*public override IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants) { 
            D.assert(descendants != null); 
            if (descendants.Count() <= 1) { 
                return descendants; 
            }
            List<_ReadingOrderSortData> data = new List<_ReadingOrderSortData>();
            foreach (FocusNode node in descendants)
                data.Add(new _ReadingOrderSortData(node));
            
            List<FocusNode> sortedList = new List<FocusNode>(); 
            List<_ReadingOrderSortData> unplaced = data;
            _ReadingOrderSortData current = _pickNext(unplaced); 
            sortedList.Add(current.node); 
            unplaced.Remove(current);
            while (unplaced.isNotEmpty()) {
                _ReadingOrderSortData next = _pickNext(unplaced);
                current = next;
                sortedList.Add(current.node);
                unplaced.Remove(current);
            }
            return sortedList;
        }*/
   // }


}