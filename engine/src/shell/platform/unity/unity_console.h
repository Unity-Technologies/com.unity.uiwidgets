#pragma once
#include "runtime/mono_api.h"

namespace uiwidgets {

typedef void (*LogDelegate)(char* message, int iSize);


class UnityConsole{
 public:
  static LogDelegate _log;

   /**
   output the log to unity editor console window
    @param fmt   log format
    @param ...   log args
    @return      null

    example: 
      UnityConsole::WriteLine("output log without fmt param");
      UnityConsole::WriteLine("%s: %d + %d = %d","output log with param", 1, 2, 3);
   */
  static void WriteLine(const char* fmt, ...);
};

} // namespace uiwidgets
