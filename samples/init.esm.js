import LayoutsPlugin from './plugins/layouts.js'

let revealOptions = getDefaultOptions?.call() || {
    plugins: []
};
// remove fullscreen as it breaks background transition (see: https://github.com/evilz/vscode-reveal/issues/922)
revealOptions.plugins = revealOptions.plugins.filter(
  (p) => p.id != "RevealFullscreen"
);

const layouts = new LayoutsPlugin({
  extractBackgrounds: false,
  layoutsDir: ".",
});
revealOptions.plugins.push(layouts);

Reveal.initialize(revealOptions);
