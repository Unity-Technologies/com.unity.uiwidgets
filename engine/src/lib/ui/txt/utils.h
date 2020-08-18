#pragma once

namespace uiwidgets {

#define DATALIST(T,N) \
extern "C" struct N{\
	T* data;\
	int length;\
};
	DATALIST(float, Float32List)
	DATALIST(int, Int32List)
	DATALIST(size_t, SizeTList)
	DATALIST(char, CharList)
	DATALIST(CharList, StringList)

	extern "C" struct StringList2 {
		char** data;
		int length;
	};
}