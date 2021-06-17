#import <UIKit/UIKit.h>
#import "uiwidgets_device.h"
#include "runtime/mono_api.h"

namespace uiwidgets {

    UIWIDGETS_API(float) IOSDeviceScaleFactor()
    {
        float scale = [[UIScreen mainScreen] scale] * 1.0;
        if ([UIWidgetsDevice NeedScreenDownSample]) {
            scale *= 0.8696;
        }
        return scale;
    }
}