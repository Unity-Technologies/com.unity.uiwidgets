using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class ScrollAwareImageProvider<T> : ImageProvider<T> {
        public ScrollAwareImageProvider(
            DisposableBuildContext context = null,
            ImageProvider<T> imageProvider = null
        ) {
            D.assert(context != null);
            D.assert(imageProvider != null);
            this.context = context;
            this.imageProvider = imageProvider;
        }
        public readonly DisposableBuildContext context;

        public readonly ImageProvider<T> imageProvider;
        public override void resolveStreamForKey(
            ImageConfiguration configuration, 
            ImageStream stream, T key,
            ImageErrorListener handleError) {
   
            if (stream.completer != null || PaintingBinding.instance.imageCache.containsKey(key)) {
              imageProvider.resolveStreamForKey(configuration, stream, key, handleError);
              return;
            }
          
            if (context.context == null) {
              return;
            }
            
            if (Scrollable.recommendDeferredLoadingForContext(context.context)) {
                SchedulerBinding.instance.scheduleFrameCallback((_)=> {
                  async_.scheduleMicrotask(
                      () => {
                          resolveStreamForKey(configuration, stream, key, handleError);
                          return null;
                      }
                      
                  );
                });
                return;
            }
          
            imageProvider.resolveStreamForKey(configuration, stream, key, handleError); 
        }
        public override ImageStreamCompleter load(T key, DecoderCallback decode) => imageProvider.load(key, decode);
        public override Future<T> obtainKey(ImageConfiguration configuration) => imageProvider.obtainKey(configuration);
    }
}