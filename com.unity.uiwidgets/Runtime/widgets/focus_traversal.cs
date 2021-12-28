using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
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

        static int _MERGE_SORT_LIMIT = 32;
        /// as they started in.
        public static void insertionSort<T>(List<T> list,
            Comparator<T> compare = null, int start = 0, int end =0) {
            
            //compare = compare == null ?  defaultCompare<T>();
            end = end == 0 ?list.Count : end;

            for (int pos = start + 1; pos < end; pos++) {
                int min = start;
                int max = pos;
                var element = list[pos];
                while (min < max) {
                    int mid = min + ((max - min) >> 1);
                    int comparison = compare(element, list[mid]);
                    if (comparison < 0) {
                        max = mid;
                    } else {
                        min = mid + 1;
                    }
                }
                setRange(list,min + 1, pos + 1, list, min);
                list[min] = element;
            }
        }

        public delegate int Comparator<T>(T a, T b);

        static int defaultCompare<T>(T value1, T value2) {
            return (value1 as IComparable).CompareTo(value2);
        }
        
        public static void mergeSort<T>(
            List<T> list,
        int? start = null , int? end = null, Comparator<T> compare = null) {
            var _start = start ?? 0;
            var _end = end ?? list.Count;
            compare = compare ?? defaultCompare;

            int length = _end - _start;
            if (length < 2) return;
            if (length < _MERGE_SORT_LIMIT) {
                insertionSort(list, compare: compare, start: _start, end: _end);
                return;
            }
           
            int middle = _start + ((_end - _start) >> 1);
            int firstLength = middle - _start;
            int secondLength = _end - middle;
            // secondLength is always the same as firstLength, or one greater.
            var scratchSpace = new List<T>(secondLength);
            _mergeSort(list, compare, middle, _end, scratchSpace, 0);
            int firstTarget = _end - firstLength;
            _mergeSort(list, compare, _start, middle, list, firstTarget);
            _merge(compare, list, firstTarget, _end, scratchSpace, 0, secondLength, list,
                _start);
        }

        public static void _mergeSort<T>(List<T> list, Comparator<T> compare, int start, int end,
            List<T> target, int targetOffset) {
            int length = end - start;
            if (length < _MERGE_SORT_LIMIT) {
                _movingInsertionSort(list, compare, start, end, target, targetOffset);
                return;
            }
            int middle = start + (length >> 1);
            int firstLength = middle - start;
            int secondLength = end - middle;
            
            int targetMiddle = targetOffset + firstLength;
           
            _mergeSort(list, compare, middle, end, target, targetMiddle);
          
            _mergeSort(list, compare, start, middle, list, middle);
            
            _merge(compare, list, middle, middle + firstLength, target, targetMiddle,
                targetMiddle + secondLength, target, targetOffset);
        }
        public static void _movingInsertionSort<T>(List<T> list, Comparator<T> compare, int start,
        int end, List<T> target, int targetOffset) {
            int length = end - start;
            if (length == 0) return;
            target[targetOffset] = list[start];
            for (int i = 1; i < length; i++) {
                var element = list[start + i];
                int min = targetOffset;
                int max = targetOffset + i;
                while (min < max) {
                    int mid = min + ((max - min) >> 1);
                    if (compare(element, target[mid]) < 0) {
                        max = mid;
                    } else {
                        min = mid + 1;
                    }
                }
                setRange(target,min + 1, targetOffset + i + 1, target, min);
                target[min] = element;
            }
        }



        public static List<T>setRange<T>(List<T> alist, int start, int end, List<T> blist, int skipConut = 0 ) {
            List<T> copyList = new List<T>();
            List<T> resultList = new List<T>();
            for (int i = skipConut; i < blist.Count; i++) {
                copyList.Add(blist[i]);
            }

            for (int i = 0; i < start; i++) {
                resultList.Add(alist[i]);
            }

            for (int i = 0; i <  copyList.Count; i++) {
                resultList.Add(blist[i]);
            }

            for (int i = start + copyList.Count - 1; i < alist.Count; i++) {
                resultList.Add(alist[i]);
            }

            return resultList;
        }

        public  static void _merge<T>(
            Comparator<T> compare,
        List<T> firstList,
        int firstStart,
        int firstEnd,
            List<T> secondList,
        int secondStart,
        int secondEnd, 
            List<T> target,
        int targetOffset) {
            // No empty lists reaches here.
            D.assert(firstStart < firstEnd);
            D.assert(secondStart < secondEnd);
            int cursor1 = firstStart;
            int cursor2 = secondStart;
            var firstElement = firstList[cursor1++];
            var secondElement = secondList[cursor2++];
            while (true) {
                if (compare(firstElement, secondElement) <= 0) {
                    target[targetOffset++] = firstElement;
                    if (cursor1 == firstEnd) break; // Flushing second list after loop.
                    firstElement = firstList[cursor1++];
                } else {
                    target[targetOffset++] = secondElement;
                    if (cursor2 != secondEnd) {
                        secondElement = secondList[cursor2++];
                        continue;
                    }
                    
                    target[targetOffset++] = firstElement;
                    setRange(target,targetOffset, targetOffset + (firstEnd - cursor1),
                        firstList, cursor1);
                    return;
                }
            }
            // First list empties first. Reached by break above.
            target[targetOffset++] = secondElement;
            setRange(target
                ,targetOffset, targetOffset + (secondEnd - cursor2), secondList, cursor2);
        }



    }
    

    public class FocusTraversalGroup : StatefulWidget {
        public FocusTraversalGroup(
            Key key = null,
            FocusTraversalPolicy policy = null,
            Widget child = null
        ) : base(key: key) {
            this.policy = policy ?? new ReadingOrderTraversalPolicy();
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
                policy = marker?.policy ?? defaultPolicy ?? new ReadingOrderTraversalPolicy();
                this.members = members ?? new List<FocusNode>();
            }
            public readonly FocusNode groupNode;
            public readonly FocusTraversalPolicy policy;
            public readonly List<FocusNode> members;
        }

        public virtual FocusNode findFirstFocus(FocusNode currentNode) {
            D.assert(currentNode != null);
            FocusScopeNode scope = currentNode.nearestScope;
            FocusNode candidate = scope.focusedChild;
            if (candidate == null && scope.descendants.Any()) {
                IEnumerable<FocusNode> sorted = _sortAllDescendants(scope);
                candidate = sorted.Any() ? sorted.First() : null;
            }

            candidate = candidate ?? currentNode;
            return candidate;
        }
        
        public abstract FocusNode findFirstFocusInDirection(FocusNode currentNode, TraversalDirection? direction);

        public virtual void invalidateScopeData(FocusScopeNode node) {
        }

        public virtual void changedScope(FocusNode node = null, FocusScopeNode oldScope = null) {
        }

        public bool next(FocusNode currentNode) => _moveFocus(currentNode, forward: true);

        public bool previous(FocusNode currentNode) => _moveFocus(currentNode, forward: false);

        public abstract bool inDirection(FocusNode currentNode, TraversalDirection direction);

        public abstract IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants);
        public _FocusTraversalGroupMarker _getMarker(BuildContext context) {
            return context?.getElementForInheritedWidgetOfExactType<_FocusTraversalGroupMarker>()?.widget as _FocusTraversalGroupMarker;
        }
        public List<FocusNode> _sortAllDescendants(FocusScopeNode scope) { 
            D.assert(scope != null); 
            _FocusTraversalGroupMarker scopeGroupMarker = _getMarker(scope.context);
            FocusTraversalPolicy defaultPolicy = scopeGroupMarker?.policy ?? new ReadingOrderTraversalPolicy();
            Dictionary<FocusNode, _FocusTraversalGroupInfo> groups = new Dictionary<FocusNode, _FocusTraversalGroupInfo>();
            foreach(FocusNode node in scope.descendants) { 
                _FocusTraversalGroupMarker groupMarker = _getMarker(node.context);
                FocusNode groupNode = groupMarker?.focusNode;
                if (node == groupNode) {
                    BuildContext parentContext =FocusTravesalUtils._getAncestor(groupNode.context, count: 2); 
                    _FocusTraversalGroupMarker parentMarker = _getMarker(parentContext); 
                    FocusNode parentNode = parentMarker?.focusNode;
                    groups[groupNode] =  groups.getOrDefault(parentNode) ?? new _FocusTraversalGroupInfo(parentMarker, members: new List<FocusNode>(), defaultPolicy: defaultPolicy);
                    D.assert( !groups[parentNode].members.Contains(node) );
                    groups[parentNode].members.Add(groupNode);
                    continue;
                }
                if (node.canRequestFocus && !node.skipTraversal) { 
                    groups[groupNode] = groups.getOrDefault(groupNode) ?? new _FocusTraversalGroupInfo(groupMarker, members: new List<FocusNode>(), defaultPolicy: defaultPolicy); 
                    D.assert(!groups[groupNode].members.Contains(node)); 
                    groups[groupNode].members.Add(node);
                }
            }
            HashSet<FocusNode> groupKeys = new HashSet<FocusNode>(groups.Keys);
            foreach ( FocusNode key in groups.Keys) { 
                List<FocusNode> sortedMembers = groups.getOrDefault(key).policy.sortDescendants(groups.getOrDefault(key).members).ToList(); 
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
    
    public class WidgetOrderTraversalPolicy : DirectionalFocusTraversalPolicyMixinFocusTraversalPolicy {
        public override IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants) {
            return descendants;
        }
    }

    public class _DirectionalPolicyData {
        public _DirectionalPolicyData(List<_DirectionalPolicyDataEntry> history) {
            D.assert(history != null);
            this.history = history;
        }


        public readonly List<_DirectionalPolicyDataEntry> history;
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
            IEnumerable<HashSet<Directionality>> allAncestors = LinqUtils<HashSet<Directionality>, _ReadingOrderSortData>.SelectList(list, ((_ReadingOrderSortData member) => new HashSet<Directionality>(member.directionalAncestors)));
            HashSet<Directionality> common = null; 
            foreach ( HashSet<Directionality> ancestorSet in allAncestors) { 
                common = common ?? ancestorSet; 
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
        public static void sortWithDirectionality(List<_ReadingOrderSortData> list, TextDirection directionality) { 
            FocusTravesalUtils.mergeSort<_ReadingOrderSortData>(list, 
                compare: (_ReadingOrderSortData a, _ReadingOrderSortData b)=> { 
                switch (directionality) { 
                    case TextDirection.ltr: 
                        return a.rect.left.CompareTo(b.rect.left); 
                    case TextDirection.rtl: 
                        return b.rect.right.CompareTo(a.rect.right); 
                } 
                D.assert(false, ()=>"Unhandled directionality $directionality"); 
                return 0; 
            }); 
        }

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
                _directionalAncestors = _directionalAncestors ?? getDirectionalityAncestors(node.context); 
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
                    foreach(Rect rect in LinqUtils<Rect,_ReadingOrderSortData>.SelectList(members,
                        (_ReadingOrderSortData data) => data.rect)){
                        _rect = _rect ?? rect;
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
        public static void sortWithDirectionality(List<_ReadingOrderDirectionalGroupData> list, TextDirection directionality) {
            FocusTravesalUtils.mergeSort<_ReadingOrderDirectionalGroupData>(list, compare: (_ReadingOrderDirectionalGroupData a, _ReadingOrderDirectionalGroupData b) =>{
                switch (directionality) {
                    case TextDirection.ltr:
                        return a.rect.left.CompareTo(b.rect.left);
                    case TextDirection.rtl:
                        return b.rect.right.CompareTo(a.rect.right);
                }
                D.assert(false, ()=>"Unhandled directionality $directionality");
                return 0;
            });
         }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TextDirection>("directionality", directionality));
            properties.add(new DiagnosticsProperty<Rect>("rect", rect));
            //properties.add(new IterableProperty<string>("members", members.map<String>((_ReadingOrderSortData member) {
            //    return ""${member.node.debugLabel}"(${member.rect})";
            //})));
        }
    }
    

    public interface DirectionalFocusTraversalPolicyMixin {
        void invalidateScopeData(FocusScopeNode node);
        void changedScope(FocusNode node = null, FocusScopeNode oldScope = null);
        FocusNode _sortAndFindInitial(FocusNode currentNode, bool vertical = false, bool first = false);

        IEnumerable<FocusNode> _sortAndFilterHorizontally(
            TraversalDirection direction,
            Rect target,
            FocusNode nearestScope);

        IEnumerable<FocusNode> _sortAndFilterVertically(
            TraversalDirection direction,
            Rect target,
            IEnumerable<FocusNode> nodes);

        bool _popPolicyDataIfNeeded(TraversalDirection direction, FocusScopeNode nearestScope, FocusNode focusedChild);
        void _pushPolicyData(TraversalDirection direction, FocusScopeNode nearestScope, FocusNode focusedChild);

        bool inDirection(FocusNode currentNode, TraversalDirection direction);


    }

    public class ReadingOrderTraversalPolicy : DirectionalFocusTraversalPolicyMixinFocusTraversalPolicy 
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
                _ReadingOrderSortData.sortWithDirectionality(bandGroup.members, bandGroup.directionality); 
            } 
            return result; 
        }
        public _ReadingOrderSortData _pickNext(List<_ReadingOrderSortData> candidates) {
            
            FocusTravesalUtils.mergeSort<_ReadingOrderSortData>(candidates, compare: (_ReadingOrderSortData a, _ReadingOrderSortData b) => a.rect.top.CompareTo(b.rect.top)); 
            _ReadingOrderSortData topmost = candidates.First();

            List<_ReadingOrderSortData> inBand(_ReadingOrderSortData current, IEnumerable<_ReadingOrderSortData> _candidates) { 
                Rect band = Rect.fromLTRB(float.NegativeInfinity, current.rect.top, float.PositiveInfinity, current.rect.bottom);
                return LinqUtils<_ReadingOrderSortData>.WhereList(_candidates,((_ReadingOrderSortData item)=> {
                    return !item.rect.intersect(band).isEmpty;
                }));
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
        }

        public override IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants) { 
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
        }
    }

    public abstract class FocusOrder : Diagnosticable , IComparable {
        public FocusOrder() {
        }

        public int CompareTo(object other) {
            D.assert(
                GetType() == other.GetType(),()=>
                "The sorting algorithm must not compare incomparable keys, since they don't "+
            $"know how to order themselves relative to each other. Comparing {this} with {other}");
            return doCompare((FocusOrder) other);
        }

        protected abstract int doCompare(FocusOrder other);
    }
    public class NumericFocusOrder : FocusOrder {
        public NumericFocusOrder(float order) {
            this.order = order;
        }

        public readonly float order;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("order", order));
        }

        protected override int doCompare(FocusOrder other) {
            other = (NumericFocusOrder) other;
            return order.CompareTo(((NumericFocusOrder) other).order);
        }
    }
    class LexicalFocusOrder : FocusOrder {
        public LexicalFocusOrder(string order) {
            D.assert(order != null);
            this.order = order;
        }


        public readonly string order;


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("order", order));
        }

        protected override int doCompare(FocusOrder other) {
            other = (LexicalFocusOrder) other;
            
            return order.CompareTo(((LexicalFocusOrder) other).order);
        }
    }
    class _OrderedFocusInfo {
        public _OrderedFocusInfo(
            FocusNode node = null,
            FocusOrder order = null) {
            D.assert(node != null);
            D.assert(order != null);
            this.order = order;
            this.node = node;
        }

        public readonly FocusNode node;
        public readonly FocusOrder order;
    }
    public class OrderedTraversalPolicy : DirectionalFocusTraversalPolicyMixinFocusTraversalPolicy {

        public OrderedTraversalPolicy(FocusTraversalPolicy secondary) {
            this.secondary = secondary;
        }

        public readonly FocusTraversalPolicy secondary;

        public override IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants) {
            FocusTraversalPolicy secondaryPolicy = secondary ?? new ReadingOrderTraversalPolicy();
            IEnumerable<FocusNode> sortedDescendants = secondaryPolicy.sortDescendants(descendants);
            List<FocusNode> unordered = new List<FocusNode>();
            List<_OrderedFocusInfo> ordered = new List<_OrderedFocusInfo>();
            foreach( FocusNode node in sortedDescendants) { 
                FocusOrder order = FocusTraversalOrder.of(node.context, nullOk: true); 
                if (order != null) {
                    ordered.Add(new _OrderedFocusInfo(node: node, order: order));
                } else {
                    unordered.Add(node);
                }
            }
            FocusTravesalUtils.mergeSort<_OrderedFocusInfo>(ordered, compare: (_OrderedFocusInfo a, _OrderedFocusInfo b)=> { 
                D.assert(
                a.order.GetType() == b.order.GetType(),()=>
                $"When sorting nodes for determining focus order, the order ({a.order}) of " +
                $"node {a.node}, isn't the same type as the order ({b.order}) of {b.node}. " +
                "Incompatible order types can't be compared.  Use a FocusTraversalGroup to group " +
                "similar orders together."
              ); 
                return a.order.CompareTo(b.order); 
            }); 
           return LinqUtils<FocusNode,_OrderedFocusInfo>.SelectList(ordered,((_OrderedFocusInfo info) => info.node)).Concat(unordered);
        }
    }

    public class FocusTraversalOrder : InheritedWidget {
        public FocusTraversalOrder(Key key = null, FocusOrder order = null, Widget child = null)
            : base(key: key, child: child) {
            this.order = order;
        }

        public readonly FocusOrder order;

        public static FocusOrder of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            FocusTraversalOrder marker = context.getElementForInheritedWidgetOfExactType<FocusTraversalOrder>()?.widget as FocusTraversalOrder; 
            FocusOrder order = marker?.order;
            if (order == null && !nullOk) {
                throw new UIWidgetsError("FocusTraversalOrder.of() was called with a context that "+
                "does not contain a TraversalOrder widget. No TraversalOrder widget " + 
                "ancestor could be found starting from the context that was passed to " + 
                "FocusTraversalOrder.of().\n" + 
                "The context used was:\n" + 
                $"  {context}");
            }
            return order;
        }
        public override bool updateShouldNotify(InheritedWidget oldWidget) => false;
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<FocusOrder>("order", order));
        }
    }
    
    public class _RequestFocusActionBase : UiWidgetAction {
        public _RequestFocusActionBase(LocalKey name) : base(name) {
        }

        FocusNode _previousFocus;
        public override void invoke(FocusNode node, Intent intent) {
            _previousFocus = FocusManagerUtils.primaryFocus;
            node.requestFocus();
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<FocusNode>("previous", _previousFocus));
        }
    }
    
    public class RequestFocusAction : _RequestFocusActionBase {
        /// Creates a [RequestFocusAction] with a fixed [key].
        public RequestFocusAction() : base(key) {
        }

        public static readonly LocalKey key = new ValueKey<Type>(typeof(RequestFocusAction));
    
        public override void invoke(FocusNode node, Intent intent) => FocusTravesalUtils._focusAndEnsureVisible(node);
    }
    
    public class NextFocusAction : _RequestFocusActionBase {
        public NextFocusAction() : base(key) {
            
        }

        public readonly static LocalKey key =  new ValueKey<Type>(typeof(NextFocusAction));

        public override void invoke(FocusNode node, Intent intent) {
            node.nextFocus();
        }
    }
    public class PreviousFocusAction : _RequestFocusActionBase {
        public PreviousFocusAction() : base(key) {
        }
        public readonly static LocalKey key = new ValueKey<Type>(typeof(PreviousFocusAction));
        public override void invoke(FocusNode node, Intent intent) => node.previousFocus();
    }
    
    public class DirectionalFocusIntent : Intent {
        public DirectionalFocusIntent(TraversalDirection direction = TraversalDirection.up, bool ignoreTextFields = true) 
        :base(DirectionalFocusAction.key) {
            this.ignoreTextFields = ignoreTextFields;
            this.direction = direction;
        }
        public readonly TraversalDirection direction;

        public readonly bool ignoreTextFields;
    }

   
    public class DirectionalFocusAction : _RequestFocusActionBase {
        public DirectionalFocusAction() : base(key) {
        }

        public readonly static LocalKey key = new ValueKey<Type>(typeof(DirectionalFocusAction));
        public override void invoke(FocusNode node, Intent intent) {
            intent = (DirectionalFocusIntent) intent;
            if (!((DirectionalFocusIntent)intent).ignoreTextFields || !(node.context.widget is EditableText)) {
                node.focusInDirection(((DirectionalFocusIntent) intent).direction);
            }
        }
    }

}