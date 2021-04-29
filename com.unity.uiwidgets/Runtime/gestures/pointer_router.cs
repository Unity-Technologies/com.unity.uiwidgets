using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerRoute(PointerEvent evt);

    public class PointerRouter {
        readonly Dictionary<int, Dictionary<PointerRoute, Matrix4>> _routeMap =
            new Dictionary<int, Dictionary<PointerRoute, Matrix4>>();

        readonly Dictionary<PointerRoute, Matrix4> _globalRoutes = new Dictionary<PointerRoute, Matrix4>();

        public void addRoute(int pointer, PointerRoute route, Matrix4 transform = null) {
            var routes = _routeMap.putIfAbsent(
                pointer,
                () => new Dictionary<PointerRoute, Matrix4>()
            );
            D.assert(!routes.ContainsKey(route));
            routes[route] = transform;
        }

        public void removeRoute(int pointer, PointerRoute route) {
            D.assert(_routeMap.ContainsKey(pointer));
            var routes = _routeMap[pointer];
            routes.Remove(route);
            if (routes.isEmpty()) {
                _routeMap.Remove(pointer);
            }
        }

        public void addGlobalRoute(PointerRoute route, Matrix4 transform = null) {
            D.assert(!_globalRoutes.ContainsKey(route));
            _globalRoutes[route] = transform;
        }

        public void removeGlobalRoute(PointerRoute route) {
            D.assert(_globalRoutes.ContainsKey(route));
            _globalRoutes.Remove(route);
        }

        public bool acceptScroll() {
            return _routeMap.Count == 0;
        }

        public void clearScrollRoute(int pointer) {
            if (_routeMap.ContainsKey(pointer)) {
                _routeMap.Remove(pointer);
            }
        }


        void _dispatch(PointerEvent evt, PointerRoute route, Matrix4 transform) {
            try {
                evt = evt.transformed(transform);
                route(evt);
            }
            catch (Exception ex) {
                D.logError("Error while routing a pointer event: ", ex);
            }
        }

        public void route(PointerEvent evt) {
            // TODO: update this to latest version
            Dictionary<PointerRoute, Matrix4> routes;
            _routeMap.TryGetValue(evt.pointer, out routes);

            Dictionary<PointerRoute, Matrix4> copiedGlobalRoutes = new Dictionary<PointerRoute, Matrix4>(_globalRoutes);

            if (routes != null) {
                _dispatchEventToRoutes(
                    evt,
                    routes,
                    new Dictionary<PointerRoute, Matrix4>(routes)
                );
            }

            _dispatchEventToRoutes(evt, _globalRoutes, copiedGlobalRoutes);
        }

        public void _dispatchEventToRoutes(
            PointerEvent evt,
            Dictionary<PointerRoute, Matrix4> referenceRoutes,
            Dictionary<PointerRoute, Matrix4> copiedRoutes
        ) {
            foreach (var item in copiedRoutes) {
                var route = item.Key;
                var transform = item.Value;
                if (referenceRoutes.ContainsKey(route)) {
                    _dispatch(evt, route, transform);
                }
            }
        }
    }
}

// TODO: FlutterErrorDetailsForPointerRouter