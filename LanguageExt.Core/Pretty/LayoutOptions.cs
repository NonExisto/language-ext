namespace LanguageExt.Pretty
{
    public record LayoutOptions(PageWidth PageWidth)
    {
        public static readonly LayoutOptions Default = new LayoutOptions(PageWidth.Default);
    }
}
