# Raytha Theming – Cursor Setup

This repository is a local development workspace for Raytha templates with a built-in simulator.

**Key principle:** Write Liquid templates, generate HTML previews via the simulator.

---

## Directory Structure

```
├── liquid/                              # Liquid templates (PRIMARY FOCUS)
│   ├── raytha_html_base_layout.liquid   # Base layout (parent template)
│   ├── raytha_html_home.liquid          # Home page (Site Page with widgets)
│   ├── raytha_html_page_fullwidth.liquid    # Site Page: single zone layout
│   ├── raytha_html_page_sidebar.liquid      # Site Page: two-column with sidebar
│   ├── raytha_html_page_multi.liquid        # Site Page: multiple zones
│   ├── raytha_html_content_item_list.liquid # Content list view (e.g., Posts list)
│   ├── raytha_html_content_item_detail.liquid # Content detail view (e.g., single Post)
│   └── widgets/                         # Widget templates
│       ├── raytha_widget_hero.liquid
│       ├── raytha_widget_wysiwyg.liquid
│       ├── raytha_widget_card.liquid
│       ├── raytha_widget_cta.liquid
│       ├── raytha_widget_contentlist.liquid
│       └── ...
├── models/                              # Content type definitions (Markdown)
├── src/
│   ├── sample-data/                     # Sample data JSON files
│   └── ...                              # Simulator source code
├── html/                                # Generated HTML output (DO NOT EDIT)
└── .cursor/
    └── setup.md                         # This file
```

---

## Core Concepts

### Template Types

Raytha has two categories of templates organized into **Themes**:

**1. Web Templates** - Render pages:

| Template | Purpose | Uses Layout? |
|----------|---------|--------------|
| `raytha_html_base_layout` | Parent template with `{% renderbody %}` | No (is the layout) |
| `raytha_html_home` | **Home page** - Site Page with widget zones | Yes |
| `raytha_html_page_fullwidth` | **Site Page** - single zone layout | Yes |
| `raytha_html_page_sidebar` | **Site Page** - two-column with sidebar | Yes |
| `raytha_html_page_multi` | **Site Page** - multiple zones for marketing pages | Yes |
| `raytha_html_content_item_list` | **Content list** - displays items from a content type (e.g., Posts) | Yes |
| `raytha_html_content_item_detail` | **Content detail** - single content item view (e.g., one Post) | Yes |

**2. Widget Templates** - Render widgets on Site Pages:

| Widget | Developer Name | Purpose |
|--------|----------------|---------|
| Hero | `raytha_widget_hero` | Large banner sections with headline and CTA |
| WYSIWYG | `raytha_widget_wysiwyg` | Rich text content blocks |
| Card | `raytha_widget_card` | Bordered content cards with image and button |
| CTA | `raytha_widget_cta` | Call-to-action sections |
| Content List | `raytha_widget_contentlist` | Dynamic lists of content items |
| Image & Text | `raytha_widget_imagetext` | Side-by-side image and text |
| FAQ | `raytha_widget_faq` | Frequently asked questions accordion |
| Embed | `raytha_widget_embed` | Embed external content (videos, maps, etc.) |

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

### 1. Define Content Models

Content models live in `/models/*.md` and define the structure of content types.

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

### 2. Generate Sample Data

Sample data JSON files in `/src/sample-data/` drive the simulator. Generate them based on the models.

**Structure for Content List Views (e.g., Posts list):**
```json
{
  "liquid_file": "raytha_html_content_item_list",
  "Target": {
    "Label": "Posts",
    "RoutePath": "posts.html",
    "Items": [
      {
        "detail_liquid_file": "raytha_html_content_item_detail",
        "PrimaryField": "Getting Started with Raytha",
        "RoutePath": "posts_getting-started.html",
        "CreationTime": "2024-01-15T10:00:00Z",
        "PublishedContent": {
          "title": { "Text": "Getting Started with Raytha", "Value": "Getting Started with Raytha" },
          "content": { "Text": "<p>Welcome to Raytha...</p>", "Value": "<p>Welcome to Raytha...</p>" }
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
  "CurrentUser": { "IsAuthenticated": false },
  "PathBase": ".",
  "QueryParams": {}
}
```

**Structure for Site Pages (e.g., Home page with widgets):**
```json
{
  "liquid_file": "raytha_html_home",
  "Target": {
    "Id": "home-page-1",
    "Title": "Home",
    "RoutePath": "home.html",
    "IsPublished": true
  },
  "Zones": {
    "hero": [
      {
        "widget_template": "raytha_widget_hero",
        "headline": "Welcome to Our Site",
        "subheadline": "Build something amazing",
        "backgroundColor": "#0d6efd",
        "textColor": "#ffffff",
        "buttonText": "Get Started",
        "buttonUrl": "posts.html",
        "alignment": "center",
        "minHeight": 500
      }
    ],
    "features": [
      {
        "widget_template": "raytha_widget_wysiwyg",
        "content": "<h2>Our Features</h2><p>Explore what we offer...</p>",
        "padding": "medium"
      }
    ],
    "cta": [
      {
        "widget_template": "raytha_widget_cta",
        "headline": "Ready to get started?",
        "buttonText": "Contact Us",
        "buttonUrl": "contact.html"
      }
    ]
  },
  "CurrentOrganization": { "OrganizationName": "My Site" },
  "CurrentUser": { "IsAuthenticated": false },
  "PathBase": ".",
  "QueryParams": {}
}
```

**Key rules:**
- `liquid_file` — Template used for the main view
- `detail_liquid_file` — Per-item, specifies template for detail page (content items only)
- `widget_template` — For widgets, specifies which widget template to use
- `Zones` — Object containing arrays of widgets per zone (Site Pages only)
- `RoutePath` — Output HTML filename (e.g., `posts_getting-started.html`)
- `PathBase` — Use `"."` for relative links in simulator
- Fields use `{ "Text": "...", "Value": "..." }` format

**Menus** go in `menus.json`:
```json
{
  "menus": [
    {
      "DeveloperName": "main",
      "IsMainMenu": true,
      "MenuItems": [
        { "Label": "Home", "Url": "home.html" },
        { "Label": "Posts", "Url": "posts.html" },
        { "Label": "About", "Url": "about.html" }
      ]
    },
    {
      "DeveloperName": "footer",
      "IsMainMenu": false,
      "MenuItems": [
        { "Label": "Privacy Policy", "Url": "privacy.html" },
        { "Label": "Terms", "Url": "terms.html" }
      ]
    }
  ]
}
```

### 3. Write Liquid Templates

Templates live in `/liquid/*.liquid`. Focus your editing here.

**Template inheritance:**
All child templates must use `{% layout 'template_name' %}` at the top:
```liquid
{% layout 'raytha_html_base_layout' %}
<div class="container">
  {{ Target.PrimaryField }}
</div>
```

Parent templates (like base layout) use `{% renderbody %}` to inject child content.

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
- `render_zone "zone_name"` — Render widgets in a zone (Site Pages only)

**Available filters:**
- `attachment_redirect_url` — Secure attachment URL
- `attachment_public_url` — Public attachment URL
- `organization_time` — Convert to org timezone
- `groupby: 'field'` — Group items by field
- `json` — Output as JSON (debugging)
- `get_navigation_menu` — Retrieve menu by developer name (filter syntax)

### 4. Write Widget Templates

Widget templates render individual widgets on Site Pages. Place them in `/liquid/widgets/`.

**Widget template structure:**
```liquid
{% comment %} widgets/raytha_widget_hero.liquid {% endcomment %}
<section class="hero-widget" style="
  background-color: {{ Target.backgroundColor | default: '#0d6efd' }};
  color: {{ Target.textColor | default: '#ffffff' }};
  min-height: {{ Target.minHeight | default: 400 }}px;
">
  <div class="container py-5 text-{{ Target.alignment | default: 'center' }}">
    {% if Target.headline != blank %}
      <h1 class="display-4 fw-bold">{{ Target.headline | escape }}</h1>
    {% endif %}
    
    {% if Target.subheadline != blank %}
      <p class="lead">{{ Target.subheadline | escape }}</p>
    {% endif %}
    
    {% if Target.buttonText != blank %}
      <a href="{{ Target.buttonUrl | default: '#' }}" class="btn btn-light btn-lg">
        {{ Target.buttonText | escape }}
      </a>
    {% endif %}
  </div>
</section>
```

**Widget template best practices:**
- Check for blank values: `{% if Target.headline != blank %}`
- Use defaults: `{{ Target.backgroundColor | default: '#ffffff' }}`
- Escape user content: `{{ Target.headline | escape }}`

### 5. Run the Simulator

```bash
cd src
dotnet run
```

This:
1. Reads all JSON files in `/src/sample-data/` (except `menus.json`)
2. Renders each using its `liquid_file` template
3. For content items with `detail_liquid_file`, generates individual detail pages
4. For Site Pages, renders widgets in zones using widget templates
5. Outputs HTML to `/html/`

### 6. Preview and Iterate

Open generated HTML files in a browser. All links should work locally.

To make changes:
1. Edit Liquid templates in `/liquid/`
2. Re-run `dotnet run` in `/src/`
3. Refresh browser

### 7. Deploy to Raytha

Copy final Liquid templates from `/liquid/` into Raytha's template editor:
1. Go to **Design → Themes** in the admin
2. Select your theme
3. Navigate to **Web Templates** or **Widget Templates**
4. Create/update templates with your Liquid code

---

## Template Usage Summary

| What You're Building | Template to Use | Sample Data Structure |
|----------------------|-----------------|----------------------|
| Home page with widgets | `raytha_html_home` | Site Page with `Zones` |
| About/Contact page with widgets | `raytha_html_page_*` | Site Page with `Zones` |
| Blog/Posts list | `raytha_html_content_item_list` | Content list with `Target.Items` |
| Single blog post | `raytha_html_content_item_detail` | Content item (via `detail_liquid_file`) |
| Articles/Docs list | `raytha_html_content_item_list` | Content list with `Target.Items` |
| Single article | `raytha_html_content_item_detail` | Content item (via `detail_liquid_file`) |

---

## Important Rules

### DO:
- Focus editing on `/liquid/*.liquid` files
- Use `{% layout 'raytha_html_base_layout' %}` at the top of all child templates
- Generate sample data based on `/models/*.md` definitions
- Use CDN links for CSS/JS (Bootstrap, etc.)
- Use `PathBase` of `"."` in sample data for relative links
- Set `RoutePath` on items to valid HTML filenames
- Include `detail_liquid_file` on content items that need detail pages
- Include `widget_template` on widgets to specify their template
- Check for blank values in widget templates
- Use the `default` filter for fallback values
- Escape user-provided content in widget templates

### DO NOT:
- Edit files in `/html/` directly — they are generated
- Use Liquid syntax that the Fluid engine doesn't support
- Include actual Liquid `{{ }}` or `{% %}` in content text (it will be parsed)
- Forget the `{% layout %}` tag in child templates
- Render empty HTML tags for blank widget settings

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

1. **Focus on Liquid templates** — Edit files in `/liquid/`
2. **Generate sample data from models** — Read `/models/*.md`, create JSON in `/src/sample-data/`
3. **Run the simulator** — Execute `cd src && dotnet run` to regenerate HTML
4. **Iterate until correct** — Update Liquid, regenerate, review HTML
5. **Never edit `/html/` directly** — These are generated outputs
6. **Always use `{% layout %}` tag** — Every child template needs it for the simulator to work

---

## Site Pages & Widgets Quick Reference

### Creating a Site Page

1. Create sample data JSON with `Zones` containing widget configurations
2. Use a site page template (e.g., `raytha_html_home`) that uses `render_zone`:

```liquid
{% layout 'raytha_html_base_layout' %}

{{ render_zone "hero" }}

<div class="container py-5">
  <div class="row">
    <div class="col-lg-8">
      {{ render_zone "main" }}
    </div>
    <div class="col-lg-4">
      {{ render_zone "sidebar" }}
    </div>
  </div>
</div>
```

3. Create widget templates in `/liquid/widgets/` for each widget type

### Available Site Page Templates

| Template | Zones | Best For |
|----------|-------|----------|
| `raytha_html_home` | hero, features, content, cta | Home/landing pages |
| `raytha_html_page_fullwidth` | main | Simple content pages |
| `raytha_html_page_sidebar` | main, sidebar | About pages, blog-style pages |
| `raytha_html_page_multi` | hero, features, content, cta | Marketing/product pages |

### Available Widget Settings

| Widget | Settings |
|--------|----------|
| Hero | headline, subheadline, backgroundColor, textColor, buttonText, buttonUrl, buttonStyle, alignment, minHeight, backgroundImage |
| WYSIWYG | content, padding |
| Card | title, description, imageUrl, buttonText, buttonUrl, buttonStyle |
| CTA | headline, content, buttonText, buttonUrl, buttonStyle, backgroundColor, textColor, alignment |
| Content List | headline, subheadline, contentType, pageSize, displayStyle, showImage, showDate, showExcerpt, linkText, linkUrl, items (array) |
