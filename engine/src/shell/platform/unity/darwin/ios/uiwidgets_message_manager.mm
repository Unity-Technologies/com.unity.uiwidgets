#include "uiwidgets_message_manager.h"
#include "runtime/mono_api.h"

#define MAX_OBJECT_NAME_LENGTH 256
static char uiwidgetsMessageObjectName[MAX_OBJECT_NAME_LENGTH] = {0};

static char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

void UIWidgetsMethodMessage(NSString* channel, NSString* method, NSArray *args)
{
    // const char* msg = NULL;
    // NSError *error;
    // NSDictionary* dict = @{
    //                     @"channel": channel,
    //                     @"method": method,
    //                     @"args": args
    //                     };

    // NSData* data = [NSJSONSerialization dataWithJSONObject:dict options:0 error:&error];
    // NSString* text = [[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease];
    // msg = [text UTF8String];
    
    // UnitySendMessage(uiwidgetsMessageObjectName, "OnUIWidgetsMethodMessage", msg);
}

UIWIDGETS_API(void) UIWidgetsMessageSetObjectName(const char* name){
    strlcpy(uiwidgetsMessageObjectName, name, MAX_OBJECT_NAME_LENGTH);
}

