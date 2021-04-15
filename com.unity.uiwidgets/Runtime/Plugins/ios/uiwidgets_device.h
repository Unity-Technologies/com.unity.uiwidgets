#ifndef UIWidgetsDevice_h
#define UIWidgetsDevice_h

#import <UIKit/UIKit.h>

@interface UIWidgetsDevice : NSObject

+(NSString *) deviceName;

+(BOOL) NeedScreenDownSample;

@end

#endif
