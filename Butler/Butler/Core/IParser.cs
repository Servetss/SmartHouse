using AngleSharp.Dom.Html;

namespace Butler.Core
{
    interface IParser<T> where T: class
    {
        T Parse(IHtmlDocument document);
    }
}
