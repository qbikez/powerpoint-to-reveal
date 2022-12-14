type PropType<TObj, TProp extends keyof TObj> = TObj[TProp];

export default class LayoutsPlugin {
  id = "layouts";
  private reveal: any;
  public settings = {
    extractBackgrounds: true,
    backgroundGradient: undefined,
    layoutsDir: "html",
    stripUnusedPlaceholders: true,
    footer: "",
  };

  constructor(options: Partial<PropType<LayoutsPlugin, "settings">> = {}) {
    this.settings = {
      ...this.settings,
      ...options,
    };
  }

  init = async (deck) => {
    console.log("my custom plugin!");
    this.reveal = deck;

    deck.on(
      "slidechanged",
      ({ indexh, indexv, previousSlide, currentSlide, origin }) => {
        // TODO: put the footer outside of slides deck, so it doesn't move with slides
        // make sure it dissapears when a slide has no footer
        // make sure it works for the first slide, after load
        console.log("slide changed. footer:");
        console.dir(currentSlide.footer);
      }
    );

    this.addStyle(`
      .layout {
        top: 0px;
        text-align: unset;
      }
    `);

    const slides = deck.getSlides(); // array of section elements

    // const resp = await fetch(
    //   `../${this.settings.layoutsDir}/styles.css`
    // );
    // var css = document.createElement("style");
    // css.setAttribute("type", "text/css");
    // css.innerText = resp.body;
    // document.getElementsByTagName("head")[0].appendChild(css);

    const deckWidth = deck.getConfig().width;
    const deckHeight = deck.getConfig().height;

    for (let slideIdx = 0; slideIdx < slides.length; slideIdx++) {
      const slide = slides[slideIdx] as HTMLElement;
      // check layout class
      const layoutName = slide.dataset.layout;
      if (!layoutName) continue;

      console.log(`processing slide with layout '${layoutName}'`);

      const lastSeparatorIdx = layoutName.lastIndexOf("/");
      const templatePath =
        lastSeparatorIdx > 0 ? layoutName.substring(0, lastSeparatorIdx) : ".";

      if (templatePath != ".") slide.classList.add(templatePath);
      slide.classList.add("layout");

      this.loadCss(`../${this.settings.layoutsDir}/${templatePath}/styles.css`);

      const resp = await fetch(
        `../${this.settings.layoutsDir}/${layoutName}.html`
      );
      let html = await resp.text();
      html = html.replace(
        /(["'])media\//g,
        `$1${this.settings.layoutsDir}/${templatePath}/media/`
      );

      var parser = new DOMParser();
      var doc = parser.parseFromString(html, "text/html");
      const layout = doc.body;

      this.applyLayout({ deckWidth, deckHeight, layout, slide });
      // load layout html file
      // For each child node in slide:
      //   check if layout has placeholder for this node (i.e. {h1})
      //   yes - put the node content into placeholder and remove the original node
      //   no - keep the node content as 'slide text' and remove the original node
      // Put the 'slide text' into it's corresponding placeholder or at the end of slide
      // Replace slide content with processed layout content
    }

    console.log("layouts applied");
  };

  loadCss = (path) => {
    var existing = document.head.querySelector(`link[href="${path}"]`);

    if (existing) return;

    var fileref = document.createElement("link");
    fileref.setAttribute("rel", "stylesheet");
    fileref.setAttribute("type", "text/css");
    fileref.setAttribute("href", path);
    document.head.appendChild(fileref);
  };

  addStyle = (style) => {
    var styleNode = document.createElement("style");
    styleNode.innerText = style;
    document.head.appendChild(styleNode);
  };

  replaceText = (placeholder: Element, source: Element | string) => {
    const firstP = placeholder.querySelector("p");
    const div = placeholder.querySelector("div");
    const isSingle =
      typeof source === "string" ? true : source.children.length == 0;
    const innerHtml =
      typeof source === "string"
        ? source
        : isSingle
        ? source.innerHTML
        : source.outerHTML;

    if (typeof source === "string") {
    } else {
      const sourceNode = source;
      // if it's a single node (i.e. h1), we can put it inside Paragraph
      // TODO: wrap top-level text inside listitems
      const listItems = sourceNode.querySelectorAll("li");
      for (let i = 0; i < listItems.length; i++) {
        const li = listItems[i];
        for (let c = 0; c < li.childNodes.length; c++) {
          const child = li.childNodes[c];
          if (child.nodeName === "#text" && !!child.textContent?.trim()) {
            const spanWrapper = document.createElement("span");
            spanWrapper.setAttribute("style", "display: inline-block");
            li.replaceChild(spanWrapper, child);
            spanWrapper.appendChild(child);
          }
        }
      }
    }
    const targetNode = /*span*/ (isSingle && firstP) || div || placeholder;

    const paragraphStyle = (firstP && firstP.getAttribute("style")) || "";

    const siblings = Array.from(
      placeholder.querySelectorAll(targetNode.nodeName)
    ).filter((s) => s.parentNode == targetNode.parentNode);

    siblings.forEach((s) => {
      if (s != targetNode) {
        s.remove();
      }
    });

    targetNode.innerHTML = innerHtml;

    if (!isSingle) {
      // we put some paragraphs in, but we need to copy the paragraph style from the layout
      // would probably be better to have a CSS rule instead of inline style
      const children = targetNode.querySelectorAll("p,li");

      children.forEach((p) => {
        const curStyle = p.getAttribute("style") || "";
        p.setAttribute("style", paragraphStyle + ";" + curStyle);
      });
    }

    placeholder.setAttribute("replaced", "true");
  };

  matchByOrder(layout: HTMLElement, slide: HTMLElement) {
    const placeholderOrder = ["title", "subtitle", "outline", "object"];
    const placeholderFilter = {
      title: ["h1"],
      subtitle: ["h1", "h2"],
      outline: ["*"],
      object: ["*"],
    };

    const placeholders = placeholderOrder
      .flatMap((value) => {
        return Array.from(
          layout.querySelectorAll(
            `[presentation-class='${value}']:not([replaced])`
          )
        );
      })
      .filter((p) => !!p);

    const replaced: Element[] = [];

    const nextMatchingPlaceholder = (contentEl) => {
      const notReplaced = placeholders.filter((p) => !replaced.includes(p));
      if (!notReplaced.length) {
        console.log("all placeholders were replaced. Nowhere to put element");
        console.log(contentEl);
        return null;
      }

      const matching = notReplaced.filter((p) => {
        const filter = placeholderFilter[p.getAttribute("presentation-class")!];
        return (
          !filter ||
          filter.includes(contentEl.nodeName.toLowerCase()) ||
          filter.includes("*")
        );
      });

      if (!matching.length) {
        console.log("no placeholder matching element");
        console.log(contentEl);
        return null;
      }

      return matching?.length > 0 && matching[0];
      // placeholders
      // where placeholderFilter
      // where not replaced
    };

    let contentOrder: Element[] = [];

    for (let i = 0; i < slide.children.length; i++) {
      const child = slide.children[i];
      contentOrder.push(child);
    }

    contentOrder = contentOrder.sort((a, b) => {
      const priority = (node) => (node.nodeName.startsWith("H") ? 0 : 1);
      return priority(a) - priority(b);
    });

    const match: Array<{ placeholder: Element; content: Element }> = [];

    for (let i = 0; i < contentOrder.length; i++) {
      const contentNode = contentOrder[i];
      const placeholder = nextMatchingPlaceholder(contentNode);

      if (placeholder) {
        replaced.push(placeholder);
        match.push({ placeholder, content: contentNode });
      }
    }

    const unusedPlaceholders = Array.from(
      layout.querySelectorAll(`[presentation-class]:not([replaced])`)
    ).filter((p) => !replaced.includes(p));
    const unusedContent = contentOrder.filter(
      (c) => match.filter((m) => m.content == c).length == 0
    );

    return { match, unusedPlaceholders, unusedContent };
  }

  matchByPosition(layout: HTMLElement, slide: HTMLElement) {
    const pixelValue = (v: string) =>
      parseFloat(/([0-9\.]+)/.exec(v)?.[0] || "");
    const byLeft = (a: HTMLElement, b: HTMLElement) =>
      pixelValue(a.style.left) + pixelValue(a.style.width) <=
      pixelValue(b.style.left)
        ? -1
        : pixelValue(b.style.left) + pixelValue(b.style.width) <=
          pixelValue(a.style.left)
        ? 1
        : 0;
    const byTop = (a: HTMLElement, b: HTMLElement) =>
      pixelValue(a.style.top) + pixelValue(a.style.height) <=
      pixelValue(b.style.top)
        ? -1
        : pixelValue(b.style.top) + pixelValue(b.style.height) <=
          pixelValue(a.style.top)
        ? 1
        : 0;

    const placeholderFilters = ["title", "subtitle", "outline", "object"];
    const placeholders = (
      Array.from(
        layout.querySelectorAll("[presentation-class]")
      ) as HTMLElement[]
    )
      .filter((p) =>
        placeholderFilters.includes(p.getAttribute("presentation-class")!)
      )
      .sort(byTop)
      .sort(byLeft);

    const replaced: Element[] = [];
    const nextMatchingPlaceholder = (contentEl: Element) => {
      const notReplaced = placeholders.filter((p) => !replaced.includes(p));
      if (!notReplaced.length) {
        console.log("all placeholders were replaced. Nowhere to put element");
        console.log(contentEl);
        return null;
      }
      return notReplaced[0];
    };

    const contentOrder = Array.from(slide.children);
    const match: Array<{ placeholder: Element; content: Element }> = [];

    for (let i = 0; i < contentOrder.length; i++) {
      const contentNode = contentOrder[i];
      const placeholder = nextMatchingPlaceholder(contentNode);

      if (placeholder) {
        replaced.push(placeholder);
        match.push({ placeholder, content: contentNode });
      }
    }

    const unusedPlaceholders = Array.from(
      layout.querySelectorAll(`[presentation-class]:not([replaced])`)
    ).filter((p) => !replaced.includes(p));
    const unusedContent = contentOrder.filter(
      (c) => match.filter((m) => m.content == c).length == 0
    );

    return { match, unusedPlaceholders, unusedContent };
  }

  applyLayout({
    deckWidth,
    deckHeight,
    layout,
    slide,
  }: {
    deckWidth: number;
    deckHeight: number;
    layout: HTMLElement;
    slide: HTMLElement;
  }) {
    if (this.settings.extractBackgrounds) {
      this.processBackground({ slide, layout, deckWidth, deckHeight });
    }
    if (this.settings.backgroundGradient) {
      slide.setAttribute(
        "data-background-gradient",
        this.settings.backgroundGradient
      );
    }

    const footer = layout.querySelector("[presentation-class='footer']");

    if (footer) {
      (slide as any).footer = footer;
      this.replaceText(footer, this.settings.footer);
    }

    const pageNumber = layout.querySelector(
      "[presentation-class='page-number']"
    );
    if (pageNumber) {
      const slideNo = this.getSlideNumber(slide);
      this.replaceText(pageNumber, slideNo);
    }

    const matchFunction = this.matchByPosition;
    const { match, unusedPlaceholders, unusedContent } = matchFunction(
      layout,
      slide
    );

    match.forEach(({ placeholder, content }) => {
      this.replaceText(placeholder, content);
      content.remove();
    });

    unusedContent.forEach((c) => {
      c.remove();
    });

    if (this.settings.stripUnusedPlaceholders) {
      unusedPlaceholders.forEach((p) => {
        // TODO: maybe remove only visible content and leave the placeholder elements for debugging?
        p.remove();
      });
    }

    slide.innerHTML = layout.innerHTML;

    return slide;
  }

  getSlideNumber(slide = this.reveal.getCurrentSlide()) {
    let config = this.reveal.getConfig();
    let value;
    let format = "h.v";

    if (typeof config.slideNumber === "function") {
      value = config.slideNumber(slide);
    } else {
      // Check if a custom number format is available
      if (typeof config.slideNumber === "string") {
        format = config.slideNumber;
      }

      // If there are ONLY vertical slides in this deck, always use
      // a flattened slide number
      if (!/c/.test(format) && this.reveal.getHorizontalSlides().length === 1) {
        format = "c";
      }

      // Offset the current slide number by 1 to make it 1-indexed
      let horizontalOffset =
        slide && slide.dataset.visibility === "uncounted" ? 0 : 1;

      value = [];
      switch (format) {
        case "c":
          value.push(this.reveal.getSlidePastCount(slide) + horizontalOffset);
          break;
        case "c/t":
          value.push(
            this.reveal.getSlidePastCount(slide) + horizontalOffset,
            "/",
            this.reveal.getTotalSlides()
          );
          break;
        default:
          let indices = this.reveal.getIndices(slide);
          value.push(indices.h + horizontalOffset);
          let sep = format === "h/v" ? "/" : ".";
          if (this.reveal.isVerticalSlide(slide))
            value.push(sep, indices.v + 1);
      }
    }

    let url = "#" + this.reveal.location.getHash(slide);
    return this.formatNumber(value[0], value[1], value[2], url);
  }

  formatNumber(a, delimiter, b, url = "#" + this.reveal.location.getHash()) {
    if (typeof b === "number" && !isNaN(b)) {
      return `
					<span class="slide-number-a">${a}</span>
					<span class="slide-number-delimiter">${delimiter}</span>
					<span class="slide-number-b">${b}</span>
					`;
    } else {
      return `
					<span class="slide-number-a">${a}</span>
					`;
    }
  }

  processBackground({
    layout,
    deckWidth,
    deckHeight,
    slide,
  }: {
    layout: HTMLElement;
    deckWidth: number;
    deckHeight: number;
    slide: HTMLElement;
  }) {
    const backgrouds = Array.from(layout.querySelectorAll("div")).filter(
      (div) =>
        div.style.backgroundImage &&
        parseInt(div.parentElement!.style.width) >= deckWidth &&
        parseInt(div.parentElement!.style.height) >= deckHeight
    );
    if (backgrouds) {
      var selectedBackground = backgrouds.find((b) =>
        b.style.backgroundImage.includes(".png")
      );

      if (selectedBackground) {
        var url = selectedBackground.style.backgroundImage
          .replace('url("', "")
          .replace('")', "");
        slide.setAttribute("data-background-image", url);
      }

      //deck.syncSlide(slide);
      backgrouds.forEach((b) => {
        b.remove();
      });
    }
  }
}
