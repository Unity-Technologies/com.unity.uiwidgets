

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_JSON_TYPE_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_JSON_TYPE_H_

//#ifdef USE_RAPID_JSON
#include <rapidjson/document.h>

using JsonValueType = rapidjson::Document;
#/*else
#include <json/json.h>

using JsonValueType = Json::Value;
#endif*/

#endif
