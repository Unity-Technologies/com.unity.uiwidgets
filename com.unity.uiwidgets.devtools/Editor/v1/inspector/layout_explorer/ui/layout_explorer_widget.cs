/*using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{

  public delegate Future OnSelectionChangedCallback();
  
  public class LayoutExplorerWidgetUtils
  {
      const float maxRequestsPerSecond = 3.0f;
  }
  
  public abstract class LayoutExplorerWidget : StatefulWidget {
    public LayoutExplorerWidget(
      InspectorController inspectorController,
      Key key
    ) : base(key: key)
    {
      this.inspectorController = inspectorController;
    }

  public readonly InspectorController inspectorController;
}


  public abstract class LayoutExplorerWidgetState<W, L> : 
    InspectorServiceClient where W : LayoutExplorerWidget where L : LayoutProperties // TickerProviderStateMixin, InspectorServiceClient where W : LayoutExplorerWidget where L : LayoutProperties
  {
    public LayoutExplorerWidgetState() {
      _onSelectionChangedCallback = onSelectionChanged;
    }

  public AnimationController entranceController;
  public CurvedAnimation entranceCurve;
  public AnimationController changeController;

  public CurvedAnimation changeAnimation;

  L _previousProperties;

  L _properties;

  InspectorObjectGroupManager objectGroupManager;

  public AnimatedLayoutProperties<L> animatedProperties
  {
    get
    {
      return _animatedProperties;
    }
  }

  AnimatedLayoutProperties<L> _animatedProperties;

  public L properties
  {
    get
    {
      return _previousProperties ?? _animatedProperties ?? _properties;
    }
  }

  public RemoteDiagnosticsNode selectedNode
  {
    get
    {
      return null; //inspectorController?.selectedNode?.value?.diagnostic;
    }
  }

  public InspectorController inspectorController
  {
    get
    {
      return null; //widget.inspectorController;
    }
  }

  public InspectorService inspectorService
  {
    get
    {
      return inspectorController?.inspectorService;
    }
  }

  //RateLimiter rateLimiter;

  public OnSelectionChangedCallback _onSelectionChangedCallback;

  Future onSelectionChanged() {
    if (!mounted) return async2.Future.value(); //if (!mounted) return async2.Future.value();
    if (!shouldDisplay(selectedNode)) {
      return async2.Future.value();
    }
    var prevRootId = id(_properties?.node);
    var newRootId = id(getRoot(selectedNode));
    var shouldFetch = prevRootId != newRootId;
    if (shouldFetch) {
      _dirty = false;
      fetchLayoutProperties().then((newSelection) =>
      {
        _setProperties(newSelection);
      });
    } else {
      updateHighlighted(_properties);
    }
    return async2.Future.value();
  }

  /// Whether this layout explorer can work with this kind of node.
  public abstract bool shouldDisplay(RemoteDiagnosticsNode node);

  public Size size
  {
    get
    {
      return properties.size;
    }
  }

  public List<LayoutProperties> children
  {
    get
    {
      return properties.displayChildren;
    }
  }

  public LayoutProperties highlighted;

  /// Returns the root widget to show.
  ///
  /// For cases such as Flex widgets or in the future ListView widgets we may
  /// want to show the layout for all widgets under a root that is the parent
  /// of the current widget.
  public abstract RemoteDiagnosticsNode getRoot(RemoteDiagnosticsNode node);
  

  Future<L> fetchLayoutProperties() {
    objectGroupManager?.cancelNext();
    var nextObjectGroup = objectGroupManager.next;
    var res = nextObjectGroup.getLayoutExplorerNode(
      getRoot(selectedNode)
    ).then((node)=>{computeLayoutProperties(node);});
    if (!nextObjectGroup.disposed) {
      D.assert(objectGroupManager.next == nextObjectGroup);
      objectGroupManager.promoteNext();
    }

    return res;
  }

  public abstract L computeLayoutProperties(RemoteDiagnosticsNode node);

  public abstract AnimatedLayoutProperties<L> computeAnimatedProperties(L nextProperties);
  public abstract void updateHighlighted(L newProperties);

  string id(RemoteDiagnosticsNode node) => node?.dartDiagnosticRef?.id;

  void _registerInspectorControllerService() {
    inspectorController?.selectedNode?.addListener(_onSelectionChangedCallback);
    inspectorService?.addClient(this);
  }

  void _unregisterInspectorControllerService() {
    inspectorController?.selectedNode
        ?.removeListener(_onSelectionChangedCallback);
    inspectorService?.removeClient(this);
  }

  
  public override void initState() {
    base.initState();
    rateLimiter = RateLimiter(maxRequestsPerSecond, refresh);
    _registerInspectorControllerService();
    _initAnimationStates();
    _updateObjectGroupManager();
    // TODO(jacobr): put inspector controller in Controllers and
    // update on didChangeDependencies.
    _animateProperties();
  }

  
  public override void didUpdateWidget(W oldWidget) {
    base.didUpdateWidget(oldWidget);
    _updateObjectGroupManager();
    _animateProperties();
    if (oldWidget.inspectorController != inspectorController) {
      _unregisterInspectorControllerService();
      _registerInspectorControllerService();
    }
  }

  
  public override void dispose() {
    entranceController.dispose();
    changeController.dispose();
    _unregisterInspectorControllerService();
    base.dispose();
  }

  void _animateProperties() {
    if (_animatedProperties != null) {
      changeController.forward();
    }
    if (_previousProperties != null) {
      entranceController.reverse();
    } else {
      entranceController.forward();
    }
  }
  
  Future setSelectionInspector(RemoteDiagnosticsNode node) {
    node.inspectorService.then((service) =>
    {
      service.setSelectionInspector(node.valueRef, false);
    });
    return null;
  }

  // update selected widget and trigger selection listener event to change focus.
  public void refreshSelection(RemoteDiagnosticsNode node) {
    inspectorController.refreshSelection(node, node, true);
  }

  public Future onTap(LayoutProperties properties) {
    setState(() => highlighted = properties);
    setSelectionInspector(properties.node);
    return null;
  }

  public void onDoubleTap(LayoutProperties properties) {
    refreshSelection(properties.node);
  }

  Future refresh() {
    if (!_dirty) return null;
    _dirty = false;
    fetchLayoutProperties().then((updatedProperties) =>
    {
      if (updatedProperties != null) _changeProperties(updatedProperties);
    });
    return null;
  }

  void _changeProperties(L nextProperties) {
    if (!mounted || nextProperties == null) return;
    updateHighlighted(nextProperties);
    setState(() => {
      _animatedProperties = computeAnimatedProperties(nextProperties);
      changeController.forward(from: 0.0f);
    });
  }

  void _setProperties(L newProperties) {
    if (!mounted) return;
    updateHighlighted(newProperties);
    if (_properties == newProperties) {
      return;
    }
    setState(() => {
      _previousProperties = _previousProperties ?? _properties;
      _properties = newProperties;
    });
    _animateProperties();
  }

  void _initAnimationStates()
  {
    entranceController = longAnimationController(
      this
    );
    entranceController.addStatusListener((status) => {
        if (status == AnimationStatus.dismissed) {
          setState(() => {
            _previousProperties = null;
            entranceController.forward();
          });
        }
      });
    entranceCurve = defaultCurvedAnimation(entranceController);
    changeController = longAnimationController(this);
    changeController.addStatusListener((status) => {
        if (status == AnimationStatus.completed) {
          setState(() => {
            _properties = _animatedProperties.end;
            _animatedProperties = null;
            changeController._value = 0.0f;
          });
        }
      });
    changeAnimation = defaultCurvedAnimation(changeController);
  }

  void _updateObjectGroupManager() {
    var service = inspectorController.inspectorService;
    if (service != objectGroupManager?.inspectorService) {
      objectGroupManager = InspectorObjectGroupManager(
        service,
        "flex-layout"
      );
    }
    onSelectionChanged();
  }

  bool _dirty = false;

  
  public override void onFlutterFrame() {
    if (!mounted) return;
    if (_dirty) {
      rateLimiter.scheduleRequest();
    }
  }

  // TODO(albertusangga): Investigate why onForceRefresh is not getting called.
  
  public override Future<Object> onForceRefresh()
  {
    fetchLayoutProperties().then((v) => 
    {
      _setProperties(v);
    });
    return null;
  }
  
  
  public override Future onInspectorSelectionChanged() {
    return null;
  }

  /// Register callback to be executed once Flutter frame is ready.
  public void markAsDirty() {
    _dirty = true;
  }
}

}*/