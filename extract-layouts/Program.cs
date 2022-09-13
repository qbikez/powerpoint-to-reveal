using Microsoft.Extensions.Configuration;
using System.Collections;
using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var rootCommand = new RootCommand("pptx to reveal layout converter");
var inOption = new Option<string>("--in", () => "./", "in dir");
var outOption = new Option<string>("--out", () => "./html", "out dir");

rootCommand.AddOption(inOption);
rootCommand.AddOption(outOption);

rootCommand.SetHandler((inOptionValue, outOptionValue) =>
{
    var converter = new OdtConverter(inOptionValue, outOptionValue);

    converter.Convert();
}, inOption, outOption);

await rootCommand.InvokeAsync(args);

public class Ns
{
    public static readonly XNamespace style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    public static readonly XNamespace office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    public static readonly XNamespace presentation = "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0";
    public static readonly XNamespace svg = "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0";
    public static readonly XNamespace draw = "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0";
    public static readonly XNamespace text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    public static readonly XNamespace fo = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
    public static readonly XNamespace xlink = "http://www.w3.org/1999/xlink";
}

public class OdtConverter
{
    public string InputDir { get; }
    public string OutputDir { get; }

    public string? TemplateName { get; }
    private XElement Xml { get; set; } = default!;

    StylesManager StylesManager { get; }

    public OdtConverter(string inputDir, string outputDir)
    {
        InputDir = inputDir;
        OutputDir = outputDir;
        Xml = Load(InputDir);
        TemplateName = Path.GetFileName(OutputDir.TrimEnd(new[] { '/', '\\' }));

        StylesManager = new StylesManager(Xml);
    }

    public void Convert()
    {
        EnsureOutputDir();
        CopyMediaFiles();
        ProcessMasterPages();
    }

    private void ProcessMasterPages()
    {
        var masterPages = Xml.Descendants(Ns.style + "master-page");
        System.Console.WriteLine($"masterPages: {masterPages.Count()}");

        int idx = 0;
        var cssBuilder = new StringBuilder();
        foreach (var page in masterPages)
        {
            System.Console.WriteLine($"{page.Attribute(Ns.style + "name")!.Value}");

            var (html, css) = ProcessPage(page, idx);
            cssBuilder.AppendLine(css.ToString());
            idx++;
        }

        using (var sw = new StreamWriter($"{OutputDir}/styles.css"))
        {
            var fonts = Directory.GetFiles($"{OutputDir}/media", "*.ttf");
            var fontBuilder = new StringBuilder();

            foreach (var font in fonts)
            {
                var fontName = string.Join(" ", Path.GetFileNameWithoutExtension(font).Split("-"));
                var fontFile = Path.GetFileName(font);
                fontBuilder.AppendLine(@$"
                @font-face {{
                    font-family: '{fontName}';
                    src: url('media/{fontFile}');
                }}
                ");
            }
            sw.WriteLine(fontBuilder.ToString());
            sw.WriteLine(@"
            * {
        margin: unset;
        margin-block: unset;
    }
    /* li {
        list-style-position: inside;
    }
    li p {
        display: inline-block;
    }*/
    ");
            sw.WriteLine(cssBuilder.ToString());
            sw.Flush();
        }
    }

    private (StringBuilder html, Css css) ProcessPage(XElement page, int idx)
    {
        var pageName = page.Attribute(Ns.style + "name")!.Value;
        pageName = pageName.Replace("\\", "--").Replace("/", "--");
        var custIdx = pageName.IndexOf("-cust-");
        if (custIdx >= 0) pageName = pageName.Substring(custIdx + "-cust-".Length);
        pageName = $"{idx:000}_{pageName}";

        using (var writer = XmlWriter.Create($"{OutputDir}/{pageName}.xml", new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "    ",
            NewLineOnAttributes = false
        }))
        {
            page.WriteTo(writer);
        }

        var allStyles = StylesManager.GetAllStylesFor(page);
        using (var writer = XmlWriter.Create($"{OutputDir}/{pageName}.styles.xml", new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "    ",
            NewLineOnAttributes = false
        }))
        {
            writer.WriteStartElement(prefix: "ofice", localName: "document-styles", ns: "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            //writer.WriteRaw("""xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">""");
            writer.WriteStartElement(prefix: "office", localName: "styles", ns: "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            foreach (var s in allStyles) { s.WriteTo(writer); }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        var (html, css) = new PageProcessor(Xml, page, StylesManager, TemplateName).Process();

        File.WriteAllText($"{OutputDir}/{pageName}.html", html.ToString());

        return (html, css);
    }

    private void CopyMediaFiles()
    {
        var inMedia = Path.Combine(InputDir, "media");
        if (Directory.Exists(inMedia))
        {
            var outMedia = Path.Combine(OutputDir, "media");
            if (!Directory.Exists(outMedia)) Directory.CreateDirectory(outMedia);
            foreach (var mediaFile in Directory.GetFiles(inMedia))
            {
                var src = mediaFile;
                var dst = Path.Combine(outMedia, Path.GetFileName(mediaFile));
                Console.WriteLine($"{src} => {dst}");
                try
                {
                    File.Copy(src, dst, overwrite: true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }
        }
    }

    private void EnsureOutputDir()
    {
        if (!Directory.Exists(OutputDir))
        {
            Directory.CreateDirectory(OutputDir);
        }
    }

    public static XElement Load(string inputDir)
    {
        var xml = XElement.Load(Path.Combine(inputDir, "styles.xml"));
        Console.WriteLine($"Loaded {xml.Nodes().Count()} nodes");

        return xml;
    }
}


public class PageProcessor
{
    public XElement Page { get; }
    public XElement Xml { get; }
    public XElement? Layout { get; private set; }
    public StylesManager StylesManager { get; }

    class Context
    {
        public int ListLevel { get; set; } = 0;
    }

    private Context context;

    public string TemplateName { get; }

    public PageProcessor(XElement xml, XElement page, StylesManager stylesManager, string templateName)
    {
        Page = page;
        Xml = xml;
        StylesManager = stylesManager;
        TemplateName = templateName;
    }

    public (StringBuilder, Css) Process()
    {
        context = new Context();

        var pageWidth = "";
        var pageHeight = "";
        var layoutName = Page.Attribute(Ns.style + "page-layout-name")?.Value;
        Layout = Xml.Descendants(Ns.style + "page-layout")
            .FirstOrDefault(l => l.Attribute(Ns.style + "name")?.Value == layoutName);

        if (Layout is not null)
        {
            // using (var writer = XmlWriter.Create($"{OutputDir}/{layoutName}.xml"))
            // {
            //     layout.WriteTo(writer);
            // }

            var layoutProps = Layout.Descendants(Ns.style + "page-layout-properties").FirstOrDefault();
            pageWidth = StylesManager.ConvertUnits(layoutProps?.Attribute(Ns.fo + "page-width")?.Value) ?? "";
            pageHeight = StylesManager.ConvertUnits(layoutProps?.Attribute(Ns.fo + "page-height")?.Value) ?? "";
        }

        var body = new StringBuilder();

        var style = StylesManager.GetElementStyle(Page);

        var backgroundImg = style?.AllAttributes?.FirstOrDefault(a => a.Name.LocalName == "background-image")?.Value;
        if (backgroundImg is not null) body.WriteBackgroundDiv(backgroundImg, width: pageWidth, height: pageHeight);

        VisitPage(body, Page);

        var css = Page.Annotation<Css>() ?? new Css();
        css.Entries.Add(new CssEntry($".{style.StyleName}", new Dictionary<string, string>()
        {
            ["text-align"] = "left"
        }));
        WrapCss(css, $".{TemplateName} ");

        var html = new StringBuilder();

        html.OpenTag("html", new Dictionary<string, string>()
        {
            //["xmlns"] = "http://www.w3.org/1999/xhtml"
        });

        html.OpenTag("head");
        html.OpenTag("link", new Dictionary<string, string>
        {
            ["rel"] = "stylesheet",
            ["type"] = "text/css",
            ["href"] = "styles.css"

        });
        html.CloseTag("link");
        html.CloseTag("head");

        html.OpenTag("body", new Dictionary<string, string>()
        {
            ["style"] = $"width: {pageWidth}; height: {pageHeight};",
            ["class"] = TemplateName
        });
        html.AppendLine(body.ToString());
        html.CloseTag("body");

        html.CloseTag("html");

        return (html, css);
    }

    private static Css MergeCss(Css parent, Css child)
    {
        var merged = new Css();
        merged.Entries.AddRange(parent.Entries);

        foreach (var childEntry in child.Entries)
        {
            var mergedEntry = parent.FirstOrDefault(e => e.Selector == childEntry.Selector);
            if (mergedEntry is null)
            {
                mergedEntry = new CssEntry(childEntry.Selector, new Dictionary<string, string>());
                merged.Entries.Add(mergedEntry);
            }
            foreach (var kvp in childEntry.Properties)
            {
                mergedEntry.Properties[kvp.Key] = kvp.Value;
            }
        }

        return merged;
    }

    private static Css WrapCss(Css css, string preSelector)
    {
        foreach (var entry in css)
        {
            entry.Selector = $"{preSelector}{entry.Selector}";
        }
        return css;
    }

    void VisitPage(StringBuilder body, XElement page) => VisitNode(body, Page);

    void VisitGraphics(StringBuilder html, XElement g)
    {
        html.OpenTag("div");

        var shapes = g.Elements(Ns.draw + "custom-shape");
        foreach (var shape in shapes) VisitShape(html, shape);

        html.CloseTag("div");
    }

    void VisitShape(StringBuilder html, XElement shape)
    {
        var attrs = ParseAttributes(shape);
        html.OpenTag("div", attrs);

        var geometries = shape.Elements(Ns.draw + "enhanced-geometry");
        var (style, styleName) = StylesManager.GetElementCss(shape);

        foreach (var geometry in geometries)
        {
            VisitGeometry(html, geometry, style);
        }

        html.CloseTag("div");
    }

    void VisitGeometry(StringBuilder html, XElement geometry, Dictionary<string, string> parentStyle)
    {
        var attrs = ParseAttributes(geometry);
        var viewBox = attrs.ContainsKey("viewBox") ? attrs["viewBox"] : "";
        var path = attrs.ContainsKey("enhanced-path") ? attrs["enhanced-path"] : "";
        var fill = parentStyle.ContainsKey("fill") ? parentStyle["fill"] : "";
        var css = "position: absolute; top: 0px; left: 0px;"; // for some reason, inside reveal, svg is not positioned exactly as its parent div. fix that with absolute 0,0.

        html.AppendLine(@$"<svg viewBox=""{viewBox}"" style=""{css}"" xmlns=""http://www.w3.org/2000/svg"" version=""1.1"">");
        html.AppendLine($"  <path fill=\"{fill}\" d=\"{path}\" />");
        html.AppendLine("</svg>");
    }

    void VisitFrame(StringBuilder html, XElement frame)
    {
        var attrs = ParseAttributes(frame);
        var style = StylesManager.GetElementStyle(frame);
        var css = StylesManager.GetCssFor(frame);

        var firstParagraph = frame.Descendants(Ns.text + "p").FirstOrDefault();
        if (firstParagraph is not null)
        {
            var paragraphStyle = GetTextCss(firstParagraph);
            css = MergeCss(css, WrapCss(paragraphStyle, $".{style.StyleName} "));
        }

        frame.AddAnnotation(css);

        html.OpenTag("div", attrs);

        var styleName = frame.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "style-name")?.Value;
        VisitChildren(html, frame, $".{styleName}");

        html.CloseTag("div");
    }

    void VisitNode(StringBuilder html, XElement el)
    {
        switch (el.Name.LocalName)
        {
            case "frame": VisitFrame(html, el); break;
            case "custom-shape": VisitFrame(html, el); break;
            case "graphics":
            case "g":
                VisitGraphics(html, el); break;
            case "p": VisitParagraph(html, el); break;
            case "image": VisitImg(html, el); break;
            case "notes": SkipNode(html, el); break;
            case "list": VisitList(html, el); break;
            case "list-item": VisitListItem(html, el); break;
            default: VisitUnknown(html, el); break;
        };
    }

    private void PropagateCss(XElement fromChild, XElement toParent, string? preSelector = null)
    {
        var parentCss = toParent.Annotation<Css>() ?? new Css();
        var childCss = fromChild.Annotation<Css>() ?? new Css();

        foreach (var childEntry in childCss)
        {
            var selector = (preSelector is not null ? $"{preSelector} {childEntry.Selector}" : childEntry.Selector).Trim();
            parentCss.Entries.Add(childEntry with { Selector = selector });
        }

        toParent.RemoveAnnotations<Css>();
        toParent.AddAnnotation(parentCss);
    }

    void SkipNode(StringBuilder html, XElement el) { }

    void VisitAndTranslate(StringBuilder html, XElement el, string targetElementName)
    {
        var attrs = ParseAttributes(el);
        html.OpenTag(targetElementName, attrs);

        VisitChildren(html, el);

        html.CloseTag(targetElementName);
    }

    void VisitUnknown(StringBuilder html, XElement el)
    {
        var styleName = el.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "style-name")?.Value;
        if (styleName == null)
        {
            VisitChildren(html, el);
        }
        else
        {
            VisitAndTranslate(html, el, "div");
        }
    }

    void VisitChildren(StringBuilder html, XElement el, string? cssPreSelector = null)
    {
        foreach (var child in el.Elements())
        {
            VisitNode(html, child);

            PropagateCss(child, el, cssPreSelector);
        }
    }

    void VisitList(StringBuilder html, XElement el)
    {
        context.ListLevel++;
        try
        {

            var css = StylesManager.GetCssFor(el, "ul");
            var styleEl = StylesManager.GetStyleElementFor(el);

            var isFinalLevel = el.Descendants(Ns.text + "list").Count() == 0;

            var listCss = StylesManager.GetListCss(styleEl, context.ListLevel, "");

            // sometimes it is a list in xml, but not really a list in PowerPoint - it doesn't have any bullet-char set
            // in that case, just ignore ul/li elements and put the text content
            var isRealList = listCss.Entries.Any(e => e.Properties.ContainsKey("list-style-type") && !string.IsNullOrWhiteSpace(e.Properties["list-style-type"]));
            if (!isRealList)
            {
                var prevListLevel = context.ListLevel;
                context.ListLevel = -1;

                VisitChildren(html, el);

                context.ListLevel = prevListLevel;

                return;
            }
            if (isFinalLevel)
            {
                css = MergeCss(css, listCss);
            }

            el.AddAnnotation(css);

            html.OpenTag("ul");

            VisitChildren(html, el, "ul");

            var parentCss = el.Annotation<Css>();
            var childCss = el.Annotation<Css>();

            var parentPaddingStr = listCss.Entries
                ?.LastOrDefault(c => c.Selector == "ul" && c.Properties.ContainsKey("padding-left"))
                ?.Properties?["padding-left"];
            var childPaddingCss = childCss
                ?.LastOrDefault(c => c.Selector == "ul ul" && c.Properties.ContainsKey("padding-left"));

            if (parentPaddingStr is not null && childPaddingCss is not null)
            {
                var childPaddingStr = childPaddingCss.Properties["padding-left"];

                var (parentPadding, parentUnit) = StylesManager.ParseUnits(parentPaddingStr);
                var (childPadding, childUnit) = StylesManager.ParseUnits(childPaddingStr!);

                // left padding of the inner list includes padding of its parent. we need to substract it
                // technically, we should take it from other list style, each level has its own hierarchy of styles,
                // but that would require another parsing pass
                // from examples, it looks like all list styles are the same for a given slide
                var paddingAdjusted = childPadding - parentPadding;

                childPaddingCss.Properties["padding-left"] = $"{paddingAdjusted.ToString(System.Globalization.CultureInfo.InvariantCulture)}{childUnit}";
            }

            html.CloseTag("ul");
        }
        finally
        {
            context.ListLevel--;
        }
    }

    void VisitListItem(StringBuilder html, XElement el)
    {
        if (context.ListLevel == -1)
        {
            VisitChildren(html, el);
            return;
        }

        var hasInnerListOnly = el.Elements().Count() == 1 && el.Elements().Single().Name.LocalName == "list";
        if (hasInnerListOnly)
        {
            var child = el.Elements().Single();
            VisitList(html, child);
            PropagateCss(child, el);
        }
        else
        {
            VisitAndTranslate(html, el, "li");

            PullAndPushCssProperties(el, el.Parent!);
        }
    }

    private void PullAndPushCssProperties(XElement el, XElement parent)
    {
        var parentCss = parent.Annotation<Css>();
        var childCss = el.Annotation<Css>();

        // TODO: font properties of 'p'
        // TODO: sometimes we don't replace text inside p, but the whole container - migrate text properties from p?


        if (parentCss == null)
        {
            parentCss = new Css();
            parent.AddAnnotation(parentCss);
        }

        PullFromParagraphToListItem(parentCss, childCss);
        PushDownMargins(parentCss, childCss);
    }

    private static void PullFromParagraphToListItem(Css? parentCss, Css? childCss)
    {
        var propsToPull = new string[] {
            "color", "text-align", "line-height", "font-size", "font-family"
        };
        PullProps(parentCss, "ul", childCss, "p", propsToPull);
    }

    private static void PullProps(Css? parentCss, string toParentElement, Css? childCss, string fromChildElement, string[] propsToPull)
    {
        var propsToParent = new Dictionary<string, string>();
        foreach (var prop in propsToPull)
        {
            var valueFromParagraph = childCss
                     ?.LastOrDefault(c => c.Selector == fromChildElement && c.Properties.ContainsKey(prop))
                     ?.Properties?[prop];
            if (!string.IsNullOrWhiteSpace(valueFromParagraph))
            {
                propsToParent[prop] = valueFromParagraph;
            }
        }
        parentCss.Entries.Add(new CssEntry(toParentElement, propsToParent));
    }

    private static void PushDownMargins(Css? parentCss, Css? childCss)
    {
        var paddingLeftStr = parentCss
            ?.LastOrDefault(c => c.Properties.ContainsKey("padding-left"))
            ?.Properties?["padding-left"];
        var marginLeftCss = childCss
            ?.LastOrDefault(c => c.Properties.ContainsKey("margin-left"));

        if (paddingLeftStr is not null && marginLeftCss is not null)
        {
            var marginLeftStr = marginLeftCss.Properties["margin-left"];

            var (paddingLeft, paddungUnit) = StylesManager.ParseUnits(paddingLeftStr);
            var (marginLeft, marginUnit) = StylesManager.ParseUnits(marginLeftStr);

            // left margin of the list item includes padding of its parent. we need to substract it
            var marginAdjusted = marginLeft - paddingLeft;

            marginLeftCss.Properties["margin-left"] = $"{marginAdjusted.ToString(System.Globalization.CultureInfo.InvariantCulture)}{marginUnit}";
        }
    }

    Css GetTextCss(XElement p)
    {
        var spans = p.Descendants(Ns.text + "span");
        var firstSpan = spans.FirstOrDefault();
        if (firstSpan is not null)
        {
            var myCss = StylesManager.GetCssFor(p, "p");
            var spanCss = StylesManager.GetCssFor(firstSpan, "p");
            var merged = MergeCss(myCss, spanCss);

            return merged;
        }

        return new Css();
    }

    void VisitParagraph(StringBuilder html, XElement p)
    {
        var merged = GetTextCss(p);

        // when we replace content of list placeholder, there might be no paragraph inside.
        // But we still want to apply the paragraph style to the list item
        var paragraphCss = new Css();
        foreach (var entry in merged)
        {
            paragraphCss.Entries.Add(new CssEntry("li > *", entry.Properties));
        }

        merged = MergeCss(merged, paragraphCss);

        p.AddAnnotation(merged);

        var attrs = ParseAttributes(p);

        html.OpenTag("p", attrs);

        var spans = p.Descendants(Ns.text + "span");
        foreach (var span in spans) VisitSpan(html, span);

        html.CloseTag("p");
    }

    private void ConvertMarginToPadding(CssEntry entry)
    {
        var keys = new[] { "margin-left" };
        foreach (var key in keys)
        {
            if (entry.Properties.ContainsKey(key))
            {
                var value = entry.Properties[key];
                entry.Properties.Remove(key);
                entry.Properties.Add(key.Replace("margin", "padding"), value);
            }
        }
    }

    void VisitSpan(StringBuilder html, XElement span)
    {
        var attrs = ParseAttributes(span);

        html.OpenTag("span", attrs);

        html.AppendLine(span.Value);

        html.CloseTag("span");
    }

    Dictionary<string, string> ParseAttributes(XElement frame)
    {
        var attrs = new Dictionary<string, string>();
        var style = new Dictionary<string, string>();
        foreach (var attr in frame.Attributes())
        {
            _ = attr.Name.LocalName switch
            {
                "object" => attrs["class"] = attr.Value,
                "x" => style["left"] = StylesManager.ConvertUnits(attr.Value) ?? "",
                "y" => style["top"] = StylesManager.ConvertUnits(attr.Value) ?? "",
                "width" => style["width"] = StylesManager.ConvertUnits(attr.Value) ?? "",
                "height" => style["height"] = StylesManager.ConvertUnits(attr.Value) ?? "",
                "layer" => style["z-index"] = "100",
                "class" => attrs["presentation-class"] = attr.Value,
                "style-name" => attrs["class"] = attr.Value,
                _ => attrs[attr.Name.LocalName] = attr.Value
            };
        }

        if (style.ContainsKey("top") || style.ContainsKey("left"))
        {
            style["position"] = "absolute";
        }
        attrs["style"] = StylesManager.BuildStyleString(style);

        return attrs;
    }

    Dictionary<string, string> ParseAttributesAndStyles(XElement frame)
    {
        var attrs = ParseAttributes(frame);
        var (style, styleName) = StylesManager.GetElementCss(frame);

        var css = attrs.ContainsKey("style") ? attrs["style"] : "";
        css += StylesManager.BuildStyleString(style);
        attrs["style"] = css;
        return attrs;
    }

    void VisitImg(StringBuilder html, XElement img)
    {
        var url = img.Attribute(Ns.xlink + "href")!.Value;
        html.WriteBackgroundDiv(url);
    }
}
public static class HtmlExtensions
{
    public static void WriteBackgroundDiv(this StringBuilder html, string url, string width = "100%", string height = "100%")
    {
        var backgroundSize = url.Contains(".emf") ? "contain" : "auto";
        url = url.Replace(".emf", ".svg");

        html.OpenTag("div", new Dictionary<string, string> { ["style"] = $"background-image: url('{url}'); background-size: {backgroundSize}; width:{width}; height:{height}" });
        html.CloseTag("div");
    }

    public static void OpenTag(this StringBuilder html, string tagName, Dictionary<string, string>? attrs = null)
    {
        html.Append(@$"<{tagName} ");
        if (attrs is not null)
        {
            foreach (var attr in attrs) html.Append($"{attr.Key}=\"{attr.Value}\" ");
        }
        html.AppendLine(">");
    }

    public static void CloseTag(this StringBuilder html, string tagName)
    {
        html.AppendLine($"</{tagName}>");
    }
}

public class Css : IEnumerable<CssEntry>
{
    public List<CssEntry> Entries { get; }

    public Css(params CssEntry[] entries) : this(entries.AsEnumerable()) { }
    public Css(IEnumerable<CssEntry>? entries = null)
    {
        Entries = entries is not null ? new List<CssEntry>(entries) : new List<CssEntry>();
    }

    public Dictionary<string, CssEntry> AsDictionary => Entries.ToDictionary(e => e.Selector, e => e);

    public IEnumerator<CssEntry> GetEnumerator() => Entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();

    public override string ToString()
    {
        var result = new StringBuilder();
        foreach (var cssentry in this)
        {
            result.AppendLine(cssentry.ToString());
        }

        return result.ToString();

    }
}
public record CssEntry
{
    private string selector;

    public string Selector { get => selector; set => selector = value.Trim(); }
    public Dictionary<string, string> Properties { get; set; }
    public CssEntry(string selector, Dictionary<string, string> properties)
    {
        this.Selector = selector;
        this.Properties = properties;
    }

    public CssEntry(string selector, IEnumerable<KeyValuePair<string, string>> properties)
        : this(selector, new Dictionary<string, string>(properties))
    { }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(this.Selector).AppendLine(" {");

        foreach (var prop in Properties) sb.AppendLine($"  {prop.Key}: {prop.Value};");

        sb.AppendLine("}");

        return sb.ToString();
    }

}

public class StylesManager
{
    readonly int dpi = 96; // default dpi for PowerPoint, results in 1280 x 720 resolution
    readonly decimal defaultLineHeight = 1.2M;

    /// px or in
    public string BaseUnits { get; init; } = "px";

    public XElement Xml { get; }

    private Dictionary<string, XElement> _stylesCache = null;
    private Dictionary<string, XElement> Styles
    {
        get
        {
            if (_stylesCache == null)
            {
                _stylesCache = Xml.Descendants().Where(d => d.Attribute(Ns.style + "name") != null)
                    .ToDictionary(el => el.Attribute(Ns.style + "name").Value, el => el);
            }

            return _stylesCache;
        }
    }

    public StylesManager(XElement xml)
    {
        Xml = xml;
    }

    public IEnumerable<XElement> GetAllStylesFor(XElement element)
    {
        return element.DescendantsAndSelf().Select(d => GetStyleElementFor(d)).Where(s => s is not null).Select(d => d!);
    }

    public IEnumerable<XElement> GetAllStyles()
    {
        return Styles.Values;
    }

    public XElement? GetStyleElementFor(XElement parent)
    {
        var styleName = parent.Attributes().FirstOrDefault(a => a.Name.LocalName == "style-name")?.Value;
        if (styleName is null) return null;

        var styleEl = Styles.ContainsKey(styleName) ? Styles[styleName] : null;

        return styleEl;
    }

    public Css GetCssFor(XElement el, string? selector = null)
        => GetCssFromStyleElement(GetStyleElementFor(el), selector);

    public Css GetCssFromStyleElement(XElement? styleEl, string? selector = null)
    {
        if (styleEl is null) return new Css();

        var style = ParseStyle(styleEl);
        selector = selector ?? $".{style.StyleName}";

        var parsedAttributes = ParseStyleAttributes(style.AllAttributes);

        if (!parsedAttributes.Any()) return new Css();

        var attributes = parsedAttributes.Union(new Dictionary<string, string>()
        {
            ["source"] = $"'{style.StyleName}'"
        });
        return new Css(new CssEntry(selector, attributes));
    }

    public Css GetListCss(XElement? styleEl, int listLevel, string? selector = null)
    {
        if (styleEl is null) return new Css();

        List<CssEntry> entries = new();

        var style = ParseStyle(styleEl);
        selector = selector ?? $".{style.StyleName}";

        foreach (var bulletStyle in style.ListStyles)
        {
            var levelStr = bulletStyle.Attribute(Ns.text + "level")?.Value;
            //text:bullet-char="•"
            if (int.TryParse(levelStr, out var level) && level == listLevel)
            {
                // <style:list-level-properties text:space-before="0in" text:min-label-width="0.3125in" />
                // <style:text-properties fo:color="#ffffff" fo:font-family="Arial" fo:font-size="100%" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
                var bulletLevelCss = ParseStyleAttributes(bulletStyle.Attributes());
                var listLevelCss = ParseStyleAttributes(bulletStyle.Element(Ns.style + "list-level-properties")?.Attributes());
                var textProps = ParseStyleAttributes(bulletStyle.Element(Ns.style + "text-properties")?.Attributes());

                var props = bulletLevelCss
                    .Union(listLevelCss)
                    .Union(textProps)
                    .Union(new Dictionary<string, string>()
                    {
                        ["source"] = $"'{style.StyleName} level {level}'"
                    });

                var ul = "ul"; //string.Join(" ", Enumerable.Repeat("ul", level));

                entries.Add(new CssEntry($"{selector} {ul}", new()
                {
                    ["padding-inline-start"] = "0px"
                }));
                entries.Add(new CssEntry($"{selector} {ul}", props));
            }
        }

        return new Css(entries);
    }

    private ElementStyle ParseStyle(XElement styleEl)
    {
        var children = styleEl.Elements();

        var style = new ElementStyle(styleEl.Attribute(Ns.style + "name")!.Value, children);

        var fillImageName = style.DrawingPageProperties?.Attributes()?.FirstOrDefault(a => a.Name.LocalName == "fill-image-name")?.Value;
        if (fillImageName is not null)
        {
            var fillImageEl = Xml.Descendants(Ns.draw + "fill-image")
                .FirstOrDefault(s => s.Attribute(Ns.draw + "name")!.Value == fillImageName);
            if (fillImageEl is not null)
            {
                var backgroundHref = fillImageEl.Attribute(Ns.xlink + "href")?.Value;
                style.DrawingPageProperties!.SetAttributeValue(Ns.draw + "background-image", backgroundHref);
            }
            else
            {
                Console.WriteLine($"WARN: could not find fill-image element with name={fillImageName}");
            }
        }

        return style;
    }

    public ElementStyle GetElementStyle(XElement parent)
    {
        var styleEl = GetStyleElementFor(parent);
        var styleName = parent.Attributes().FirstOrDefault(a => a.Name.LocalName == "style-name")?.Value;

        return styleEl is not null ? ParseStyle(styleEl) : new ElementStyle(styleName);
    }

    public string? ConvertLineHeight(string? value)
    {
        if (value is null) return value;
        if (value.EndsWith("%"))
        {
            var (percentValue, _) = ParseUnits(value);
            return (percentValue / 100M * defaultLineHeight).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        return ConvertUnits(value);
    }

    public string? ConvertUnits(string? value)
    {
        if (value is null) return value;
        if (value.EndsWith("in") && BaseUnits == "px")
        {
            var (inchValue, _) = ParseUnits(value);
            var pixelValue = Math.Round(inchValue * dpi, 0);
            return $"{pixelValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}px";
        }
        if (value.EndsWith("px") && BaseUnits == "in")
        {
            var (pixelValue, _) = ParseUnits(value);
            var inchValue = Math.Round(pixelValue / (decimal)dpi, 2);
            return $"{inchValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}in";
        }

        return value;
    }

    public static (decimal value, string unit) ParseUnits(string? valueWithUnit)
    {
        var match = Regex.Match(valueWithUnit ?? "", "^([0-9.\\-]+)(.*)");
        if (match.Success)
        {
            return (decimal.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), match.Groups[2].Value);
        }
        else
        {
            throw new ArgumentException($"value '{valueWithUnit}' doesn't match expected number format");
        }
    }

    public (Dictionary<string, string> style, string? styleName) GetElementCss(XElement element)
    {
        var style = GetElementStyle(element);
        var css = ParseStyleAttributes(style?.AllAttributes);

        return (css, style?.StyleName);
    }

    public Dictionary<string, string> ParseStyleAttributes(IEnumerable<XAttribute>? attributes)
    {
        var css = new Dictionary<string, string>();
        if (attributes is null) return css;

        foreach (var attr in attributes)
        {
            _ = attr.Name.LocalName switch
            {
                "color" => css["color"] = attr.Value,
                "font-family" => css["font-family"] = attr.Value,
                "font-size" => css["font-size"] = ConvertUnits(attr.Value) ?? "",
                "text-align" => css["text-align"] = attr.Value,
                "line-height" => css["line-height"] = ConvertLineHeight(attr.Value) ?? "",
                "margin-left" => css["margin-left"] = ConvertUnits(attr.Value) ?? "",
                "margin-right" => css["margin-right"] = ConvertUnits(attr.Value) ?? "",
                "margin-top" => css["margin-top"] = ConvertUnits(attr.Value) ?? "",
                "margin-bottom" => css["margin-bottom"] = ConvertUnits(attr.Value) ?? "",
                "padding-left" => css["padding-left"] = ConvertUnits(attr.Value) ?? "",
                "padding-right" => css["padding-right"] = ConvertUnits(attr.Value) ?? "",
                "padding-top" => css["padding-top"] = ConvertUnits(attr.Value) ?? "",
                "padding-bottom" => css["padding-bottom"] = ConvertUnits(attr.Value) ?? "",
                "fill-color" => css["fill"] = attr.Value,
                "bullet-char" => css["list-style-type"] = $"'{attr.Value}'",
                "space-before" => css["padding-left"] = ConvertUnits(attr.Value) ?? "", // padding-inline-start
                "text-transform" => css["text-transform"] = attr.Value,
                _ => _ = "ignore" //css[$"x-{attr.Name.LocalName}"] = attr.Value
                                  // TODO: background-color?
            };

            if (attr.Name.LocalName == "fill-color" && attributes.FirstOrDefault(a => a.Name.LocalName == "fill")?.Value == "solid")
                css["background"] = attr.Value;


            if (attr.Name.LocalName == "textarea-vertical-align" && attr.Value == "middle")
            {
                css["display"] = "flex";
                css["align-items"] = "flex-start";
                css["align-content"] = "flex-start";
                css["flex-direction"] = "column";
                css["flex-wrap"] = "wrap";
                css["justify-content"] = "center";
            }
        }

        return css;
    }

    public static string BuildStyleString(Dictionary<string, string> cssAttrs)
        => cssAttrs.Aggregate("", (s, curr) => s + $"{curr.Key}: {curr.Value}; ");
}

public record ElementStyle(string StyleName, IEnumerable<XElement>? PropertyNodes = null)
{
    public IEnumerable<XElement> PropertyNodes { get; init; } = PropertyNodes ?? Array.Empty<XElement>();

    public XElement? ParagraphProperties => GetPropertyNode("paragraph-properties");
    public XElement? TextProperties => GetPropertyNode("text-properties");
    public XElement? GraphicsProperties => GetPropertyNode("graphic-properties");
    public XElement? DrawingPageProperties => GetPropertyNode("drawing-page-properties");
    public IEnumerable<XElement> ListStyles => PropertyNodes.Where(n => n.Name.LocalName == "list-level-style-bullet");
    public XElement? GetPropertyNode(string key) => PropertyNodes.SingleOrDefault(p => p.Name.LocalName == key);

    public IEnumerable<XAttribute> AllAttributes => PropertyNodes
                .Where(n => n.Name.LocalName != "list-level-style-bullet")
                .SelectMany(p => p.Attributes());
}