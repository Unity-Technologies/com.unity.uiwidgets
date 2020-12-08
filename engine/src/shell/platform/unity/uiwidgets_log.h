#include "runtime/mono_api.h"

namespace uiwidgets {
    //use UnityLog just like printf, and this will output the result to unity
#define UnityLog(...)   {char log_str[512] = { 0 }; sprintf_s(log_str, __VA_ARGS__); Debug::Log(log_str, strlen(log_str));}
extern "C"
{
    class Debug
    {
    public:
        static void (*Log)(char* message,int iSize);
    };

    void (*Debug::Log)(char* message, int iSize);
    // export c++ function interface for c#
    UIWIDGETS_API(void) InitCSharpDelegate(void (*Log)(char* message, int iSize)){Debug::Log = Log;}
}

} // namespace uiwidgets
