#if TARGET_OS_IOS

#import "UnityAppController.h"
#import <TargetConditionals.h>
#import "custom_app_controller.h"

extern void UnityPluginLoad(struct IUnityInterfaces *interfaces);
extern void UnityPluginUnload(void);

#pragma mark - App controller subclasssing

@implementation CustomUIWidgetsAppController
- (void)shouldAttachRenderDelegate;
{
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(CustomUIWidgetsAppController);

#endif 