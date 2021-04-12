#include "switches.h"

#include <cstdint>

namespace uiwidgets {

#if OS_ANDROID || OS_WIN
extern "C" uint8_t _binary_icudtl_dat_start[];
extern "C" uint8_t _binary_icudtl_dat_end[];

std::unique_ptr<fml::Mapping> GetICUStaticMapping() {
  return std::make_unique<fml::NonOwnedMapping>(
      _binary_icudtl_dat_start,
      _binary_icudtl_dat_end - _binary_icudtl_dat_start);
}
#endif

std::unique_ptr<fml::Mapping> GetSymbolMapping(std::string symbol_prefix,
                                               std::string native_lib_path) {
  const uint8_t* mapping;
  intptr_t size;

  auto lookup_symbol = [&mapping, &size, symbol_prefix](
                           const fml::RefPtr<fml::NativeLibrary>& library) {
    mapping = library->ResolveSymbol((symbol_prefix + "_start").c_str());
    size = reinterpret_cast<intptr_t>(
        library->ResolveSymbol((symbol_prefix + "_size").c_str()));
  };

  fml::RefPtr<fml::NativeLibrary> library =
      fml::NativeLibrary::CreateForCurrentProcess();
  lookup_symbol(library);

  if (!(mapping && size)) {
    // Symbol lookup for the current process fails on some devices.  As a
    // fallback, try doing the lookup based on the path to the Flutter library.
    library = fml::NativeLibrary::Create(native_lib_path.c_str());
    lookup_symbol(library);
  }

  FML_CHECK(mapping && size) << "Unable to resolve symbols: " << symbol_prefix;
  return std::make_unique<fml::NonOwnedMapping>(mapping, size);
}

}  // namespace uiwidgets