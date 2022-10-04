import LayoutsPlugin from "./plugins/dist/layouts.js";

console.log("init.esm");

// remove fullscreen as it breaks background transition (see: https://github.com/evilz/vscode-reveal/issues/922)
revealOptions.plugins = revealOptions.plugins.filter(
  (p) => p.id != "RevealFullscreen"
);

const layouts = new LayoutsPlugin({
  extractBackgrounds: true,
  backgroundGradient: "linear-gradient(to bottom right, #201d52, #210d40)",
  layoutsDir: "layouts",
  footer: "my awesome presentation"
});
revealOptions.plugins.push(layouts);

console.dir(revealOptions);

Reveal.initialize(revealOptions);
