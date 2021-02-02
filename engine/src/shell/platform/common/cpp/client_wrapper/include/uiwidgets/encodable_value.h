

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_ENCODABLE_VALUE_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_ENCODABLE_VALUE_H_

#include <assert.h>

#include <cstdint>
#include <map>
#include <string>
#include <utility>
#include <vector>

namespace uiwidgets {

static_assert(sizeof(double) == 8, "EncodableValue requires a 64-bit double");

class EncodableValue;

using EncodableList = std::vector<EncodableValue>;
using EncodableMap = std::map<EncodableValue, EncodableValue>;

class EncodableValue {
 public:
  enum class Type {
    kNull,
    kBool,
    kInt,
    kLong,
    kDouble,
    kString,
    kByteList,
    kIntList,
    kLongList,
    kDoubleList,
    kList,
    kMap,
  };

  EncodableValue() {}

  explicit EncodableValue(bool value) : bool_(value), type_(Type::kBool) {}

  explicit EncodableValue(int32_t value) : int_(value), type_(Type::kInt) {}

  explicit EncodableValue(int64_t value) : long_(value), type_(Type::kLong) {}

  explicit EncodableValue(double value)
      : double_(value), type_(Type::kDouble) {}

  explicit EncodableValue(const char* value)
      : string_(new std::string(value)), type_(Type::kString) {}

  explicit EncodableValue(const std::string& value)
      : string_(new std::string(value)), type_(Type::kString) {}

  explicit EncodableValue(std::vector<uint8_t> list)
      : byte_list_(new std::vector<uint8_t>(std::move(list))),
        type_(Type::kByteList) {}

  explicit EncodableValue(std::vector<int32_t> list)
      : int_list_(new std::vector<int32_t>(std::move(list))),
        type_(Type::kIntList) {}

  explicit EncodableValue(std::vector<int64_t> list)
      : long_list_(new std::vector<int64_t>(std::move(list))),
        type_(Type::kLongList) {}

  explicit EncodableValue(std::vector<double> list)
      : double_list_(new std::vector<double>(std::move(list))),
        type_(Type::kDoubleList) {}

  explicit EncodableValue(EncodableList list)
      : list_(new EncodableList(std::move(list))), type_(Type::kList) {}

  explicit EncodableValue(EncodableMap map)
      : map_(new EncodableMap(std::move(map))), type_(Type::kMap) {}

  explicit EncodableValue(Type type) : type_(type) {
    switch (type_) {
      case Type::kNull:
        break;
      case Type::kBool:
        bool_ = false;
        break;
      case Type::kInt:
        int_ = 0;
        break;
      case Type::kLong:
        long_ = 0;
        break;
      case Type::kDouble:
        double_ = 0.0;
        break;
      case Type::kString:
        string_ = new std::string();
        break;
      case Type::kByteList:
        byte_list_ = new std::vector<uint8_t>();
        break;
      case Type::kIntList:
        int_list_ = new std::vector<int32_t>();
        break;
      case Type::kLongList:
        long_list_ = new std::vector<int64_t>();
        break;
      case Type::kDoubleList:
        double_list_ = new std::vector<double>();
        break;
      case Type::kList:
        list_ = new std::vector<EncodableValue>();
        break;
      case Type::kMap:
        map_ = new std::map<EncodableValue, EncodableValue>();
        break;
    }
  }

  ~EncodableValue() { DestroyValue(); }

  EncodableValue(const EncodableValue& other) {
    DestroyValue();

    type_ = other.type_;
    switch (type_) {
      case Type::kNull:
        break;
      case Type::kBool:
        bool_ = other.bool_;
        break;
      case Type::kInt:
        int_ = other.int_;
        break;
      case Type::kLong:
        long_ = other.long_;
        break;
      case Type::kDouble:
        double_ = other.double_;
        break;
      case Type::kString:
        string_ = new std::string(*other.string_);
        break;
      case Type::kByteList:
        byte_list_ = new std::vector<uint8_t>(*other.byte_list_);
        break;
      case Type::kIntList:
        int_list_ = new std::vector<int32_t>(*other.int_list_);
        break;
      case Type::kLongList:
        long_list_ = new std::vector<int64_t>(*other.long_list_);
        break;
      case Type::kDoubleList:
        double_list_ = new std::vector<double>(*other.double_list_);
        break;
      case Type::kList:
        list_ = new std::vector<EncodableValue>(*other.list_);
        break;
      case Type::kMap:
        map_ = new std::map<EncodableValue, EncodableValue>(*other.map_);
        break;
    }
  }

  EncodableValue(EncodableValue&& other) noexcept { *this = std::move(other); }

  EncodableValue& operator=(const EncodableValue& other) {
    if (&other == this) {
      return *this;
    }
    using std::swap;
    EncodableValue temp(other);
    swap(*this, temp);
    return *this;
  }

  EncodableValue& operator=(EncodableValue&& other) noexcept {
    if (&other == this) {
      return *this;
    }
    DestroyValue();

    type_ = other.type_;
    switch (type_) {
      case Type::kNull:
        break;
      case Type::kBool:
        bool_ = other.bool_;
        break;
      case Type::kInt:
        int_ = other.int_;
        break;
      case Type::kLong:
        long_ = other.long_;
        break;
      case Type::kDouble:
        double_ = other.double_;
        break;
      case Type::kString:
        string_ = other.string_;
        break;
      case Type::kByteList:
        byte_list_ = other.byte_list_;
        break;
      case Type::kIntList:
        int_list_ = other.int_list_;
        break;
      case Type::kLongList:
        long_list_ = other.long_list_;
        break;
      case Type::kDoubleList:
        double_list_ = other.double_list_;
        break;
      case Type::kList:
        list_ = other.list_;
        break;
      case Type::kMap:
        map_ = other.map_;
        break;
    }

    other.type_ = Type::kNull;
    return *this;
  }

  template <typename T>
  EncodableValue& operator=(const T& value) {
    *this = EncodableValue(value);
    return *this;
  }

  bool operator<(const EncodableValue& other) const {
    if (type_ != other.type_) {
      return type_ < other.type_;
    }
    switch (type_) {
      case Type::kNull:
        return false;
      case Type::kBool:
        return bool_ < other.bool_;
      case Type::kInt:
        return int_ < other.int_;
      case Type::kLong:
        return long_ < other.long_;
      case Type::kDouble:
        return double_ < other.double_;
      case Type::kString:
        return *string_ < *other.string_;
      case Type::kByteList:
      case Type::kIntList:
      case Type::kLongList:
      case Type::kDoubleList:
      case Type::kList:
      case Type::kMap:
        return this < &other;
    }
    assert(false);
    return false;
  }

  bool BoolValue() const {
    assert(IsBool());
    return bool_;
  }

  int32_t IntValue() const {
    assert(IsInt());
    return int_;
  }

  int64_t LongValue() const {
    assert(IsLong() || IsInt());
    if (IsLong()) {
      return long_;
    }
    return int_;
  }

  double DoubleValue() const {
    assert(IsDouble());
    return double_;
  }

  const std::string& StringValue() const {
    assert(IsString());
    return *string_;
  }

  const std::vector<uint8_t>& ByteListValue() const {
    assert(IsByteList());
    return *byte_list_;
  }

  std::vector<uint8_t>& ByteListValue() {
    assert(IsByteList());
    return *byte_list_;
  }

  const std::vector<int32_t>& IntListValue() const {
    assert(IsIntList());
    return *int_list_;
  }

  std::vector<int32_t>& IntListValue() {
    assert(IsIntList());
    return *int_list_;
  }

  const std::vector<int64_t>& LongListValue() const {
    assert(IsLongList());
    return *long_list_;
  }

  std::vector<int64_t>& LongListValue() {
    assert(IsLongList());
    return *long_list_;
  }

  const std::vector<double>& DoubleListValue() const {
    assert(IsDoubleList());
    return *double_list_;
  }

  std::vector<double>& DoubleListValue() {
    assert(IsDoubleList());
    return *double_list_;
  }

  const EncodableList& ListValue() const {
    assert(IsList());
    return *list_;
  }

  EncodableList& ListValue() {
    assert(IsList());
    return *list_;
  }

  const EncodableMap& MapValue() const {
    assert(IsMap());
    return *map_;
  }

  EncodableMap& MapValue() {
    assert(IsMap());
    return *map_;
  }

  bool IsNull() const { return type_ == Type::kNull; }

  bool IsBool() const { return type_ == Type::kBool; }

  bool IsInt() const { return type_ == Type::kInt; }

  bool IsLong() const { return type_ == Type::kLong; }

  bool IsDouble() const { return type_ == Type::kDouble; }

  bool IsString() const { return type_ == Type::kString; }

  bool IsByteList() const { return type_ == Type::kByteList; }

  bool IsIntList() const { return type_ == Type::kIntList; }

  bool IsLongList() const { return type_ == Type::kLongList; }

  bool IsDoubleList() const { return type_ == Type::kDoubleList; }

  bool IsList() const { return type_ == Type::kList; }

  bool IsMap() const { return type_ == Type::kMap; }

  Type type() const { return type_; }

 private:
  void DestroyValue() {
    switch (type_) {
      case Type::kNull:
      case Type::kBool:
      case Type::kInt:
      case Type::kLong:
      case Type::kDouble:
        break;
      case Type::kString:
        delete string_;
        break;
      case Type::kByteList:
        delete byte_list_;
        break;
      case Type::kIntList:
        delete int_list_;
        break;
      case Type::kLongList:
        delete long_list_;
        break;
      case Type::kDoubleList:
        delete double_list_;
        break;
      case Type::kList:
        delete list_;
        break;
      case Type::kMap:
        delete map_;
        break;
    }

    type_ = Type::kNull;
  }

  union {
    bool bool_;
    int32_t int_;
    int64_t long_;
    double double_;
    std::string* string_;
    std::vector<uint8_t>* byte_list_;
    std::vector<int32_t>* int_list_;
    std::vector<int64_t>* long_list_;
    std::vector<double>* double_list_;
    std::vector<EncodableValue>* list_;
    std::map<EncodableValue, EncodableValue>* map_;
  };

  Type type_ = Type::kNull;
};

}  // namespace uiwidgets

#endif
