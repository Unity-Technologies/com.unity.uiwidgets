#include "unity_console.h"
#include <stdarg.h>

namespace uiwidgets {

 void UnityConsole::WriteLine(const char* fmt, ...) {
  char log_str[512] = { 0 };
  va_list ap;
  va_start(ap, fmt);
  vsprintf(log_str, fmt, ap);
  _log(log_str, strlen(log_str));
  va_end(ap);
}

LogDelegate UnityConsole::_log;

UIWIDGETS_API(void) 
InitNativeConsoleDelegate(LogDelegate Log) { 
  UnityConsole::_log = Log; 
}

}
