using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class PagesUtils {
        public static Widget _defaultTransitionsBuilder(BuildContext context, Animation<float>
            animation, Animation<float> secondaryAnimation, Widget child) {
            return child;
        }
    }

    public abstract class PageRoute : ModalRoute {
        
        public PageRoute(
            RouteSettings settings = null, 
            bool fullscreenDialog = false
        ) : base(settings) {
            this.fullscreenDialog = fullscreenDialog;
        }
        
        public readonly bool fullscreenDialog;

        public override bool opaque {
            get { return true; }
        }

        public override bool barrierDismissible {
            get { return false; }
        }

        public override bool canTransitionTo(TransitionRoute nextRoute) {
            return nextRoute is PageRoute;
        }

        public override bool canTransitionFrom(TransitionRoute previousRoute) {
            return previousRoute is PageRoute;
        }

    }

    public class PageRouteBuilder : PageRoute {
        public readonly RoutePageBuilder pageBuilder;

        public readonly RouteTransitionsBuilder transitionsBuilder;

        public PageRouteBuilder(
            RouteSettings settings = null,
            RoutePageBuilder pageBuilder = null,
            RouteTransitionsBuilder transitionsBuilder = null,
            TimeSpan? transitionDuration = null,
            bool opaque = true,
            bool barrierDismissible = false,
            Color barrierColor = null,
            string barrierLabel = null,
            bool maintainState = true,
            bool fullscreenDialog = false
        ) : base(settings,fullscreenDialog) {
            D.assert(pageBuilder != null);
            this.opaque = opaque;
            this.pageBuilder = pageBuilder;
            this.transitionsBuilder = transitionsBuilder ?? PagesUtils._defaultTransitionsBuilder;
            this.transitionDuration = transitionDuration ?? TimeSpan.FromMilliseconds(300);
            this.barrierColor = barrierColor;
            this.barrierLabel = barrierLabel;
            this.maintainState = maintainState;
            this.barrierDismissible = barrierDismissible;
        }

        public override TimeSpan transitionDuration { get; }

        public override bool opaque { get; }

        public override string barrierLabel { get; }
        public override bool barrierDismissible { get; }

        public override Color barrierColor { get; }

        public override bool maintainState { get; }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return pageBuilder(context, animation, secondaryAnimation);
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return transitionsBuilder(context, animation, secondaryAnimation, child);
        }
    }
}