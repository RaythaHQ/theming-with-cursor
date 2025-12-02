# Raytha Theming – Cursor Setup

This repository is a local development workspace for Raytha templates with a built-in simulator.

**Key principle:** Write Liquid templates, generate HTML previews via the simulator. Each site project is isolated in its own folder.

---

## Directory Structure

```
/raytha-theming/
├── src/                              # Simulator source (NEVER MODIFY)
│   ├── liquid/                       # Starter templates (copied to new projects)
│   │   ├── raytha_html_*.liquid      # Page templates
│   │   └── widgets/                  # Widget templates
│   │       ├── hero.liquid
│   │       ├── wysiwyg.liquid
│   │       ├── cta.liquid
│   │       └── ...
│   ├── models/                       # Starter content model
│   │   └── posts.md
│   ├── Program.cs                    # Simulator entry point
│   ├── RenderEngine.cs               # Liquid rendering engine
│   └── ...
├── dist/                             # YOUR PROJECTS (gitignored)
│   └── <site-name>/                  # Each site is a separate project
│       ├── liquid/                   # Customized templates
│       │   ├── raytha_html_*.liquid
│       │   └── widgets/
│       ├── sample-data/              # Your site's JSON data
│       │   ├── menus.json
│       │   ├── site-pages.json
│       │   └── posts.json
│       ├── models/                   # Content type definitions
│       └── htmlOutput/               # Generated HTML
├── .cursor/
│   ├── setup.md                      # This file
│   └── raytha-template-documentation.md
├── raytha.config.json                # API sync configuration
└── README.md
```

---

## Core Concepts

### Template Types

Raytha has two categories of templates organized into **Themes**:

**1. Web Templates** - Render pages:

| Template | Purpose | Uses Layout? |
|----------|---------|--------------|
| `raytha_html_base_layout` | Parent template with `{% renderbody %}` | No (is the layout) |
| `raytha_html_home` | **Home page** - Site Page with widget sections | Yes |
| `raytha_html_page_fullwidth` | **Site Page** - single section layout | Yes |
| `raytha_html_page_sidebar` | **Site Page** - two-column with sidebar | Yes |
| `raytha_html_page_multi` | **Site Page** - multiple sections for marketing pages | Yes |
| `raytha_html_content_item_list` | **Content list** - displays items from a content type | Yes |
| `raytha_html_content_item_detail` | **Content detail** - single content item view | Yes |

**2. Widget Templates** - Render widgets on Site Pages (in `liquid/widgets/`):

| Widget | File | Purpose |
|--------|------|---------|
| Hero | `hero.liquid` | Large banner sections with headline and CTA |
| WYSIWYG | `wysiwyg.liquid` | Rich text content blocks |
| Card | `card.liquid` | Bordered content cards with image and button |
| CTA | `cta.liquid` | Call-to-action sections |
| Content List | `contentlist.liquid` | Dynamic lists of content items |
| Image & Text | `imagetext.liquid` | Side-by-side image and text |
| FAQ | `faq.liquid` | Frequently asked questions accordion |
| Embed | `embed.liquid` | Embed external content (videos, maps, etc.) |

### Template Inheritance

All child templates should start with:
```liquid
{% layout 'raytha_html_base_layout' %}
```

The base layout contains:
- HTML document structure
- Navigation menu
- Footer
- `{% renderbody %}` where child content is injected

---

## Workflow

### 1. Initialize a New Project

```bash
cd src
dotnet run -- --site mywebsite
```

This creates `/dist/mywebsite/` with:
- `liquid/` - Starter templates copied from `/src/liquid/`
- `models/` - Starter content model (posts.md)
- `sample-data/` - Starter menus.json
- `htmlOutput/` - Empty (for generated HTML)

### 2. Define Content Models

Content models live in `/dist/<site>/models/*.md` and define the structure of content types.

Format:
```markdown
# Posts Content Type

| Label       | Developer Name | Field Type        |
|-------------|----------------|-------------------|
| Title       | title          | single_line_text  |
| Content     | content        | wysiwyg           |
| Featured Image | featured_image | attachment     |
```

When creating or updating models, use these field types:
- `single_line_text` — Short text
- `long_text` — Multi-line text
- `wysiwyg` — Rich HTML content
- `dropdown` — Single select with choices
- `radio` — Radio button selection
- `multiple_select` — Multi-select
- `checkbox` — Boolean
- `date` — Date picker
- `number` — Numeric
- `attachment` — File upload
- `one_to_one_relationship` — Link to another content item

### 3. Generate Sample Data

Sample data JSON files in `/dist/<site>/sample-data/` drive the simulator.

**Reserved files:**
- `menus.json` - Navigation menus
- `site-pages.json` - Site pages with widgets

**Content type files:** Any other `.json` file (e.g., `posts.json`, `articles.json`)

#### Site Pages (`site-pages.json`)

```json
{
  "CurrentOrganization": { "OrganizationName": "My Site", "TimeZone": "UTC" },
  "PathBase": "",
  "pages": [
    {
      "id": "home-page",
      "title": "Home",
      "routePath": "home",
      "webTemplateDeveloperName": "raytha_html_home",
      "isPublished": true,
      "publishedWidgets": {
        "hero": [
          {
            "id": "widget-1",
            "widgetType": "hero",
            "row": 1,
            "column": 1,
            "columnSpan": 12,
            "settingsJson": "{\"headline\": \"Welcome\", \"subheadline\": \"Your tagline here\"}"
          }
        ],
        "main": [
          {
            "id": "widget-2",
            "widgetType": "wysiwyg",
            "row": 1,
            "column": 1,
            "columnSpan": 12,
            "settingsJson": "{\"content\": \"<p>Your content here</p>\"}"
          }
        ]
      }
    }
  ]
}
```

**Key fields:**
- `routePath` - Output filename (`home` → `index.html`, `about` → `about.html`)
- `webTemplateDeveloperName` - Which template to render
- `publishedWidgets` - Object with section names as keys, arrays of widgets as values
- `settingsJson` - JSON string with widget settings

#### Content Type Data (e.g., `posts.json`)

```json
{
  "liquid_file": "raytha_html_content_item_list",
  "Target": {
    "Label": "Posts",
    "RoutePath": "posts",
    "Items": [
      {
        "detail_liquid_file": "raytha_html_content_item_detail",
        "PrimaryField": "Getting Started with Raytha",
        "RoutePath": "posts_getting-started",
        "CreationTime": "2024-01-15T10:00:00Z",
        "PublishedContent": {
          "title": { "Text": "Getting Started", "Value": "Getting Started" },
          "content": { "Text": "<p>Welcome...</p>", "Value": "<p>Welcome...</p>" }
        }
      }
    ],
    "TotalCount": 1,
    "PageNumber": 1,
    "PageSize": 25,
    "TotalPages": 1
  },
  "ContentType": {
    "LabelPlural": "Posts",
    "LabelSingular": "Post",
    "DeveloperName": "posts"
  },
  "CurrentOrganization": { "OrganizationName": "My Site" },
  "PathBase": ""
}
```

#### Menus (`menus.json`)

```json
{
  "menus": [
    {
      "Id": "main-menu",
      "Label": "Main Menu",
      "DeveloperName": "main_menu",
      "IsMainMenu": true,
      "MenuItems": [
        { "Id": "1", "Label": "Home", "Url": "/", "Ordinal": 1 },
        { "Id": "2", "Label": "Posts", "Url": "/posts", "Ordinal": 2 }
      ]
    }
  ]
}
```

### 4. Write Liquid Templates

Templates live in `/dist/<site>/liquid/*.liquid`.

**Template inheritance:**
```liquid
{% layout 'raytha_html_base_layout' %}
<div class="container">
  {{ Target.PrimaryField }}
</div>
```

Parent templates use `{% renderbody %}` to inject child content.

**Available variables:**
- `Target` — Main content (single item, list with `.Items`, or site page)
- `Target.PrimaryField` — Primary field value
- `Target.PublishedContent.<field>.Text` — Field display value
- `Target.PublishedContent.<field>.Value` — Field raw value
- `Target.RoutePath` — Route path
- `Target.CreationTime` — Creation timestamp
- `ContentType.LabelPlural`, `ContentType.DeveloperName`
- `CurrentOrganization.OrganizationName`
- `CurrentUser.IsAuthenticated`, `CurrentUser.IsAdmin`
- `PathBase` — Base path for URLs
- `QueryParams` — URL query parameters

**Available functions:**
- `get_main_menu()` — Returns main navigation menu
- `get_menu('developer_name')` — Returns specific menu
- `get_content_items(ContentType='name', Filter='odata', OrderBy='field desc', PageNumber=1, PageSize=10)` — Query content
- `get_content_item_by_id(id)` — Get single item
- `get_content_type_by_developer_name(name)` — Get content type metadata
- `render_section("section_name")` — Render widgets in a section (Site Pages only)
- `get_section("section_name")` — Get raw widget data for custom rendering

**Available filters:**
- `attachment_redirect_url` — Secure attachment URL
- `attachment_public_url` — Public attachment URL
- `organization_time` — Convert to org timezone
- `groupby: 'field'` — Group items by field
- `json` — Output as JSON (debugging)

### 5. Write Widget Templates

Widget templates render individual widgets on Site Pages. Located in `/dist/<site>/liquid/widgets/`.

**Widget template structure:**
```liquid
{% comment %}
Widget: Hero
Settings: headline, subheadline, backgroundColor, textColor, buttonText, buttonUrl, alignment, minHeight
{% endcomment %}

<section class="hero-widget" style="
  background-color: {{ widget.settings.backgroundColor | default: '#0d6efd' }};
  color: {{ widget.settings.textColor | default: '#ffffff' }};
  min-height: {{ widget.settings.minHeight | default: 400 }}px;
">
  <div class="container py-5 text-{{ widget.settings.alignment | default: 'center' }}">
    {% if widget.settings.headline != blank %}
      <h1 class="display-4 fw-bold">{{ widget.settings.headline | escape }}</h1>
    {% endif %}
    
    {% if widget.settings.subheadline != blank %}
      <p class="lead">{{ widget.settings.subheadline | escape }}</p>
    {% endif %}
    
    {% if widget.settings.buttonText != blank %}
      <a href="{{ widget.settings.buttonUrl | default: '#' }}" class="btn btn-light btn-lg">
        {{ widget.settings.buttonText | escape }}
      </a>
    {% endif %}
  </div>
</section>
```

**Widget context variables:**
- `widget.id` — Widget instance ID
- `widget.type` — Widget type (e.g., "hero")
- `widget.settings.*` — Widget settings from `settingsJson`
- `widget.row`, `widget.column`, `widget.columnSpan` — Grid position
- `widget.css_class`, `widget.html_id`, `widget.custom_attributes` — Styling

**Widget template best practices:**
- Check for blank values: `{% if widget.settings.headline != blank %}`
- Use defaults: `{{ widget.settings.backgroundColor | default: '#ffffff' }}`
- Escape user content: `{{ widget.settings.headline | escape }}`

### 6. Run the Simulator

```bash
cd src
dotnet run -- --site mywebsite
```

This:
1. Reads all JSON files in `/dist/mywebsite/sample-data/`
2. Renders site pages from `site-pages.json`
3. Renders content types from other JSON files
4. For content items with `detail_liquid_file`, generates individual detail pages
5. Outputs HTML to `/dist/mywebsite/htmlOutput/`

### 7. Preview and Iterate

Open generated HTML files in a browser. All links should work locally.

To make changes:
1. Edit templates in `/dist/mywebsite/liquid/`
2. Re-run `dotnet run -- --site mywebsite`
3. Refresh browser

### 8. Deploy to Raytha

Copy final templates from `/dist/mywebsite/liquid/` into Raytha's template editor:
1. Go to **Design → Themes** in the admin
2. Select your theme
3. Navigate to **Web Templates** or **Widget Templates**
4. Create/update templates with your Liquid code

**Note:** Remove `{% layout 'name' %}` tags when copying to Raytha (they're local-only).

---

## Commands Reference

```bash
# Initialize or render a project
dotnet run -- --site <name>

# Render the default project
dotnet run

# Render and sync to Raytha
dotnet run -- --site mywebsite --sync

# Only sync templates (no render)
dotnet run -- --site mywebsite --sync-only

# Show help
dotnet run -- --help
```

---

## Template Usage Summary

| What You're Building | Template to Use | Sample Data Structure |
|----------------------|-----------------|----------------------|
| Home page with widgets | `raytha_html_home` | `site-pages.json` with `publishedWidgets` |
| About/Contact page | `raytha_html_page_*` | `site-pages.json` with `publishedWidgets` |
| Blog/Posts list | `raytha_html_content_item_list` | Content JSON with `Target.Items` |
| Single blog post | `raytha_html_content_item_detail` | Via `detail_liquid_file` |

---

## Important Rules

### DO:
- Initialize projects with `dotnet run -- --site <name>`
- Focus editing on `/dist/<site>/liquid/*.liquid` files
- Generate sample data based on `/dist/<site>/models/*.md` definitions
- Use CDN links for CSS/JS (Bootstrap, etc.)
- Set `PathBase` to `""` for root-relative links
- Set `RoutePath` on items to valid filenames (no `.html` extension needed)
- Check for blank values in widget templates
- Use the `default` filter for fallback values
- Escape user-provided content in widget templates
- Access widget settings via `widget.settings.*`

### DO NOT:
- Edit files in `/src/` — they are starter templates
- Edit files in `/dist/<site>/htmlOutput/` — they are generated
- Use Liquid syntax that the Fluid engine doesn't support
- Include actual Liquid `{{ }}` or `{% %}` in content text
- Forget the `{% layout %}` tag in child templates (for local rendering)
- Render empty HTML tags for blank widget settings
- **Put CSS classes in WYSIWYG content** — TipTap editor produces plain HTML only

---

## WYSIWYG Content Rules (CRITICAL)

WYSIWYG content comes from TipTap editor which produces **plain semantic HTML only**:

**✅ ALLOWED in WYSIWYG content:**
```html
<h2>Heading</h2>
<p>Paragraph text with <strong>bold</strong> and <em>italic</em>.</p>
<ul><li>List item</li></ul>
<ol><li>Numbered item</li></ol>
<blockquote><p>Quote text</p></blockquote>
<a href="url">Link text</a>
```

**❌ NOT ALLOWED in WYSIWYG content:**
```html
<div class="custom-class">...</div>
<p class="text-muted">...</p>
<section id="special">...</section>
```

### When You Need Custom Styling

1. **Use widget `cssClass` meta field** — Style child elements via CSS:
   ```json
   { "cssClass": "testimonial-section", "settingsJson": "{\"content\": \"<blockquote>...</blockquote>\"}" }
   ```
   Then in your base layout CSS:
   ```css
   .testimonial-section blockquote { font-style: italic; background: #f5f5f5; }
   ```

2. **Use an Embed widget** — For custom HTML with classes:
   ```json
   { "widgetType": "embed", "settingsJson": "{\"embedCode\": \"<div class='contact-info'>...</div>\"}" }
   ```

3. **Split into multiple widgets** — Break complex content into separate widgets

4. **Create a dedicated widget** — For reusable custom components, make a new widget template

---

## CSS and JavaScript

Always use CDN links in Liquid templates:

```liquid
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">

<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" crossorigin="anonymous"></script>
```

---

## Image Handling

The simulator returns placeholder images for attachment filters:
- Images: `https://placehold.co/400x300/e2e8f0/64748b?text=Image+Placeholder`
- Files: `https://placehold.co/200x200/f1f5f9/475569?text=File`

In sample data, just use any filename for attachment values.

---

## Cursor Behavior Summary

When working in this repo, Cursor should:

1. **Initialize projects** — Run `dotnet run -- --site <name>` to create new projects
2. **Focus on Liquid templates** — Edit files in `/dist/<site>/liquid/`
3. **Generate sample data** — Read `/dist/<site>/models/*.md`, create JSON in `/dist/<site>/sample-data/`
4. **Run the simulator** — Execute `cd src && dotnet run -- --site <name>` to regenerate HTML
5. **Iterate until correct** — Update Liquid, regenerate, review HTML
6. **Never edit `/src/`** — These are starter templates
7. **Never edit `htmlOutput/`** — These are generated outputs

---

## Site Pages & Widgets Quick Reference

### Creating a Site Page

1. Add page to `site-pages.json` with `publishedWidgets` containing section configurations
2. Use a site page template that uses `render_section()`:

```liquid
{% layout 'raytha_html_base_layout' %}

{{ render_section("hero") }}

<div class="container py-5">
  <div class="row">
    <div class="col-lg-8">
      {{ render_section("main") }}
    </div>
    <div class="col-lg-4">
      {{ render_section("sidebar") }}
    </div>
  </div>
</div>
```

3. Customize widget templates in `liquid/widgets/` for each widget type

### Available Site Page Templates

| Template | Sections | Best For |
|----------|----------|----------|
| `raytha_html_home` | hero, features, content, cta | Home/landing pages |
| `raytha_html_page_fullwidth` | main | Simple content pages |
| `raytha_html_page_sidebar` | main, sidebar | About pages, blog-style |
| `raytha_html_page_multi` | hero, features, content, cta | Marketing/product pages |

### Widget Meta Fields

All widgets support these meta fields (set at the widget level, not in settingsJson):

| Field | Purpose | Example |
|-------|---------|---------|
| `cssClass` | CSS class on widget wrapper | `"testimonial-section"`, `"dark-theme"` |
| `htmlId` | HTML ID attribute | `"hero-main"`, `"contact-form"` |
| `customAttributes` | Additional HTML attributes | `"data-aos=\"fade-up\""` |

**Use meta fields instead of wrapping content in divs.** For example, to style a WYSIWYG section:

```json
{
  "id": "testimonial-1",
  "widgetType": "wysiwyg",
  "cssClass": "testimonial-section",
  "settingsJson": "{\"content\": \"<blockquote>...</blockquote>\", \"padding\": \"large\"}"
}
```

### Widget Settings by Type

| Widget | Settings |
|--------|----------|
| Hero | headline, subheadline, backgroundColor, textColor, buttonText, buttonUrl, buttonStyle, alignment, minHeight, backgroundImage |
| WYSIWYG | content, padding (none/small/medium/large), maxWidth (narrow/medium/wide/full) |
| Card | title, description, imageUrl, buttonText, buttonUrl, buttonStyle |
| CTA | headline, content, buttonText, buttonUrl, buttonStyle, backgroundColor, textColor, alignment |
| Image+Text | headline, content, imageUrl, imagePosition (left/right), buttonText, buttonUrl |
| FAQ | headline, items (array of question/answer) |
| Embed | embedCode, aspectRatio |
| Content List | headline, contentType, pageSize, displayStyle, showImage, showDate, showExcerpt |
