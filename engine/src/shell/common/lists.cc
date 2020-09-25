#pragma once

#include "lists.h"

namespace uiwidgets {
namespace {
UIWIDGETS_API(void) Lists_Free(void* data) { delete []data; }  // namespace
}  // namespace
}  // namespace uiwidgets