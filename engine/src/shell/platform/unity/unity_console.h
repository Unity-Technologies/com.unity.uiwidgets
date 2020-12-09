#include "runtime/mono_api.h"

namespace uiwidgets {

typedef void (*LogDelegate)(char* message, int iSize);

extern "C"{
class UnityConsole{
 public:
   static void (*Log)(char* message,int iSize);
   /*
   output the log to unity editor console window
   */
   static void WriteLine(const char* fmt, ...){ 
   char log_str[512] = { 0 };
   va_list ap;
   va_start(ap, fmt);
   sprintf_s(log_str, fmt, ap);
   Log(log_str, strlen(log_str));
   va_end(ap);
  }
};
  void (*UnityConsole::Log)(char* message, int iSize);
  UIWIDGETS_API(void) 
  InitNativeConsoleDelegate(LogDelegate Log){ UnityConsole::Log = Log; }
}
} // namespace uiwidgets
