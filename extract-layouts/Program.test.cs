using System.Xml.Linq;
using FluentAssertions;
using Xunit;
using Snapper;
using Snapper.Attributes;
using System.Xml.XPath;
using FluentAssertions.Collections;

public class StylesManagerTests
{
    [Fact]
    public void gets_element_style()
    {
        var styleXml = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <office:styles>
        <style:style style:family="text" style:name="a2">
            <style:text-properties fo:text-transform="uppercase" fo:color="#ffffff" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-family="Calibri Light" fo:font-size="0.5in" style:font-size-asian="0.5in" style:font-size-complex="0.5in" fo:letter-spacing="0in" fo:language="en" fo:country="US" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="true"/>
        </style:style>
        <style:style style:family="text" style:name="a3">
            <style:text-properties fo:text-transform="uppercase" fo:color="#ffffff" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-family="Calibri Light" fo:font-size="0.5in" style:font-size-asian="0.5in" style:font-size-complex="0.5in" fo:letter-spacing="0in" fo:language="en" fo:country="US" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="true"/>
        </style:style>
        <style:style style:family="paragraph" style:name="a4">
            <style:paragraph-properties fo:line-height="100%" fo:text-align="left" style:tab-stop-distance="0.5in" fo:margin-left="0in" fo:margin-right="0in" fo:text-indent="0in" fo:margin-top="0in" fo:margin-bottom="0in" style:punctuation-wrap="hanging" style:vertical-align="auto" style:writing-mode="lr-tb">
                <style:tab-stops/>
            </style:paragraph-properties>
            <style:text-properties fo:font-variant="normal" fo:text-transform="none" fo:color="#000000" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-size="0.25in" style:font-size-asian="0.25in" style:font-size-complex="0.25in" fo:letter-spacing="0in" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="false"/>
        </style:style>
        <style:style style:family="presentation" style:name="a5">
            <style:graphic-properties fo:wrap-option="wrap" fo:padding-top="0.05in" fo:padding-bottom="0.05in" fo:padding-left="0.1in" fo:padding-right="0.1in" draw:textarea-vertical-align="middle" draw:textarea-horizontal-align="left" draw:fill="none" draw:stroke="none" draw:auto-grow-width="false" draw:auto-grow-height="false" style:shrink-to-fit="true"/>
            <style:paragraph-properties style:font-independent-line-spacing="true" style:writing-mode="lr-tb"/>
        </style:style>
        </office:styles>
        </office:document-styles>
        """);

        var document = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <draw:frame draw:id="id0" presentation:style-name="a5" draw:name="Title Placeholder 1" svg:x="0.75in" svg:y="0.66667in" svg:width="11.07986in" svg:height="1.59259in" presentation:class="title" presentation:placeholder="false">
            <draw:text-box>
                <text:p text:style-name="a4" text:class-names="" text:cond-style-name="">
                    <text:span text:style-name="a2" text:class-names="">Click to edit Master title style</text:span>
                    <text:span text:style-name="a3" text:class-names=""/>
                </text:p>
            </draw:text-box>
            <svg:title/>
            <svg:desc/>
        </draw:frame>
        </office:document-styles>
        """);

        var stylesMgr = new StylesManager(styleXml);

        var style = stylesMgr.GetElementStyle(document.Elements().First())!;

        style.Should().NotBeNull();
        style.StyleName.Should().Be("a5");

        style.GraphicsProperties.Should().NotBeNull();
        style.ParagraphProperties.Should().NotBeNull();
        style.TextProperties.Should().BeNull();
    }

    [Fact]
    public void gets_master_page_background()
    {
        var styleXml = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <office:styles>
            <draw:fill-image draw:name="a416" xlink:href="media/image2.jpeg" xlink:show="embed" xlink:actuate="onLoad"/>
            <style:style style:family="drawing-page" style:name="a417">
                <style:drawing-page-properties draw:fill="bitmap" draw:fill-image-name="a416" style:repeat="stretch" presentation:visibility="visible" draw:background-size="border" presentation:background-objects-visible="true" presentation:background-visible="true" presentation:display-header="false" presentation:display-footer="false" presentation:display-page-number="false" presentation:display-date-time="false"/>
            </style:style>
        </office:styles>
        </office:document-styles>
        """);

        var document = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
            <style:master-page style:name="Master1-Layout6-titleOnly-Title-Only" style:page-layout-name="pageLayout1" draw:style-name="a417">
            </style:master-page>
        </office:document-styles>
        """);

        var stylesMgr = new StylesManager(styleXml);

        var style = stylesMgr.GetElementStyle(document.Elements().First())!;

        style.Should().NotBeNull();
        style.DrawingPageProperties.Should().NotBeNull();
        style.AllAttributes.Should().Contain(a => a.Name.LocalName == "background-image");
    }

    [Fact]
    public void list_style_to_css()
    {
        var styleXml = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <office:styles>
        <text:list-style style:name="a8" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
            <text:list-level-style-bullet text:level="1" text:bullet-char="•">
                <style:list-level-properties text:space-before="0in" text:min-label-width="0.3125in" />
                <style:text-properties fo:color="#ffffff" fo:font-family="Arial" fo:font-size="100%" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
            </text:list-level-style-bullet>
            <text:list-level-style-bullet text:level="2" text:bullet-char="•">
                <style:list-level-properties text:space-before="0.5in" text:min-label-width="0.3125in" />
                <style:text-properties fo:color="#ffffff" fo:font-family="Arial" fo:font-size="100%" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
            </text:list-level-style-bullet>
        </text:list-style>
        </office:styles>
        </office:document-styles>
        """);

        var stylesMgr = new StylesManager(styleXml);
        var allStyles = stylesMgr.GetAllStyles();

        allStyles.Should().HaveCount(1);

        var level1 = stylesMgr.GetListCss(allStyles.First(), 1, "");

        level1.Entries.Should().Contain(entry => entry.Selector == "ul");

        level1.Entries.Should().NotContain(entry => entry.Selector == "ul ul");
        level1.Entries.Should().NotContain(entry => entry.Selector.StartsWith(".a8"));

        var level2 = stylesMgr.GetListCss(allStyles.First(), 2, "");
        // on each level, we produce a single "ul" selector, which is the wrapped by html processing
        level1.Entries.Should().Contain(entry => entry.Selector == "ul");

        var l2entry = level1.Entries.Last(entry => entry.Selector == "ul");
        l2entry.Properties.Should().Contain(p => p.Key == "list-style-type");
        l2entry.Properties["list-style-type"].Should().Be("'•'");
    }
}

[UpdateSnapshots]
public class PageProcessorTests
{
    [Fact]
    public void moves_styles_to_correct_node()
    {
        var styleXml = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <office:styles>
        <style:style style:family="text" style:name="a2">
            <style:text-properties fo:text-transform="uppercase" fo:color="#ffffff" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-family="Calibri Light" fo:font-size="0.5in" style:font-size-asian="0.5in" style:font-size-complex="0.5in" fo:letter-spacing="0in" fo:language="en" fo:country="US" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="true"/>
        </style:style>
        <style:style style:family="text" style:name="a3">
            <style:text-properties fo:text-transform="uppercase" fo:color="#ffffff" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-family="Calibri Light" fo:font-size="0.5in" style:font-size-asian="0.5in" style:font-size-complex="0.5in" fo:letter-spacing="0in" fo:language="en" fo:country="US" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="true"/>
        </style:style>
        <style:style style:family="paragraph" style:name="a4">
            <style:paragraph-properties fo:line-height="100%" fo:text-align="left" style:tab-stop-distance="0.5in" fo:margin-left="0in" fo:margin-right="0in" fo:text-indent="0in" fo:margin-top="0in" fo:margin-bottom="0in" style:punctuation-wrap="hanging" style:vertical-align="auto" style:writing-mode="lr-tb">
                <style:tab-stops/>
            </style:paragraph-properties>
            <style:text-properties fo:font-variant="normal" fo:text-transform="none" fo:color="#000000" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-size="0.25in" style:font-size-asian="0.25in" style:font-size-complex="0.25in" fo:letter-spacing="0in" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="false"/>
        </style:style>
        <style:style style:family="presentation" style:name="a5">
            <style:graphic-properties fo:wrap-option="wrap" fo:padding-top="0.05in" fo:padding-bottom="0.05in" fo:padding-left="0.1in" fo:padding-right="0.1in" draw:textarea-vertical-align="middle" draw:textarea-horizontal-align="left" draw:fill="none" draw:stroke="none" draw:auto-grow-width="false" draw:auto-grow-height="false" style:shrink-to-fit="true"/>
            <style:paragraph-properties style:font-independent-line-spacing="true" style:writing-mode="lr-tb"/>
        </style:style>
        </office:styles>
        </office:document-styles>
        """);

        var document = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <style:master-page style:name="Master1-Layout2-obj-Title-and-Content" style:page-layout-name="pageLayout1" draw:style-name="a121" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
        <draw:frame draw:id="id0" presentation:style-name="a5" draw:name="Title Placeholder 1" svg:x="0.75in" svg:y="0.66667in" svg:width="11.07986in" svg:height="1.59259in" presentation:class="title" presentation:placeholder="false">
            <draw:text-box>
                <text:p text:style-name="a4" text:class-names="" text:cond-style-name="">
                    <text:span text:style-name="a2" text:class-names="">Click to edit Master title style</text:span>
                    <text:span text:style-name="a3" text:class-names=""/>
                </text:p>
            </draw:text-box>
            <svg:title/>
            <svg:desc/>
        </draw:frame>
        </style:master-page>
        </office:document-styles>
        """);

        var stylesMgr = new StylesManager(styleXml) { BaseUnits = "in" };
        var pageProcessor = new PageProcessor(document, document.Element(Ns.style + "master-page"), stylesMgr, "template-1");

        var (html, css) = pageProcessor.Process();

        var expectedEntries = new Dictionary<string, Dictionary<string, string>>()
        {
            [".template-1 .a5"] = new()
            {
                ["padding-top"] = "0.05in", // this is easy, the style from a5 class
                ["padding-left"] = "0.1in"
            },
            [".template-1 .a5 p"] = new()
            {
                ["margin-left"] = "0in", // extracted from a4 (paragraph)
                ["color"] = "#ffffff" // extracted from a2 (first span)
            }
        };

        css.Should().Contain(expectedEntries);
    }

    [Fact]
    public void list_to_css()
    {
        var styleXml = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <office:styles>        
        <style:style style:family="presentation" style:name="a22" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
            <style:graphic-properties fo:wrap-option="wrap" fo:padding-top="0.05in" fo:padding-bottom="0.05in" fo:padding-left="0.1in" fo:padding-right="0.1in" draw:textarea-vertical-align="middle" draw:textarea-horizontal-align="left" draw:fill="none" draw:stroke="none" draw:auto-grow-width="false" draw:auto-grow-height="false" style:shrink-to-fit="true" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
            <style:paragraph-properties style:font-independent-line-spacing="true" style:writing-mode="lr-tb" />
        </style:style>
        <style:style style:family="paragraph" style:name="a7" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
            <style:paragraph-properties fo:line-height="100%" fo:text-align="left" style:tab-stop-distance="0.5in" fo:margin-left="0.3125in" fo:margin-right="0in" fo:text-indent="-0.3125in" fo:margin-top="0in" fo:margin-bottom="0.13889in" style:punctuation-wrap="hanging" style:vertical-align="auto" style:writing-mode="lr-tb" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0">
                <style:tab-stops />
            </style:paragraph-properties>
            <style:text-properties fo:font-variant="normal" fo:text-transform="none" fo:color="#000000" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-size="0.25in" style:font-size-asian="0.25in" style:font-size-complex="0.25in" fo:letter-spacing="0in" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="false" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
        </style:style>
        <style:style style:family="text" style:name="a6" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
            <style:text-properties fo:font-variant="normal" fo:text-transform="none" fo:color="#ffffff" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-family="Calibri" fo:font-size="0.25in" style:font-size-asian="0.25in" style:font-size-complex="0.25in" fo:letter-spacing="0in" fo:language="en" fo:country="US" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="true" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
        </style:style>
        <text:list-style style:name="a8" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
            <text:list-level-style-bullet text:level="1" text:bullet-char="a8l1">
                <style:list-level-properties text:space-before="0in" text:min-label-width="0.3125in" />
                <style:text-properties fo:color="#ffffff" fo:font-family="Arial" fo:font-size="100%" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
            </text:list-level-style-bullet>
            <text:list-level-style-bullet text:level="2" text:bullet-char="a8l2">
                <style:list-level-properties text:space-before="0.5in" text:min-label-width="0.3125in" />
                <style:text-properties fo:color="#ffffff" fo:font-family="Arial" fo:font-size="100%" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
            </text:list-level-style-bullet>
            <text:list-level-style-bullet text:level="3" text:bullet-char="a8l3">
                <style:list-level-properties text:space-before="1in" text:min-label-width="0.3125in" />
                <style:text-properties fo:color="#ffffff" fo:font-family="Arial" fo:font-size="100%" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
            </text:list-level-style-bullet>
        </text:list-style>
        <text:list-style style:name="a11" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
            <text:list-level-style-bullet text:level="1" text:bullet-char="a11l1">
                <style:list-level-properties text:space-before="0in" text:min-label-width="0.3125in" />
            </text:list-level-style-bullet>
            <text:list-level-style-bullet text:level="2" text:bullet-char="a11l2">
                <style:list-level-properties text:space-before="0.5in" text:min-label-width="0.3125in" />
            </text:list-level-style-bullet>
            <text:list-level-style-bullet text:level="3" text:bullet-char="a11l3">
                <style:list-level-properties text:space-before="1in" text:min-label-width="0.3125in" />
            </text:list-level-style-bullet>
        </text:list-style>
        <style:style style:family="paragraph" style:name="a10" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
            <style:paragraph-properties fo:line-height="100%" fo:text-align="left" style:tab-stop-distance="0.5in" fo:margin-left="0.8125in" fo:margin-right="0in" fo:text-indent="-0.3125in" fo:margin-top="0in" fo:margin-bottom="0.13889in" style:punctuation-wrap="hanging" style:vertical-align="auto" style:writing-mode="lr-tb" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0">
                <style:tab-stops />
            </style:paragraph-properties>
            <style:text-properties fo:font-variant="normal" fo:text-transform="none" fo:color="#000000" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-size="0.25in" style:font-size-asian="0.25in" style:font-size-complex="0.25in" fo:letter-spacing="0in" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="false" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
        </style:style>
        <style:style style:family="text" style:name="a9" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
            <style:text-properties fo:font-variant="normal" fo:text-transform="none" fo:color="#ffaaff" style:text-line-through-type="none" style:text-line-through-style="none" style:text-line-through-width="auto" style:text-line-through-color="font-color" style:text-position="0% 100%" fo:font-family="Calibri" fo:font-size="0.22222in" style:font-size-asian="0.22222in" style:font-size-complex="0.22222in" fo:letter-spacing="0in" fo:language="en" fo:country="US" fo:font-style="normal" style:font-style-asian="normal" style:font-style-complex="normal" style:text-underline-type="none" style:text-underline-style="none" style:text-underline-width="auto" style:text-underline-color="font-color" fo:font-weight="normal" style:font-weight-asian="normal" style:font-weight-complex="normal" style:text-underline-mode="continuous" style:letter-kerning="true" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" />
        </style:style>

        </office:styles>
        </office:document-styles>
        """);

        var document = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
        <style:master-page style:name="Master1-Layout2-obj-Title-and-Content" style:page-layout-name="pageLayout1" draw:style-name="a121" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
        <draw:frame draw:id="id1" presentation:style-name="a22" draw:name="Text Placeholder 2" svg:x="0.75in" svg:y="2.34259in" svg:width="11.07986in" svg:height="3.99074in" presentation:class="outline" presentation:placeholder="false" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0">
        <draw:text-box>
                <text:list text:style-name="a8" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
                <text:list-item>
                    <text:p text:style-name="a7" text:class-names="" text:cond-style-name="">
                        <text:span text:style-name="a6" text:class-names="">Click to edit Master text styles</text:span>
                    </text:p>
                </text:list-item>
            </text:list>
            <text:list text:style-name="a11" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
                <text:list-item>
                    <text:list text:style-name="a11">
                        <text:list-item>
                            <text:p text:style-name="a10" text:class-names="" text:cond-style-name="">
                                <text:span text:style-name="a9" text:class-names="">Second level</text:span>
                            </text:p>
                        </text:list-item>
                    </text:list>
                </text:list-item>
            </text:list>
        </draw:text-box>
        </draw:frame>
        </style:master-page>
        </office:document-styles>
        """);

        var stylesMgr = new StylesManager(styleXml) { BaseUnits = "in" };
        var pageProcessor = new PageProcessor(document, document.Element(Ns.style + "master-page"), stylesMgr, "template-1");

        var (html, css) = pageProcessor.Process();

        var expectedEntries = new Dictionary<string, Dictionary<string, string>>()
        {
            [".template-1 .a22 ul"] = new()
            {
                ["list-style-type"] = "'a8l1'",
                ["color"] = "#ffffff"
            },
            [".template-1 .a22 ul ul"] = new()
            {
                ["list-style-type"] = "'a11l2'",
                ["color"] = "#ffaaff"
            },
            [".template-1 .a22 ul p"] = new()
            {

            },
            [".template-1 .a22 ul ul p"] = new()
            {

            }
        };

        css.Should().Contain(expectedEntries);

        var notExpectedEntries = new Dictionary<string, Dictionary<string, string>>()
        {
            [".template-1 .a22 ul"] = new()
            {
                ["list-style-type"] = "'a11l1'",
            },
            [".template-1 .a22 ul ul"] = new()
            {
                ["list-style-type"] = "'a8l2'",
            }
        };

        css.Should().NotContain(notExpectedEntries);
    }

    [Fact]
    public void fake_lists_are_stripped_out()
    {
        var document = XElement.Parse("""
        <office:document-styles xmlns:dom="http://www.w3.org/2001/xml-events" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" xmlns:presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" xmlns:smil="urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.3">
          <draw:text-box>
            <text:list text:style-name="a11180" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
                <text:list-item>
                    <text:p text:style-name="a11179" text:class-names="" text:cond-style-name="">
                        <text:span text:style-name="a11178" text:class-names="">Lorem Ipsum</text:span>
                    </text:p>
                </text:list-item>
             </text:list>
           </draw:text-box>    
        </office:document-styles>
        """);

        var stylesMgr = new StylesManager(new XElement("styles")) { BaseUnits = "in" };
        var pageProcessor = new PageProcessor(document, document.Elements().First(), stylesMgr, "template-1");

        var (builder, css) = pageProcessor.Process();

        var html = XElement.Parse(builder.ToString());

        html.ShouldMatchSnapshot();
        html.Descendants("ul").Should().BeEmpty();
        var span = html.XPathSelectElements("/body/p/span").Single();
        span.Value.Trim().Should().Be("Lorem Ipsum");
    }

    [Fact]
    public void converts_graphics_to_svg()
    {
        var document = XElement.Parse("""
         <?xml version="1.0" encoding="utf-8"?>
            <style:master-page style:name="Master1-Layout6-cust-Front-Page---Dark" style:page-layout-name="pageLayout1" draw:style-name="a412" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0">
                <draw:g draw:name="Graphic 5" draw:id="id79">
                    <svg:title xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" />
                    <svg:desc xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" />
                    <draw:custom-shape svg:x="2.04695in" svg:y="1.61571in" svg:width="0.31495in" svg:height="0.31349in" draw:id="id80" draw:style-name="a428" draw:name="Freeform 9" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0">
                        <svg:title />
                        <svg:desc />
                        <text:p text:style-name="a427" text:class-names="" text:cond-style-name="" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
                            <text:span text:style-name="a426" text:class-names="" />
                        </text:p>
                        <draw:enhanced-geometry draw:type="non-primitive" svg:viewBox="0 0 287990 286654" xmlns:dr3d="urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0" draw:enhanced-path="M 23905 287461 L 30011 287461 C 37183 287459 44347 286986 51456 286047 L 56755 285321 55392 280199 C 41566 232326 44719 181175 64320 135343 82118 96740 118251 53169 166249 46901 169045 46555 171861 46402 174678 46442 200934 46749 224879 61447 236941 84662 247559 107932 248803 134369 240416 158524 L 239629 161295 241760 163206 C 248056 168956 254054 175022 259731 181380 L 265088 187361 267718 179717 C 281444 145306 281196 106932 267027 72699 248322 34803 208525 11851 166192 14547 115160 17796 77088 58844 63034 76350 32412 114716 15329 162048 14421 211039 14120 235588 17004 260073 23003 283887 Z N" draw:text-areas="?f86 ?f88 ?f87 ?f89" draw:glue-points="?f47 ?f48 ?f49 ?f48 ?f50 ?f51 ?f52 ?f53 ?f54 ?f55 ?f56 ?f57 ?f58 ?f59 ?f60 ?f61 ?f62 ?f63 ?f64 ?f65 ?f66 ?f67 ?f68 ?f69 ?f70 ?f71 ?f72 ?f73 ?f74 ?f75 ?f76 ?f77 ?f78 ?f79 ?f80 ?f81 ?f82 ?f83 ?f84 ?f85" draw:glue-point-leaving-directions="-90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90, -90">
                        </draw:enhanced-geometry>
                    </draw:custom-shape>
                </draw:g>
            </style:master-page>
         """);

        var stylesMgr = new StylesManager(new XElement("styles")) { BaseUnits = "in" };
        var pageProcessor = new PageProcessor(document, document.Elements().First(), stylesMgr, "template-1");

        var (builder, css) = pageProcessor.Process();

        var html = XElement.Parse(builder.ToString());
        builder.ToString().Should().Contain("svg");

    }
}
public static class ShouldExtensions
{
    public static void Contain(this GenericCollectionAssertions<CssEntry> assertions, Dictionary<string, Dictionary<string, string>> expectedEntries)
    {
        foreach (var style in expectedEntries)
        {
            assertions.Contain(e => e.Selector == style.Key, because: style.Key);
            var entries = assertions.Subject;

            foreach (var kvp in style.Value)
            {
                entries.Should().Contain(e => e.Properties.ContainsKey(kvp.Key) && e.Properties[kvp.Key] == kvp.Value,
                    because: $"\nstyle for {style.Key} should contain {kvp.Key}={kvp.Value}"
                );
            }
        }
    }

    public static void NotContain(this GenericCollectionAssertions<CssEntry> assertions, Dictionary<string, Dictionary<string, string>> notExpectedEntries)
    {
        foreach (var style in notExpectedEntries)
        {
            assertions.Contain(e => e.Selector == style.Key, because: style.Key);
            var entries = assertions.Subject;

            foreach (var kvp in style.Value)
            {
                entries.Should().NotContain(e => e.Properties.ContainsKey(kvp.Key) && e.Properties[kvp.Key] == kvp.Value,
                    because: $"\nstyle for {style.Key} should NOT contain {kvp.Key}={kvp.Value}\n"
                );
            }
        }
    }
}