#include "pipeline.h"

namespace uiwidgets {

size_t GetNextPipelineTraceID() {
  static std::atomic_size_t PipelineLastTraceID = {0};
  return ++PipelineLastTraceID;
}

}  // namespace uiwidgets
