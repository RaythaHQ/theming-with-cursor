# Sample Prompt: Knowledge Base Website

This is an example prompt for building a knowledge base website using the Raytha template workflow. Modify it for your own project needs.

---

You are working in a Raytha theme repository. Follow the workflow in `.cursor/setup.md`:
1. Initialize a project: `dotnet run -- --site mysite`
2. Define content models in `/dist/mysite/models/`
3. Generate sample data in `/dist/mysite/sample-data/`
4. Customize Liquid templates in `/dist/mysite/liquid/`
5. Run the simulator to generate HTML previews
6. Iterate until the design is correct

---

## Goal

Design a **modern knowledge base / help center** for a SaaS product with:
- Clean, professional design with strong accent colors
- Responsive layout
- Good typography and visual hierarchy

---

## Step 1: Initialize the Project

```bash
cd src
dotnet run -- --site helpdesk
```

This creates `/dist/helpdesk/` with starter templates and models.

---

## Step 2: Create the Content Model

Edit `dist/helpdesk/models/articles.md` with fields for knowledge base articles:

| Label        | Developer Name | Field Type        |
|--------------|----------------|-------------------|
| Title        | title          | single_line_text  |
| Summary      | summary        | long_text         |
| Content      | content        | wysiwyg           |
| Category     | category       | dropdown          |
| Difficulty   | difficulty     | radio             |

Categories might include: Getting Started, Integrations, Billing, Troubleshooting, API
Difficulty levels: Beginner, Intermediate, Advanced

---

## Step 3: Generate Sample Data

Create `/dist/helpdesk/sample-data/articles.json` based on the model above.

Include:
- At least 5-6 realistic articles with varied categories and difficulties
- Each item should have `detail_liquid_file` set to generate individual article pages
- Use realistic SaaS help content (e.g., "Connecting Your Account", "API Authentication", "Managing Team Members")
- Set `RoutePath` for each item (e.g., `articles_connecting-your-account`)

Also update `menus.json` to include navigation to the articles list.

---

## Step 4: Customize Liquid Templates

### Base Layout (`raytha_html_base_layout.liquid`)
Update the base layout with:
- Modern header with logo and navigation (use `get_main_menu()`)
- Clean footer
- Use Bootstrap 5 via CDN
- Add custom CSS for the knowledge base aesthetic

### Home Page (`raytha_html_home.liquid`)
Create a knowledge base landing page with:
- Hero section with search bar placeholder
- Category cards/tiles linking to filtered article lists
- "Popular Articles" or "Recently Updated" section
- Use `get_content_items(ContentType='articles', PageSize=6)` to fetch articles

### Article List (`raytha_html_content_item_list.liquid`)
Update for article listings:
- Page title showing the content type or category
- Search bar (can be non-functional placeholder)
- Article cards showing: title, category, summary, difficulty badge, last updated
- Pagination controls using `Target.PageNumber`, `Target.TotalPages`, etc.
- Loop through `Target.Items`

### Article Detail (`raytha_html_content_item_detail.liquid`)
Update for single article view:
- Breadcrumbs (Home > Category > Article Title)
- Article title and metadata (category, difficulty, last updated)
- Main content area with good typography for docs
- "Related Articles" section at the bottom

---

## Step 5: Run and Preview

After creating templates and sample data:

```bash
cd src
dotnet run -- --site helpdesk
```

Open the generated HTML files in `/dist/helpdesk/htmlOutput/` to preview:
- `index.html` (home page)
- `articles.html` (list view)
- Individual article pages

---

## Step 6: Iterate

Review the HTML output. If changes are needed:
1. Update the Liquid templates in `/dist/helpdesk/liquid/`
2. Re-run `dotnet run -- --site helpdesk`
3. Refresh browser to see changes

Repeat until the design looks polished.

---

## Design Guidelines

- **Colors**: Choose a cohesive palette with primary accent, secondary accent, and neutral backgrounds
- **Typography**: Clear hierarchy with distinct heading sizes, readable body text
- **Cards**: Clean article preview cards with subtle shadows or borders
- **Badges**: Category and difficulty indicators as colored pills/badges
- **Spacing**: Generous whitespace, consistent padding
- **Responsive**: Works well on desktop and mobile

---

## When Complete

Once satisfied with the templates:
1. Copy the Liquid files from `/dist/helpdesk/liquid/` into Raytha's template editor (Design → Themes → Web Templates)
2. Create the Articles content type in Raytha matching your model
3. Add real content and publish

---

## Notes

- The simulator uses placeholder images for attachments automatically
- All links between pages work when previewing locally
- Sample data should feel realistic to properly test the templates
- Focus on the Liquid templates — the HTML files are just for preview

---

# Alternative Prompt: Site Page with Widgets

If you prefer a more flexible landing page using Site Pages and Widgets, use this approach:

## Goal

Create a **marketing landing page** using Site Pages with widget sections for easy content management.

## Step 1: Initialize the Project

```bash
cd src
dotnet run -- --site marketing
```

## Step 2: Create Site Page Sample Data

Create `/dist/marketing/sample-data/site-pages.json`:

```json
{
  "CurrentOrganization": { "OrganizationName": "My Product", "TimeZone": "UTC" },
  "PathBase": "",
  "pages": [
    {
      "id": "landing-page-1",
      "title": "Welcome",
      "routePath": "home",
      "webTemplateDeveloperName": "raytha_html_home",
      "isPublished": true,
      "publishedWidgets": {
        "hero": [
          {
            "id": "widget-hero-1",
            "widgetType": "hero",
            "row": 1,
            "column": 1,
            "columnSpan": 12,
            "settingsJson": "{\"headline\": \"Build Something Amazing\", \"subheadline\": \"The platform that helps you create, manage, and publish content effortlessly.\", \"backgroundColor\": \"#0d6efd\", \"textColor\": \"#ffffff\", \"buttonText\": \"Get Started Free\", \"buttonUrl\": \"#pricing\", \"buttonStyle\": \"light\", \"alignment\": \"center\", \"minHeight\": 500}"
          }
        ],
        "features": [
          {
            "id": "widget-features-1",
            "widgetType": "wysiwyg",
            "row": 1,
            "column": 1,
            "columnSpan": 12,
            "cssClass": "text-center",
            "settingsJson": "{\"content\": \"<h2>Why Choose Us</h2><p>Everything you need to succeed</p>\", \"padding\": \"large\"}"
          }
        ],
        "cta": [
          {
            "id": "widget-cta-1",
            "widgetType": "cta",
            "row": 1,
            "column": 1,
            "columnSpan": 12,
            "settingsJson": "{\"headline\": \"Ready to get started?\", \"content\": \"Join thousands of happy customers today.\", \"buttonText\": \"Start Free Trial\", \"buttonUrl\": \"#signup\", \"buttonStyle\": \"primary\", \"backgroundColor\": \"#f8f9fa\", \"textColor\": \"#212529\", \"alignment\": \"center\"}"
          }
        ]
      }
    }
  ]
}
```

## Step 3: Customize Site Page Template

Edit `/dist/marketing/liquid/raytha_html_home.liquid`:

```liquid
{% layout 'raytha_html_base_layout' %}

{{ render_section("hero") }}

<section class="py-5">
  <div class="container">
    {{ render_section("features") }}
  </div>
</section>

{{ render_section("cta") }}
```

## Step 4: Customize Widget Templates

Edit widget templates in `/dist/marketing/liquid/widgets/`:

**Hero Widget (`widgets/hero.liquid`):**
```liquid
<section class="hero-widget" style="
  background-color: {{ widget.settings.backgroundColor | default: '#0d6efd' }};
  color: {{ widget.settings.textColor | default: '#ffffff' }};
  min-height: {{ widget.settings.minHeight | default: 400 }}px;
  display: flex;
  align-items: center;
">
  <div class="container py-5 text-{{ widget.settings.alignment | default: 'center' }}">
    {% if widget.settings.headline != blank %}
      <h1 class="display-3 fw-bold mb-3">{{ widget.settings.headline | escape }}</h1>
    {% endif %}
    
    {% if widget.settings.subheadline != blank %}
      <p class="lead mb-4 opacity-75">{{ widget.settings.subheadline | escape }}</p>
    {% endif %}
    
    {% if widget.settings.buttonText != blank %}
      <a href="{{ widget.settings.buttonUrl | default: '#' }}" 
         class="btn btn-{{ widget.settings.buttonStyle | default: 'light' }} btn-lg px-4">
        {{ widget.settings.buttonText | escape }}
      </a>
    {% endif %}
  </div>
</section>
```

**CTA Widget (`widgets/cta.liquid`):**
```liquid
<section class="cta-widget py-5" style="
  background-color: {{ widget.settings.backgroundColor | default: '#f8f9fa' }};
  color: {{ widget.settings.textColor | default: '#212529' }};
">
  <div class="container text-{{ widget.settings.alignment | default: 'center' }}">
    {% if widget.settings.headline != blank %}
      <h2 class="h1 fw-bold mb-3">{{ widget.settings.headline | escape }}</h2>
    {% endif %}
    
    {% if widget.settings.content != blank %}
      <p class="lead mb-4">{{ widget.settings.content | escape }}</p>
    {% endif %}
    
    {% if widget.settings.buttonText != blank %}
      <a href="{{ widget.settings.buttonUrl | default: '#' }}" 
         class="btn btn-{{ widget.settings.buttonStyle | default: 'primary' }} btn-lg">
        {{ widget.settings.buttonText | escape }}
      </a>
    {% endif %}
  </div>
</section>
```

## Step 5: Run and Preview

```bash
cd src
dotnet run -- --site marketing
```

Open `/dist/marketing/htmlOutput/index.html` to preview.

## Benefits of Site Pages with Widgets

- **Flexible layouts**: Rearrange widgets without changing templates
- **Reusable components**: Same widget templates work across multiple pages
- **Content editor friendly**: Non-technical users can update content easily
- **Consistent design**: Widget templates enforce design standards
- **Easy A/B testing**: Swap widgets to test different layouts
