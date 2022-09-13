# Powerpoint to reveal converter

This tool can convert powerpoint **layouts** to html that can be used in reveal presentations.

## Usage

### PPTX/ODP to HTML

1. Open a presentation in PowerPoint. Go to View -> Master Layout. On the left, you'll see a list of master layouts that are defined in the presentation. This is what's going to be exported. **Not The actual presentation content**.
2. Save the presentation as ODP (Open Document Presentation) format.
3. Rename that ODP to ZIP and extract it somewhere (i.e. `my-presentation-odp`).
4. If your presentation uses any fancy fonts, put them in `media` directory, inside the extracted one (i.e. `my-presentation-odp/media`). 
5. Extract layouts to html:
   
   ```
   > cd extract-layouts
   > dotnet run --in {path-to-extracted-odp-dir} --out {output-directory}
   ```
6. Review the converted html files. When opened in a browser, they should look the same as presentation layouts.

### Use html layouts in reveal.js

1. Add `LayoutsPlugin` to `plugins` section of reveal options:

    ```
    const layouts = new LayoutsPlugin({
        extractBackgrounds: true,
        layoutsDir: ".",
    });
    revealOptions.plugins.push(layouts);
    ```
2. 