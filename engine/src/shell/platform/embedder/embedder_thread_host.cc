#define FML_USED_ON_EMBEDDER

#include "embedder_thread_host.h"

#include <algorithm>

#include "flutter/fml/message_loop.h"

namespace uiwidgets {

static std::pair<bool, fml::RefPtr<EmbedderTaskRunner>>
CreateEmbedderTaskRunner(const UIWidgetsTaskRunnerDescription* description) {
  if (description == nullptr) {
    // This is not embedder error. The embedder API will just have to create a
    // plain old task runner (and create a thread for it) instead of using a
    // task runner provided to us by the embedder.
    return {true, {}};
  }

  if (description->runs_task_on_current_thread_callback == nullptr) {
    FML_LOG(ERROR) << "UIWidgetsTaskRunnerDescription.runs_task_on_current_"
                      "thread_callback was nullptr.";
    return {false, {}};
  }

  if (description->post_task_callback == nullptr) {
    FML_LOG(ERROR)
        << "UIWidgetsTaskRunnerDescription.post_task_callback was nullptr.";
    return {false, {}};
  }

  auto user_data = description->user_data;

  auto post_task_callback_c = description->post_task_callback;
  auto runs_task_on_current_thread_callback_c =
      description->runs_task_on_current_thread_callback;

  EmbedderTaskRunner::DispatchTable task_runner_dispatch_table = {
      // .post_task_callback
      [post_task_callback_c, user_data](EmbedderTaskRunner* task_runner,
                                        uint64_t task_baton,
                                        fml::TimePoint target_time) -> void {
        UIWidgetsTask task = {
            // runner
            reinterpret_cast<UIWidgetsTaskRunner>(task_runner),
            // task
            task_baton,
        };
        post_task_callback_c(task, target_time.ToEpochDelta().ToNanoseconds(),
                             user_data);
      },
      // runs_task_on_current_thread_callback
      [runs_task_on_current_thread_callback_c, user_data]() -> bool {
        return runs_task_on_current_thread_callback_c(user_data);
      }};

  return {true, fml::MakeRefCounted<EmbedderTaskRunner>(
                    task_runner_dispatch_table, description->identifier)};
}

static fml::RefPtr<fml::TaskRunner> GetCurrentThreadTaskRunner() {
  fml::MessageLoop::EnsureInitializedForCurrentThread();
  return fml::MessageLoop::GetCurrent().GetTaskRunner();
}

constexpr const char* kUIWidgetsThreadName = "io.uiwidgets";

// static
std::unique_ptr<EmbedderThreadHost>
EmbedderThreadHost::CreateEmbedderManagedThreadHost(
    const UIWidgetsCustomTaskRunners* custom_task_runners) {
  if (custom_task_runners == nullptr) {
    return nullptr;
  }

  // The IO threads are always created by the engine and the embedder
  // has no opportunity to specify task runners for the same.
  //
  // If/when more task runners are exposed, this mask will need to be updated.
  uint64_t engine_thread_host_mask = ThreadHost::Type::IO;

  auto platform_task_runner_pair =
      CreateEmbedderTaskRunner(custom_task_runners->platform_task_runner);
  auto render_task_runner_pair =
      CreateEmbedderTaskRunner(custom_task_runners->render_task_runner);
  auto ui_task_runner_pair =
      CreateEmbedderTaskRunner(custom_task_runners->ui_task_runner);

  if (!platform_task_runner_pair.first || !render_task_runner_pair.first ||
      !ui_task_runner_pair.first) {
    // User error while supplying a custom task runner. Return an invalid thread
    // host. This will abort engine initialization. Don't fallback to defaults
    // if the user wanted to specify a task runner but just messed up instead.
    return nullptr;
  }

  // If the embedder has not supplied a GPU task runner, one needs to be
  // created.
  if (!render_task_runner_pair.second) {
    engine_thread_host_mask |= ThreadHost::Type::GPU;
  }

  // If both the platform task runner and the GPU task runner are specified and
  // have the same identifier, store only one.
  if (platform_task_runner_pair.second && render_task_runner_pair.second) {
    if (platform_task_runner_pair.second->GetEmbedderIdentifier() ==
        render_task_runner_pair.second->GetEmbedderIdentifier()) {
      render_task_runner_pair.second = platform_task_runner_pair.second;
    }
  }

  if (platform_task_runner_pair.second && ui_task_runner_pair.second) {
    if (platform_task_runner_pair.second->GetEmbedderIdentifier() ==
        ui_task_runner_pair.second->GetEmbedderIdentifier()) {
      ui_task_runner_pair.second = platform_task_runner_pair.second;
    }
  }

  // Create a thread host with just the threads that need to be managed by the
  // engine. The embedder has provided the rest.
  ThreadHost thread_host(kUIWidgetsThreadName, engine_thread_host_mask);

  // If the embedder has supplied a platform task runner, use that. If not, use
  // the current thread task runner.
  auto platform_task_runner =
      platform_task_runner_pair.second
          ? static_cast<fml::RefPtr<fml::TaskRunner>>(
                platform_task_runner_pair.second)
          : GetCurrentThreadTaskRunner();

  // If the embedder has supplied a GPU task runner, use that. If not, use the
  // one from our thread host.
  auto render_task_runner = render_task_runner_pair.second
                                ? static_cast<fml::RefPtr<fml::TaskRunner>>(
                                      render_task_runner_pair.second)
                                : thread_host.raster_thread->GetTaskRunner();

  // If the embedder has supplied a ui task runner, use that. If not, use
  // the current thread task runner.
  auto ui_task_runner = ui_task_runner_pair.second
                            ? static_cast<fml::RefPtr<fml::TaskRunner>>(
                                  ui_task_runner_pair.second)
                            : GetCurrentThreadTaskRunner();

  TaskRunners task_runners(
      kUIWidgetsThreadName,
      platform_task_runner,                   // platform
      render_task_runner,                     // raster
      ui_task_runner,                         // ui
      thread_host.io_thread->GetTaskRunner()  // io (always engine managed)
  );

  if (!task_runners.IsValid()) {
    return nullptr;
  }

  std::set<fml::RefPtr<EmbedderTaskRunner>> embedder_task_runners;

  if (platform_task_runner_pair.second) {
    embedder_task_runners.insert(platform_task_runner_pair.second);
  }

  if (render_task_runner_pair.second) {
    embedder_task_runners.insert(render_task_runner_pair.second);
  }

  if (ui_task_runner_pair.second) {
    embedder_task_runners.insert(ui_task_runner_pair.second);
  }

  auto embedder_host = std::make_unique<EmbedderThreadHost>(
      std::move(thread_host), task_runners, embedder_task_runners);

  if (embedder_host->IsValid()) {
    return embedder_host;
  }

  return nullptr;
}

EmbedderThreadHost::EmbedderThreadHost(
    ThreadHost host, TaskRunners runners,
    const std::set<fml::RefPtr<EmbedderTaskRunner>>& embedder_task_runners)
    : host_(std::move(host)), runners_(runners) {
  for (const auto& runner : embedder_task_runners) {
    runners_map_[reinterpret_cast<int64_t>(runner.get())] = runner;
  }
}

EmbedderThreadHost::~EmbedderThreadHost() = default;

bool EmbedderThreadHost::IsValid() const { return runners_.IsValid(); }

const TaskRunners& EmbedderThreadHost::GetTaskRunners() const {
  return runners_;
}

bool EmbedderThreadHost::PostTask(int64_t runner, uint64_t task) const {
  auto found = runners_map_.find(runner);
  if (found == runners_map_.end()) {
    return false;
  }
  return found->second->PostTask(task);
}

}  // namespace uiwidgets
