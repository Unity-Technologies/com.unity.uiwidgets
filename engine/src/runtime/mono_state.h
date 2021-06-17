#pragma once

#include <functional>
#include <memory>

#include "mono_isolate_scope.h"

namespace uiwidgets {

class MonoState : public std::enable_shared_from_this<MonoState> {
 public:
  class Scope {
   public:
    explicit Scope(MonoState* mono_state);
    explicit Scope(std::shared_ptr<MonoState> mono_state);
    ~Scope();

   private:
    MonoIsolateScope scope_;
  };

  MonoState();
  virtual ~MonoState();

  static MonoState* From(Mono_Isolate isolate);
  static MonoState* Current();
  static bool EnsureCurrentIsolate();

  std::weak_ptr<MonoState> GetWeakPtr();

  Mono_Isolate isolate() { return isolate_; }
  void SetIsolate(Mono_Isolate isolate);

 private:
  Mono_Isolate isolate_;

 protected:
  FML_DISALLOW_COPY_AND_ASSIGN(MonoState);
};

}  // namespace uiwidgets
