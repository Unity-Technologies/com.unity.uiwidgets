#include "lib/ui/ui_mono_state.h"
#include "paragraph.h"
#include "utils.h"

#include <flutter\third_party\txt\src\txt\paragraph.h>

namespace uiwidgets {
    Paragraph::Paragraph(std::unique_ptr<txt::Paragraph> paragraph)
        : m_paragraph(std::move(paragraph)) {}

    Paragraph::~Paragraph() = default;

    size_t Paragraph::GetAllocationSize() {
        // We don't have an accurate accounting of the paragraph's memory consumption,
        // so return a fixed size to indicate that its impact is more than the size
        // of the Paragraph class.
        return 2000;
    }

    double Paragraph::width() {
        return m_paragraph->GetMaxWidth();
    }

    double Paragraph::height() {
        return m_paragraph->GetHeight();
    }

    double Paragraph::longestLine() {
        return m_paragraph->GetLongestLine();
    }

    double Paragraph::minIntrinsicWidth() {
        return m_paragraph->GetMinIntrinsicWidth();
    }

    double Paragraph::maxIntrinsicWidth() {
        return m_paragraph->GetMaxIntrinsicWidth();
    }

    double Paragraph::alphabeticBaseline() {
        return m_paragraph->GetAlphabeticBaseline();
    }

    double Paragraph::ideographicBaseline() {
        return m_paragraph->GetIdeographicBaseline();
    }

    bool Paragraph::didExceedMaxLines() {
        return m_paragraph->DidExceedMaxLines();
    }

    void Paragraph::layout(double width) {
        m_paragraph->Layout(width);
    }

    void Paragraph::paint(Canvas * canvas, double x, double y) {
        SkCanvas* sk_canvas = canvas->canvas();
        if (!sk_canvas)
            return;
        m_paragraph->Paint(sk_canvas, x, y);
    }

    static Float32List EncodeTextBoxes(
        const std::vector<txt::Paragraph::TextBox> & boxes) {
        // Layout:
        // First value is the number of values.
        // Then there are boxes.size() groups of 5 which are LTRBD, where D is the
        // text direction index.
        Float32List result = {
            new float[boxes.size() * 5], 
            boxes.size() * 5 
        };

        unsigned long position = 0;
        for (unsigned long i = 0; i < boxes.size(); i++) {
            const txt::Paragraph::TextBox& box = boxes[i];
            result.data[position++] = box.rect.fLeft;
            result.data[position++] = box.rect.fTop;
            result.data[position++] = box.rect.fRight;
            result.data[position++] = box.rect.fBottom;
            result.data[position++] = static_cast<float>(box.direction);
        }
        return result;
    }

    Float32List Paragraph::getRectsForRange(unsigned start,
        unsigned end,
        unsigned boxHeightStyle,
        unsigned boxWidthStyle) {
        std::vector<txt::Paragraph::TextBox> boxes = m_paragraph->GetRectsForRange(
            start, end, static_cast<txt::Paragraph::RectHeightStyle>(boxHeightStyle),
            static_cast<txt::Paragraph::RectWidthStyle>(boxWidthStyle));
        return EncodeTextBoxes(boxes);
    }

    Float32List Paragraph::getRectsForPlaceholders() {
        std::vector<txt::Paragraph::TextBox> boxes =
            m_paragraph->GetRectsForPlaceholders();
        return EncodeTextBoxes(boxes);
    }

    SizeTList Paragraph::getPositionForOffset(double dx, double dy) {
        SizeTList result = {
            new size_t[2],
            2
        };
        txt::Paragraph::PositionWithAffinity pos =
            m_paragraph->GetGlyphPositionAtCoordinate(dx, dy);
        result.data[0] = pos.position;
        result.data[1] = static_cast<int>(pos.affinity);
        return result;
    }

    SizeTList Paragraph::getWordBoundary(unsigned offset) {
        txt::Paragraph::Range<size_t> point = m_paragraph->GetWordBoundary(offset);
        SizeTList result = {
            new size_t[2],
            2
        };
        result.data[0] = point.start;
        result.data[1] = point.end;
        return result;
    }

    SizeTList Paragraph::getLineBoundary(unsigned offset) {
        std::vector<txt::LineMetrics> metrics = m_paragraph->GetLineMetrics();
        int line_start = -1;
        int line_end = -1;
        for (txt::LineMetrics& line : metrics) {
            if (offset >= line.start_index && offset <= line.end_index) {
                line_start = line.start_index;
                line_end = line.end_index;
                break;
            }
        }
        SizeTList result = {
            new size_t[2],
            2
        };
        result.data[0] = line_start;
        result.data[1] = line_end;
        return result;
    }

    Float32List Paragraph::computeLineMetrics() {
        std::vector<txt::LineMetrics> metrics = m_paragraph->GetLineMetrics();

        // Layout:
        // boxes.size() groups of 9 which are the line metrics
        // properties
        Float32List result = {
            new float[metrics.size() * 9],
            2
        };
        unsigned long position = 0;
        for (unsigned long i = 0; i < metrics.size(); i++) {
            const txt::LineMetrics& line = metrics[i];
            result.data[position++] = static_cast<double>(line.hard_break);
            result.data[position++] = line.ascent;
            result.data[position++] = line.descent;
            result.data[position++] = line.unscaled_ascent;
            // We add then round to get the height. The
            // definition of height here is different
            // than the one in LibTxt.
            result.data[position++] = round(line.ascent + line.descent);
            result.data[position++] = line.width;
            result.data[position++] = line.left;
            result.data[position++] = line.baseline;
            result.data[position++] = static_cast<double>(line.line_number);
        }

        return result;
    }

    UIWIDGETS_API(double) Paragraph_width(Paragraph* ptr) {
        return ptr->width();
    }

    UIWIDGETS_API(double) Paragraph_height(Paragraph* ptr) {
        return ptr->height();
    }

    UIWIDGETS_API(double) Paragraph_longestLine(Paragraph* ptr) {
        return ptr->longestLine();
    }

    UIWIDGETS_API(double) Paragraph_minIntrinsicWidth(Paragraph* ptr) {
        return ptr->minIntrinsicWidth();
    }

    UIWIDGETS_API(double) Paragraph_maxIntrinsicWidth(Paragraph* ptr) {
        return ptr->maxIntrinsicWidth();
    }

    UIWIDGETS_API(double) Paragraph_alphabeticBaseline(Paragraph* ptr) {
        return ptr->alphabeticBaseline();
    }

    UIWIDGETS_API(double) Paragraph_ideographicBaseline(Paragraph* ptr) {
        return ptr->ideographicBaseline();
    }

    UIWIDGETS_API(bool) Paragraph_didExceedMaxLines(Paragraph* ptr) {
        return ptr->didExceedMaxLines();
    }

    UIWIDGETS_API(void) Paragraph_layout(Paragraph* ptr, double width) {
        ptr->layout(width);
    }

    UIWIDGETS_API(Float32List) Paragraph_getRectsForRange(Paragraph* ptr, int start, int end, int boxHeightStyle, int boxWidthStyle) {
        return ptr->getRectsForRange(start, end, boxHeightStyle, boxWidthStyle);
    }

    UIWIDGETS_API(Float32List) Paragraph_getRectsForPlaceholders(Paragraph* ptr) {
        return ptr->getRectsForPlaceholders();
    }
    UIWIDGETS_API(SizeTList) Paragraph_getPositionForOffset(Paragraph* ptr, double dx, double dy) {
        return ptr->getPositionForOffset(dx, dy);
    }

    UIWIDGETS_API(SizeTList) Paragraph_getWordBoundary(Paragraph* ptr, int offset) {
        return ptr->getWordBoundary(offset);
    }

    UIWIDGETS_API(SizeTList) Paragraph_getLineBoundary(Paragraph* ptr, int offset) {
        return ptr->getLineBoundary(offset);
    }

    UIWIDGETS_API(void) Paragraph_paint(Paragraph* ptr, Canvas* canvas, double x, double y) {
        ptr->paint(canvas, x, y);
    }

    UIWIDGETS_API(Float32List) Paragraph_computeLineMetrics(Paragraph* ptr) {
        return ptr->computeLineMetrics();
    }

    UIWIDGETS_API(void) Paragraph_dispose(Paragraph* ptr) {
        ptr->Release();
    }
}