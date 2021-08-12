using System.Runtime.Versioning;
using Unity.UIWidgets.async;

class CastStreamTransformer<SS, ST, TS, TT>
    : StreamTransformerBase<TS, TT> {
    public readonly StreamTransformer<SS, ST> _source;

    public CastStreamTransformer(StreamTransformer<SS, ST> _source) {
        this._source = _source;
    }

    public override StreamTransformer<RS, RT> cast<RS, RT>() =>
        new CastStreamTransformer<SS, ST, RS, RT>(_source);
    public override Stream<TT> bind(Stream<TS> stream) =>
        _source.bind(stream.cast<SS>()).cast<TT>();
}