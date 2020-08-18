#pragma once

#include <flutter/fml/memory/ref_counted.h>
#include <src\lib\ui\painting\canvas.h>
#include <flutter\third_party\txt\src\txt\paragraph.h>
#include "utils.h"

namespace uiwidgets {

	class Paragraph : public fml::RefCountedThreadSafe<Paragraph> {
		FML_FRIEND_MAKE_REF_COUNTED(Paragraph);

	public:
		static fml::RefPtr<Paragraph> Create();

		static fml::RefPtr<Paragraph> Create(std::unique_ptr<txt::Paragraph> txt_paragraph) {
			return fml::MakeRefCounted<Paragraph>(std::move(txt_paragraph));
		}

		~Paragraph();


		double width();
		double height();
		double longestLine();
		double minIntrinsicWidth();
		double maxIntrinsicWidth();
		double alphabeticBaseline();
		double ideographicBaseline();
		bool didExceedMaxLines();

		void layout(double width);
		void paint(Canvas* canvas, double x, double y);

		Float32List getRectsForRange(unsigned start,
			unsigned end,
			unsigned boxHeightStyle,
			unsigned boxWidthStyle);
		Float32List getRectsForPlaceholders();
		SizeTList getPositionForOffset(double dx, double dy);
		SizeTList getWordBoundary(unsigned offset);
		SizeTList getLineBoundary(unsigned offset);
		Float32List computeLineMetrics();

		size_t GetAllocationSize();
		std::unique_ptr<txt::Paragraph> m_paragraph;
	private:
		

		explicit Paragraph(std::unique_ptr<txt::Paragraph> paragraph);
	};

}  // namespace uiwidgets
