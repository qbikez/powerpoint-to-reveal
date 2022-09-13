class LayoutsPlugin {
  id = "layouts";
  settings = {
      processBackgrounds: false,
      backgroundGradient: undefined,
      layoutsDir: "html",
      stripUnusedPlaceholders: true,
  };
  constructor(options = {}) {
      this.settings = {
          ...this.settings,
          ...options,
      };
  }
  init = async (deck) => {
      console.log("my custom plugin!");
      deck.on("slidechanged", ({ indexh, indexv, previousSlide, currentSlide, origin }) => {
          // TODO: put the footer outside of slides deck, so it doesn't move with slides
          // make sure it dissapears when a slide has no footer
          // make sure it works for the first slide, after load
          console.log("slide changed. footer:");
          console.dir(currentSlide.footer);
      });
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
          const slide = slides[slideIdx];
          // check layout class
          const layoutName = slide.dataset.layout;
          if (!layoutName)
              return;
          console.log(`processing slide with layout '${layoutName}'`);
          const lastSeparatorIdx = layoutName.lastIndexOf("/");
          const templatePath = lastSeparatorIdx > 0 ? layoutName.substring(0, lastSeparatorIdx) : ".";
          if (templatePath != ".")
              slide.classList.add(templatePath);
          slide.classList.add("layout");
          this.loadCss(`../${this.settings.layoutsDir}/${templatePath}/styles.css`);
          const resp = await fetch(`../${this.settings.layoutsDir}/${layoutName}.html`);
          let html = await resp.text();
          html = html.replace(/(["'])media\//g, `$1${this.settings.layoutsDir}/${templatePath}/media/`);
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
      if (existing)
          return;
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
  replaceText = (placeholder, sourceNode) => {
      const isSingle = sourceNode.children.length == 0;
      const span = placeholder.querySelector("span");
      const firstP = placeholder.querySelector("p");
      const div = placeholder.querySelector("div");
      // if it's a single node (i.e. h1), we can put it inside Paragraph
      // TODO: wrap top-level text inside listitems
      const targetNode = /*span*/ (isSingle && firstP) || div || placeholder;
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
      const paragraphStyle = (firstP && firstP.getAttribute("style")) || "";
      const siblings = Array.from(placeholder.querySelectorAll(targetNode.nodeName)).filter((s) => s.parentNode == targetNode.parentNode);
      siblings.forEach((s) => {
          if (s != targetNode) {
              s.remove();
          }
      });
      targetNode.innerHTML = isSingle
          ? sourceNode.innerHTML
          : sourceNode.outerHTML;
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
  matchByOrder(layout, slide) {
      const placeholderOrder = ["title", "subtitle", "outline", "object"];
      const placeholderFilter = {
          title: ["h1"],
          subtitle: ["h1", "h2"],
          outline: ["*"],
          object: ["*"],
      };
      const placeholders = placeholderOrder
          .flatMap((value) => {
          return Array.from(layout.querySelectorAll(`[presentation-class='${value}']:not([replaced])`));
      })
          .filter((p) => !!p);
      const replaced = [];
      const nextMatchingPlaceholder = (contentEl) => {
          const notReplaced = placeholders.filter((p) => !replaced.includes(p));
          if (!notReplaced.length) {
              console.log("all placeholders were replaced. Nowhere to put element");
              console.log(contentEl);
              return null;
          }
          const matching = notReplaced.filter((p) => {
              const filter = placeholderFilter[p.getAttribute("presentation-class")];
              return (!filter ||
                  filter.includes(contentEl.nodeName.toLowerCase()) ||
                  filter.includes("*"));
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
      let contentOrder = [];
      for (let i = 0; i < slide.children.length; i++) {
          const child = slide.children[i];
          contentOrder.push(child);
      }
      contentOrder = contentOrder.sort((a, b) => {
          const priority = (node) => (node.nodeName.startsWith("H") ? 0 : 1);
          return priority(a) - priority(b);
      });
      const match = [];
      for (let i = 0; i < contentOrder.length; i++) {
          const contentNode = contentOrder[i];
          const placeholder = nextMatchingPlaceholder(contentNode);
          if (placeholder) {
              replaced.push(placeholder);
              match.push({ placeholder, content: contentNode });
          }
      }
      const unusedPlaceholders = placeholders.filter((p) => !replaced.includes(p));
      const unusedContent = contentOrder.filter((c) => match.filter((m) => m.content == c).length == 0);
      return { match, unusedPlaceholders, unusedContent };
  }
  matchByPosition(layout, slide) {
      const pixelValue = (v) => parseFloat(/([0-9\.]+)/.exec(v)?.[0] || "");
      const byLeft = (a, b) => pixelValue(a.style.left) + pixelValue(a.style.width) <=
          pixelValue(b.style.left)
          ? -1
          : pixelValue(b.style.left) + pixelValue(b.style.width) <=
              pixelValue(a.style.left)
              ? 1
              : 0;
      const byTop = (a, b) => pixelValue(a.style.top) + pixelValue(a.style.height) <=
          pixelValue(b.style.top)
          ? -1
          : pixelValue(b.style.top) + pixelValue(b.style.height) <=
              pixelValue(a.style.top)
              ? 1
              : 0;
      const placeholderFilters = ["title", "subtitle", "outline", "object"];
      const placeholders = Array.from(layout.querySelectorAll("[presentation-class]"))
          .filter((p) => placeholderFilters.includes(p.getAttribute("presentation-class")))
          .sort(byTop)
          .sort(byLeft);
      const replaced = [];
      const nextMatchingPlaceholder = (contentEl) => {
          const notReplaced = placeholders.filter((p) => !replaced.includes(p));
          if (!notReplaced.length) {
              console.log("all placeholders were replaced. Nowhere to put element");
              console.log(contentEl);
              return null;
          }
          return notReplaced[0];
      };
      const contentOrder = Array.from(slide.children);
      const match = [];
      for (let i = 0; i < contentOrder.length; i++) {
          const contentNode = contentOrder[i];
          const placeholder = nextMatchingPlaceholder(contentNode);
          if (placeholder) {
              replaced.push(placeholder);
              match.push({ placeholder, content: contentNode });
          }
      }
      const unusedPlaceholders = placeholders.filter((p) => !replaced.includes(p));
      const unusedContent = contentOrder.filter((c) => match.filter((m) => m.content == c).length == 0);
      return { match, unusedPlaceholders, unusedContent };
  }
  applyLayout({ deckWidth, deckHeight, layout, slide, }) {
      if (this.settings.processBackgrounds) {
          this.processBackground({ slide, layout, deckWidth, deckHeight });
      }
      if (this.settings.backgroundGradient) {
          slide.setAttribute("data-background-gradient", this.settings.backgroundGradient);
      }
      const footer = layout.querySelector("[presentation-class='footer']");
      if (footer) {
          slide.footer = footer;
      }
      const matchFunction = this.matchByPosition;
      const { match, unusedPlaceholders, unusedContent } = matchFunction(layout, slide);
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
  processBackground({ layout, deckWidth, deckHeight, slide, }) {
      const backgrouds = Array.from(layout.querySelectorAll("div")).filter((div) => div.style.backgroundImage &&
          parseInt(div.parentElement.style.width) >= deckWidth &&
          parseInt(div.parentElement.style.height) >= deckHeight);
      if (backgrouds) {
          var imgBackgrounds = backgrouds.filter((b) => b.style.backgroundImage.includes(".jpeg") || b.style.backgroundImage.includes(".png"));
          const selectedBackground = imgBackgrounds.length && imgBackgrounds[0];
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

let revealOptions = window.revealOptions || {
    plugins: []
};
// remove fullscreen as it breaks background transition (see: https://github.com/evilz/vscode-reveal/issues/922)
revealOptions.plugins = revealOptions.plugins.filter(
  (p) => p.id != "RevealFullscreen"
);

const layouts = new LayoutsPlugin({
  extractBackgrounds: true,
  layoutsDir: ".",
});
revealOptions.plugins.push(layouts);

Reveal.initialize(revealOptions);
