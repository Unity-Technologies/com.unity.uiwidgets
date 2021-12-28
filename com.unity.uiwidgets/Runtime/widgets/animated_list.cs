using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public delegate Widget AnimatedListItemBuilder(BuildContext context, int index, Animation<float> animation);

   
    public delegate Widget AnimatedListRemovedItemBuilder(BuildContext context, Animation<float> animation);

    public class AnimatedListUtils {
        public  static int binarySearch<T>(List<T> sortedList, T value) where T : _ActiveItem  {
            int min = 0;
            int max = sortedList.Count;
            while (min < max) {
                int mid = min + ((max - min) >> 1);
                T element = sortedList[mid];
                int comp = element.CompareTo(value);
                if (comp == 0) {
                    return mid;
                }
                if (comp < 0) {
                    min = mid + 1;
                } else {
                    max = mid;
                }
            }
            return -1;
        }
    }

    public class _ActiveItem  : IComparable {
        
        public _ActiveItem(
            AnimationController controller,
            int itemIndex) {
            this.controller = controller;
            this.itemIndex = itemIndex;
            removedItemBuilder = null;
        }
        public _ActiveItem(
            AnimationController controller,
            int itemIndex,
            AnimatedListRemovedItemBuilder removedItemBuilder
            ) {
            this.controller = controller;
            this.itemIndex = itemIndex;
            this.removedItemBuilder = removedItemBuilder;
        }
        public _ActiveItem(
            int itemIndex) {
            controller = null;
            this.itemIndex = itemIndex;
            removedItemBuilder = null;
        }

        public readonly AnimationController controller;
        public readonly AnimatedListRemovedItemBuilder removedItemBuilder;
        public int itemIndex;


        public int CompareTo(object other) {
            D.assert(GetType() == other.GetType());
            return itemIndex - ((_ActiveItem)other).itemIndex;
        }
    }
    public class AnimatedList : StatefulWidget {
        public AnimatedList(
            Key key = null,
            AnimatedListItemBuilder itemBuilder = null,
            int initialItemCount = 0,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollController controller = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            EdgeInsetsGeometry padding = null) : base(key: key) { 
            D.assert(itemBuilder != null);
            D.assert(initialItemCount >= 0);
            this.itemBuilder = itemBuilder;
            this.initialItemCount = initialItemCount;
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.controller = controller;
            this.primary = primary;
            this.physics = physics;
            this.shrinkWrap = shrinkWrap;
            this.padding = padding;
        }

        public readonly AnimatedListItemBuilder itemBuilder;
        public readonly int initialItemCount;
        public readonly Axis scrollDirection;
        public readonly bool reverse;
        public readonly ScrollController controller;
        public readonly bool? primary;
        public readonly ScrollPhysics physics;
        public readonly bool shrinkWrap;
        public readonly EdgeInsetsGeometry padding;

        public static AnimatedListState of(BuildContext context,  bool nullOk = false ) { 
            D.assert(context != null);
            AnimatedListState result = context.findAncestorStateOfType<AnimatedListState>();
            if (nullOk || result != null)
                return result; 
            throw new UIWidgetsError(new List<DiagnosticsNode>{
                new ErrorSummary("AnimatedList.of() called with a context that does not contain an AnimatedList."),
                new ErrorDescription("No AnimatedList ancestor could be found starting from the context that was passed to AnimatedList.of()."), 
                new ErrorHint(
                    "This can happen when the context provided is from the same StatefulWidget that " +
                    "built the AnimatedList. Please see the AnimatedList documentation for examples " +
                    "of how to refer to an AnimatedListState object:" +
                    "  https://api.flutter.dev/flutter/widgets/AnimatedListState-class.html"
                ), 
                    context.describeElement("The context used was")
                    
            }); 
        }

        public override State createState() {
            return new AnimatedListState();
        }
    }
    public class AnimatedListState : TickerProviderStateMixin< AnimatedList> { 
        TimeSpan _kDuration = new TimeSpan(0,0,0,0,300);

        public readonly GlobalKey<SliverAnimatedListState> _sliverAnimatedListKey =
            GlobalKey<SliverAnimatedListState>.key();
        
        public void insertItem(int index,  TimeSpan? duration = null ) {
            duration = duration ?? _kDuration;
            _sliverAnimatedListKey.currentState.insertItem(index, duration: duration);
         }

        public void removeItem(int index, AnimatedListRemovedItemBuilder builder, TimeSpan? duration = null ) { 
            duration = duration ?? _kDuration; 
            _sliverAnimatedListKey.currentState.removeItem(index, builder, duration: duration);
        }
        public override Widget build(BuildContext context) { 
            return new CustomScrollView(
                scrollDirection: widget.scrollDirection,
                reverse: widget.reverse,
                controller: widget.controller,
                primary: widget.primary,
                physics: widget.physics,
                shrinkWrap: widget.shrinkWrap, 
                slivers: new List<Widget>{
                    new SliverPadding(
                        padding: widget.padding ??  EdgeInsets.all(0), 
                        sliver: new SliverAnimatedList(
                            key: _sliverAnimatedListKey, 
                            itemBuilder: widget.itemBuilder, 
                            initialItemCount: widget.initialItemCount
                            )
                        )
                }
            ); 
        } 
    }
    public class SliverAnimatedList : StatefulWidget {
        public SliverAnimatedList(
            Key key = null,
            AnimatedListItemBuilder itemBuilder = null,
            int initialItemCount = 0
        ) : base(key: key) {
            D.assert(itemBuilder != null);
            D.assert(initialItemCount >= 0);
            this.itemBuilder = itemBuilder;
            this.initialItemCount = initialItemCount;
        }


        public readonly AnimatedListItemBuilder itemBuilder;

        public readonly int initialItemCount;

        public override State createState() {
            return new SliverAnimatedListState();
        }


        public static SliverAnimatedListState of(BuildContext context, bool nullOk = false) { 
            D.assert(context != null);
            SliverAnimatedListState result = context.findAncestorStateOfType<SliverAnimatedListState>();
            if (nullOk || result != null)
                return result;
            throw new UIWidgetsError(
                "SliverAnimatedList.of() called with a context that does not contain a SliverAnimatedList.\n" + 
                " No SliverAnimatedListState ancestor could be found starting from the " + 
                "context that was passed to SliverAnimatedListState.of(). This can " + 
                "happen when the context provided is from the same StatefulWidget that " + 
                "built the AnimatedList. Please see the SliverAnimatedList documentation " +
                "for examples of how to refer to an AnimatedListState object: " +
                "https://docs.flutter.io/flutter/widgets/SliverAnimatedListState-class.html \n" + 
                "The context used was:\n" +
                $"  {context}");
        }
    }
    public class SliverAnimatedListState : TickerProviderStateMixin<SliverAnimatedList> {
        public readonly List<_ActiveItem> _incomingItems = new List<_ActiveItem>();
        public readonly List<_ActiveItem> _outgoingItems = new List<_ActiveItem>(); 
        TimeSpan _kDuration = new TimeSpan(0,0,0,0,300);
        int _itemsCount = 0;
        public override void initState() { 
            base.initState(); 
            _itemsCount = widget.initialItemCount;
        }
        public override void dispose() {
            List<_ActiveItem> Items = new List<_ActiveItem>();
            foreach (var item in _incomingItems) {
                Items.Add(item);
            }
            foreach (var item in _outgoingItems) {
                Items.Add(item);
            }
            foreach ( _ActiveItem item in Items) {
              item.controller.dispose();
            }
            base.dispose();
        }
        
        public _ActiveItem _removeActiveItemAt(List<_ActiveItem> items, int itemIndex) { 
            int i = AnimatedListUtils.binarySearch(items, new _ActiveItem(itemIndex));
            _ActiveItem item = null;
            if (i != -1) {
                item = items[i];
                items.RemoveAt(i);
            } 
            return i == -1 ? null : item;
        }
        public _ActiveItem _activeItemAt(List<_ActiveItem> items, int itemIndex) {
            int i = AnimatedListUtils.binarySearch(items, new _ActiveItem(itemIndex));
            return i == -1 ? null : items[i];
        }
        public int _indexToItemIndex(int index) { 
            int itemIndex = index; 
            foreach ( _ActiveItem item in _outgoingItems) { 
                if (item.itemIndex <= itemIndex) 
                    itemIndex += 1;
                else 
                    break;
            }
            return itemIndex;
        }
        public int _itemIndexToIndex(int itemIndex) {
            int index = itemIndex;
            foreach ( _ActiveItem item in _outgoingItems) {
             D.assert(item.itemIndex != itemIndex);
              if (item.itemIndex < itemIndex)
                index -= 1;
              else
                break;
            }
            return index;
        }
        public SliverChildDelegate _createDelegate() { 
            return new SliverChildBuilderDelegate(_itemBuilder, childCount: _itemsCount);
        }
        public void insertItem(int index, TimeSpan? duration = null) { 
            duration = duration ?? _kDuration; 
            D.assert(index >= 0);
            int itemIndex = _indexToItemIndex(index);
            D.assert(itemIndex >= 0 && itemIndex <= _itemsCount);

            foreach ( _ActiveItem item in _incomingItems) {
              if (item.itemIndex >= itemIndex)
                item.itemIndex += 1;
            }
            foreach ( _ActiveItem item in _outgoingItems) {
              if (item.itemIndex >= itemIndex)
                item.itemIndex += 1;
            }
            AnimationController controller = new AnimationController(
                duration: duration, 
                vsync: this
            ); 
            _ActiveItem incomingItem = new _ActiveItem(
                controller,
                itemIndex
            );
            setState(()=> {
                _incomingItems.Add(incomingItem); 
                _incomingItems.Sort();
                _itemsCount += 1;
            });
            controller.forward().then((_)=> { 
                _removeActiveItemAt(_incomingItems, incomingItem.itemIndex).controller.dispose(); 
            }); 
        }
        public void removeItem(int index, AnimatedListRemovedItemBuilder builder, TimeSpan? duration = null) { 
            duration = duration ?? _kDuration; 
            D.assert(index >= 0);
            D.assert(builder != null);
            D.assert(duration != null);
            int itemIndex = _indexToItemIndex(index);
            D.assert(itemIndex >= 0 && itemIndex < _itemsCount);
            D.assert(_activeItemAt(_outgoingItems, itemIndex) == null);
            _ActiveItem incomingItem = _removeActiveItemAt(_incomingItems, itemIndex); 
            AnimationController controller = incomingItem?.controller 
                                             ?? new AnimationController(duration: duration, value: 1.0f, vsync: this); 
            _ActiveItem outgoingItem = new _ActiveItem(controller, itemIndex, builder); 
            setState(()=> { 
                _outgoingItems.Add(outgoingItem); 
                _outgoingItems.Sort(); 
            });
            
            controller.reverse().then(( _ )=> { 
                _removeActiveItemAt(_outgoingItems, outgoingItem.itemIndex).controller.dispose();
                foreach ( _ActiveItem item in _incomingItems) {
                    if (item.itemIndex > outgoingItem.itemIndex)
                        item.itemIndex -= 1;
                }
                foreach ( _ActiveItem item in _outgoingItems) {
                    if (item.itemIndex > outgoingItem.itemIndex)
                        item.itemIndex -= 1;
                }

                setState(() => _itemsCount -= 1);
            });
        }
        public Widget _itemBuilder(BuildContext context, int itemIndex) { 
            _ActiveItem outgoingItem = _activeItemAt(_outgoingItems, itemIndex);
            if (outgoingItem != null) { 
                return outgoingItem.removedItemBuilder(
                    context, 
                    outgoingItem.controller.view
                );
            }
            _ActiveItem incomingItem = _activeItemAt(_incomingItems, itemIndex);
            Animation<float> animation = incomingItem?.controller?.view ?? Animations.kAlwaysCompleteAnimation;
            return widget.itemBuilder(
                context, 
                _itemIndexToIndex(itemIndex), 
                animation
            );
        }
        public override Widget build(BuildContext context) { 
            return new SliverList(
                del: _createDelegate()
                );
            }
        }


    }