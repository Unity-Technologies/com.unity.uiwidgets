using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class FocusTraversalGroup : StatefulWidget {
        public FocusTraversalGroup(
            Key key = null,
            FocusTraversalPolicy policy = null,
            Widget child = null
        ) : base(key: key) {
            policy = policy ?? ReadingOrderTraversalPolicy();
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
            FocusTraversalPolicy defaultPolicy = scopeGroupMarker?.policy ?? ReadingOrderTraversalPolicy();
            Dictionary<FocusNode, _FocusTraversalGroupInfo> groups = new Dictionary<FocusNode, _FocusTraversalGroupInfo>();
            foreach(FocusNode node in scope.descendants) { 
                _FocusTraversalGroupMarker groupMarker = _getMarker(node.context);
                FocusNode groupNode = groupMarker?.focusNode;
                if (node == groupNode) {
                    BuildContext parentContext = _getAncestor(groupNode.context, count: 2); 
                    _FocusTraversalGroupMarker parentMarker = _getMarker(parentContext); 
                    FocusNode parentNode = parentMarker?.focusNode; 
                    groups[parentNode] ??= new _FocusTraversalGroupInfo(parentMarker, members: new List<FocusNode>(), defaultPolicy: defaultPolicy);
                    D.assert( !groups[parentNode].members.Contains(node) );
                    groups[parentNode].members.add(groupNode);
                    continue;
                }
                if (node.canRequestFocus && !node.skipTraversal) { 
                    groups[groupNode] ??= _FocusTraversalGroupInfo(groupMarker, members: new List<FocusNode>(), defaultPolicy: defaultPolicy); 
                    D.assert(!groups[groupNode].members.contains(node)); 
                    groups[groupNode].members.add(node);
                }
            }
            HashSet<FocusNode> groupKeys = groups.Keys.ToSet(); 
            foreach ( FocusNode key in groups.keys) { 
                List<FocusNode> sortedMembers = groups[key].policy.sortDescendants(groups[key].members).toList(); 
                groups[key].members.clear(); 
                groups[key].members.addAll(sortedMembers); 
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
                sortedDescendants.toSet().difference(scope.traversalDescendants.toSet()).isEmpty, 
                $"sorted descendants contains more nodes than it should: ({sortedDescendants.toSet().difference(scope.traversalDescendants.toSet())})"
                ); 
            D.assert(
                scope.traversalDescendants.toSet().difference(sortedDescendants.toSet()).isEmpty, 
                'sorted descendants are missing some nodes: (${scope.traversalDescendants.toSet().difference(sortedDescendants.toSet())})'
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
                    _focusAndEnsureVisible(
                        firstFocus, 
                        alignmentPolicy: forward ? ScrollPositionAlignmentPolicy.keepVisibleAtEnd : ScrollPositionAlignmentPolicy.keepVisibleAtStart,
                        ); 
                    return true; 
                } 
            } 
            List<FocusNode> sortedNodes = _sortAllDescendants(nearestScope); 
            if (forward && focusedChild == sortedNodes.Last()) { 
                _focusAndEnsureVisible(sortedNodes.First(), alignmentPolicy: ScrollPositionAlignmentPolicy.keepVisibleAtEnd); 
                return true; 
            } 
            if (!forward && focusedChild == sortedNodes.First()) { 
                _focusAndEnsureVisible(sortedNodes.Last(), alignmentPolicy: ScrollPositionAlignmentPolicy.keepVisibleAtStart); 
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
                    _focusAndEnsureVisible(
                        node, 
                        alignmentPolicy: forward ? ScrollPositionAlignmentPolicy.keepVisibleAtEnd : ScrollPositionAlignmentPolicy.keepVisibleAtStart,
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
    class ReadingOrderTraversalPolicy : FocusTraversalPolicy , DirectionalFocusTraversalPolicyMixin { 
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
                result.Add(_ReadingOrderDirectionalGroupData(currentGroup)); 
                currentGroup = new List<_ReadingOrderSortData>(){candidate};
            } 
            if (currentGroup.isNotEmpty()) { 
                result.Add(_ReadingOrderDirectionalGroupData(currentGroup));
            }
            
            foreach ( _ReadingOrderDirectionalGroupData bandGroup in result) { 
                if (bandGroup.members.length == 1) { 
                    continue; 
                } 
                _ReadingOrderSortData.sortWithDirectionality(bandGroup.members, bandGroup.directionality); 
            } 
            return result; 
        }
        public _ReadingOrderSortData _pickNext(List<_ReadingOrderSortData> candidates) {
            
            mergeSort<_ReadingOrderSortData>(candidates, compare: (_ReadingOrderSortData a, _ReadingOrderSortData b) => a.rect.top.compareTo(b.rect.top)); 
            _ReadingOrderSortData topmost = candidates.First();

            List<_ReadingOrderSortData> inBand(_ReadingOrderSortData current, IEnumerable<_ReadingOrderSortData> candidates) { 
                Rect band = Rect.fromLTRB(double.negativeInfinity, current.rect.top, double.infinity, current.rect.bottom);
                return candidates.Where((_ReadingOrderSortData item)=> {
                    return !item.rect.intersect(band).isEmpty;
                }).ToList();
            }
            List<_ReadingOrderSortData> inBandOfTop = inBand(topmost, candidates);
            D.assert(topmost.rect.isEmpty || inBandOfTop.isNotEmpty);
            if (inBandOfTop.Count <= 1) {
                return topmost;
            }
            TextDirection nearestCommonDirectionality = _ReadingOrderSortData.commonDirectionalityOf(inBandOfTop);
            _ReadingOrderSortData.sortWithDirectionality(inBandOfTop, nearestCommonDirectionality);
            List<_ReadingOrderDirectionalGroupData> bandGroups = _collectDirectionalityGroups(inBandOfTop);
    if (bandGroups.Count == 1) {
      // There's only one directionality group, so just send back the first
      // one in that group, since it's already sorted.
      return bandGroups.First().members.first;
    }

    // Sort the groups based on the common directionality and bounding boxes.
    _ReadingOrderDirectionalGroupData.sortWithDirectionality(bandGroups, nearestCommonDirectionality);
    return bandGroups.first.members.first;
  }

  // Sorts the list of nodes based on their geometry into the desired reading
  // order based on the directionality of the context for each node.
  @override
  IEnumerable<FocusNode> sortDescendants(IEnumerable<FocusNode> descendants) {
    assert(descendants != null);
    if (descendants.length <= 1) {
      return descendants;
    }

    final List<_ReadingOrderSortData> data = <_ReadingOrderSortData>[
      for (final FocusNode node in descendants) _ReadingOrderSortData(node),
    ];

    final List<FocusNode> sortedList = <FocusNode>[];
    final List<_ReadingOrderSortData> unplaced = data;

    // Pick the initial widget as the one that is at the beginning of the band
    // of the topmost, or the topmost, if there are no others in its band.
    _ReadingOrderSortData current = _pickNext(unplaced);
    sortedList.add(current.node);
    unplaced.remove(current);

    // Go through each node, picking the next one after eliminating the previous
    // one, since removing the previously picked node will expose a new band in
    // which to choose candidates.
    while (unplaced.isNotEmpty) {
      final _ReadingOrderSortData next = _pickNext(unplaced);
      current = next;
      sortedList.add(current.node);
      unplaced.remove(current);
    }
    return sortedList;
  }
}


}