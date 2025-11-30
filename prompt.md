# Sample Prompt: Knowledge Base Website

This is an example prompt for building a knowledge base website using the Raytha template workflow. Modify it for your own project needs.

---

You are working in a Raytha theme repository. Follow the workflow in `.cursor/setup.md`:
1. Define content models in `/models/`
2. Generate sample data in `/src/sample-data/`
3. Write Liquid templates in `/liquid/`
4. Run the simulator to generate HTML previews
5. Iterate until the design is correct

---

## Goal

Design a **modern knowledge base / help center** for a SaaS product with:
- Clean, professional design with strong accent colors
- Responsive layout
- Good typography and visual hierarchy

---

## Step 1: Create the Content Model

Create `models/articles.md` with fields for knowledge base articles:

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

## Step 2: Generate Sample Data

Create `/src/sample-data/articles.json` based on the model above.

Include:
- At least 5-6 realistic articles with varied categories and difficulties
- Each item should have `detail_liquid_file` set to generate individual article pages
- Use realistic SaaS help content (e.g., "Connecting Your Account", "API Authentication", "Managing Team Members")
- Set `RoutePath` for each item (e.g., `articles_connecting-your-account.html`)

Also update `menus.json` to include navigation to the articles list.

---

## Step 3: Create Liquid Templates

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

## Step 4: Run and Preview

After creating templates and sample data:

```bash
cd src
dotnet run
```

Open the generated HTML files in `/html/` to preview:
- `home.html`
- `articles.html` (list view)
- Individual article pages

---

## Step 5: Iterate

Review the HTML output. If changes are needed:
1. Update the Liquid templates in `/liquid/`
2. Re-run `dotnet run`
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
1. Copy the Liquid files from `/liquid/` into Raytha's template editor (Design → Themes → Web Templates)
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

Create a **marketing landing page** using Site Pages with widget zones for easy content management.

## Step 1: Create Site Page Sample Data

Create `/src/sample-data/landing.json`:

```json
{
  "liquid_file": "raytha_html_site_page",
  "Target": {
    "Id": "landing-page-1",
    "Title": "Welcome",
    "RoutePath": "landing.html",
    "IsPublished": true
  },
  "Zones": {
    "hero": [
      {
        "widget_template": "hero",
        "headline": "Build Something Amazing",
        "subheadline": "The platform that helps you create, manage, and publish content effortlessly.",
        "backgroundColor": "#0d6efd",
        "textColor": "#ffffff",
        "buttonText": "Get Started Free",
        "buttonUrl": "#pricing",
        "buttonStyle": "light",
        "alignment": "center",
        "minHeight": 500
      }
    ],
    "features": [
      {
        "widget_template": "content_list",
        "headline": "Why Choose Us",
        "subheadline": "Everything you need to succeed",
        "displayStyle": "cards",
        "contentType": "features",
        "pageSize": 3,
        "showImage": true,
        "showExcerpt": true
      }
    ],
    "cta": [
      {
        "widget_template": "cta",
        "headline": "Ready to get started?",
        "content": "Join thousands of happy customers today.",
        "buttonText": "Start Free Trial",
        "buttonUrl": "#signup",
        "buttonStyle": "primary",
        "backgroundColor": "#f8f9fa",
        "textColor": "#212529",
        "alignment": "center"
      }
    ]
  },
  "CurrentOrganization": { "OrganizationName": "My Product" },
  "PathBase": ".",
  "QueryParams": {}
}
```

## Step 2: Create Site Page Template

Create `/liquid/raytha_html_site_page.liquid`:

```liquid
{% layout 'raytha_html_base_layout' %}

{{ render_zone "hero" }}

<section class="py-5">
  <div class="container">
    {{ render_zone "features" }}
  </div>
</section>

{{ render_zone "cta" }}
```

## Step 3: Create Widget Templates

Create widget templates in `/liquid/widgets/`:

**Hero Widget (`widgets/hero.liquid`):**
```liquid
<section class="hero-widget" style="
  background-color: {{ Target.backgroundColor | default: '#0d6efd' }};
  color: {{ Target.textColor | default: '#ffffff' }};
  min-height: {{ Target.minHeight | default: 400 }}px;
  display: flex;
  align-items: center;
">
  <div class="container py-5 text-{{ Target.alignment | default: 'center' }}">
    {% if Target.headline != blank %}
      <h1 class="display-3 fw-bold mb-3">{{ Target.headline | escape }}</h1>
    {% endif %}
    
    {% if Target.subheadline != blank %}
      <p class="lead mb-4 opacity-75">{{ Target.subheadline | escape }}</p>
    {% endif %}
    
    {% if Target.buttonText != blank %}
      <a href="{{ Target.buttonUrl | default: '#' }}" 
         class="btn btn-{{ Target.buttonStyle | default: 'light' }} btn-lg px-4">
        {{ Target.buttonText | escape }}
      </a>
    {% endif %}
  </div>
</section>
```

**CTA Widget (`widgets/cta.liquid`):**
```liquid
<section class="cta-widget py-5" style="
  background-color: {{ Target.backgroundColor | default: '#f8f9fa' }};
  color: {{ Target.textColor | default: '#212529' }};
">
  <div class="container text-{{ Target.alignment | default: 'center' }}">
    {% if Target.headline != blank %}
      <h2 class="h1 fw-bold mb-3">{{ Target.headline | escape }}</h2>
    {% endif %}
    
    {% if Target.content != blank %}
      <p class="lead mb-4">{{ Target.content | escape }}</p>
    {% endif %}
    
    {% if Target.buttonText != blank %}
      <a href="{{ Target.buttonUrl | default: '#' }}" 
         class="btn btn-{{ Target.buttonStyle | default: 'primary' }} btn-lg">
        {{ Target.buttonText | escape }}
      </a>
    {% endif %}
  </div>
</section>
```

**Content List Widget (`widgets/content_list.liquid`):**
```liquid
<div class="content-list-widget">
  {% if Target.headline != blank %}
    <div class="text-center mb-5">
      <h2 class="display-6 fw-bold">{{ Target.headline | escape }}</h2>
      {% if Target.subheadline != blank %}
        <p class="lead text-muted">{{ Target.subheadline | escape }}</p>
      {% endif %}
    </div>
  {% endif %}
  
  {% if Target.items.size > 0 %}
    <div class="row g-4">
      {% for item in Target.items %}
        <div class="col-md-4">
          <div class="card h-100 shadow-sm">
            {% if Target.showImage and item.PublishedContent.image != blank %}
              <img src="{{ item.PublishedContent.image | attachment_redirect_url }}" 
                   class="card-img-top" alt="{{ item.PrimaryField | escape }}">
            {% endif %}
            <div class="card-body">
              <h5 class="card-title">{{ item.PrimaryField | escape }}</h5>
              {% if Target.showExcerpt and item.PublishedContent.summary != blank %}
                <p class="card-text">{{ item.PublishedContent.summary | truncate: 100 }}</p>
              {% endif %}
            </div>
          </div>
        </div>
      {% endfor %}
    </div>
  {% endif %}
</div>
```

## Benefits of Site Pages with Widgets

- **Flexible layouts**: Rearrange widgets without changing templates
- **Reusable components**: Same widget templates work across multiple pages
- **Content editor friendly**: Non-technical users can update content easily
- **Consistent design**: Widget templates enforce design standards
- **Easy A/B testing**: Swap widgets to test different layouts
