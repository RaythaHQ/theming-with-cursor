# Raytha Template Instructions (for Cursor)

Use this as a reference when generating or editing Raytha templates.

---

## 1. Templating engine

- Raytha templates use **Liquid** syntax rendered via the **.NET Fluid** library.
- For general Liquid behavior / filters, refer to Shopify's Liquid docs.
- Raytha adds its own objects, functions, and filters on top.

---

## 2. Themes & template storage

- Templates are organized into **Themes**.
- A theme is a collection of:
  - **Web Templates**: Liquid templates that render pages
  - **Widget Templates**: Templates for Site Page widgets
  - **Media Assets**: CSS, JavaScript, images, and fonts
- Only one theme can be **active** at a time.
- Manage themes in **Admin → Design → Themes**.
- There is a default parent layout template named **`_Layout`** or **`raytha_html_base_layout`**.

---

## 2.5 Available field types

Raytha supports the following field types (developer names):

| Field Type              | Developer Name            | Notes                                      |
|-------------------------|---------------------------|--------------------------------------------|
| Single Line Text        | `single_line_text`        | Short text input                           |
| Long Text               | `long_text`               | Multi-line text input                      |
| WYSIWYG                 | `wysiwyg`                 | Rich text editor                           |
| Dropdown                | `dropdown`                | Single-select with choices                 |
| Radio Buttons           | `radio`                   | Single-select with choices                 |
| Multiple Select         | `multiple_select`         | Multi-select with choices                  |
| Checkbox                | `checkbox`                | Boolean (true/false)                       |
| Date                    | `date`                    | Date picker                                |
| Number                  | `number`                  | Numeric input                              |
| Attachment              | `attachment`              | File upload                                |
| One-to-One Relationship | `one_to_one_relationship` | Relates to another content item            |

---

## 3. Parent / child templates

- Templates can **inherit** from a parent template.
- A template can itself be a **parent**.

Rules:

- A **parent template must include**:

  ```liquid
  {% renderbody %}
  ```

  This is where the child template's content will be rendered. If it is missing on a parent, Raytha will throw an error.

- Template inheritance is limited to **5 levels deep**.
- When creating a template:
  - Select a parent layout (usually `_Layout`) if appropriate.
  - Mark as "parent" only if it is meant to be inherited from.
  - Never forget `{% renderbody %}` in parents.

---

## 4. Core variables

Raytha exposes a number of variables in the template context.

### 4.1 Common top-level variables

| Variable                               | Type      | Notes / Example                                      |
|----------------------------------------|-----------|------------------------------------------------------|
| `QueryParams`                          | Key-Value | `{{ QueryParams["yourParam"] }}`                     |
| `PathBase`                             | String    | Base path if site is hosted at a route (e.g. `/app`) |
| `CurrentUser.IsAuthenticated`          | Boolean   |                                                      |
| `CurrentUser.IsAdmin`                  | Boolean   |                                                      |
| `CurrentUser.Roles`                    | Array     |                                                      |
| `CurrentUser.UserGroups`               | Array     | Array of group developer names                       |
| `CurrentUser.LastModificationTime`     | Date      |                                                      |
| `CurrentUser.UserId`                   | String    |                                                      |
| `CurrentUser.FirstName`                | String    |                                                      |
| `CurrentUser.LastName`                 | String    |                                                      |
| `CurrentUser.EmailAddress`             | String    |                                                      |
| `CurrentOrganization.OrganizationName` | String    |                                                      |
| `CurrentOrganization.WebsiteUrl`       | String    |                                                      |
| `CurrentOrganization.TimeZone`         | String    |                                                      |
| `CurrentOrganization.DateFormat`       | String    |                                                      |
| `ContentType.LabelPlural`              | String    | Human-readable plural name                           |
| `ContentType.LabelSingular`            | String    | Human-readable singular name                         |
| `ContentType.DeveloperName`            | String    | Developer name of the content type                   |
| `ContentType.Description`              | String    |                                                      |
| `Target`                               | Object    | Main view context (list, detail, or site page)       |
| `Target.Items`                         | Array     | Items in list views                                  |

### 4.2 Request information

```liquid
{{ Request.Path }}
{{ Request.QueryString }}
{{ Request.Host }}
```

---

## 5. `Target` and view types

Every public Raytha page is one of:

- A **list view** of a content type
- A **detail view** of a single content item
- A **Site Page** with widget zones

`Target` behavior:

- **List view:**
  - `Target` is a list context object.
  - `Target.Items` is an array of content items.
- **Detail view:**
  - `Target` is the content item itself.
- **Site Page:**
  - `Target` is the site page object with `Id`, `Title`, `RoutePath`, `IsPublished`.

---

## 6. List views

A list view (per content type) allows you to:

- Publish / unpublish the view.
- Optionally set it as the **home page**.
- Configure:
  - Route path, e.g. `/posts`
  - Base filter (OData)
  - Sort order
  - Default pagination
  - Which columns show & are searchable
  - Which **template** is used to render it

You can have multiple list views per content type; each can have its own template and route.

### 6.1 Looping over `Target.Items`

Typical pattern for rendering items:

```liquid
{% for item in Target.Items %}
    <div class="ud-single-blog">
        <div class="ud-blog-content">
            <span class="ud-blog-date">
                {{ item.CreationTime | date: "%b %d, %Y" }}
            </span>

            <h2 class="ud-blog-title">
                <a href="/{{ item.RoutePath }}">
                    {{ item.PrimaryField }}
                </a>
            </h2>

            {% if item.PublishedContent.content %}
                <div class="ud-blog-desc">
                    {{ item.PublishedContent.content | strip_html | truncate: 280, "..." }}
                    <a href="/{{ item.RoutePath }}">read more</a>
                </div>
            {% endif %}
        </div>
    </div>

    {% unless forloop.last %}
        <hr/>
    {% endunless %}
{% endfor %}
```

Common item properties:

- `item.PrimaryField`
- `item.RoutePath`
- `item.CreationTime`
- `item.PublishedContent.<fieldDeveloperName>`

### 6.2 Pagination UI

Raytha exposes pagination info on `Target`:

- `Target.TotalCount`
- `Target.RoutePath`
- `Target.PageNumber`
- `Target.TotalPages`
- `Target.PageSize`
- `Target.FirstVisiblePageNumber`
- `Target.LastVisiblePageNumber`
- `Target.PreviousDisabledCss`
- `Target.NextDisabledCss`
- `Target.HasPreviousPage`
- `Target.HasNextPage`

Example pagination snippet:

```liquid
<nav aria-label="page navigation" class="py-4">
    {% if Target.TotalCount == 1 %}
        <p>{{ Target.TotalCount }} result</p>
    {% else %}
        <p>{{ Target.TotalCount }} results</p>
    {% endif %}

    <ul class="pagination">
        <li class="page-item {% if Target.PreviousDisabledCss %}disabled{% endif %}">
            <a href="/{{ Target.RoutePath }}?pageNumber={{ Target.PageNumber | minus: 1 }}" class="page-link">
                «
            </a>
        </li>

        {% if Target.FirstVisiblePageNumber > 1 %}
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        {% endif %}

        {% for i in (Target.FirstVisiblePageNumber..Target.LastVisiblePageNumber) %}
            <li class="page-item {% if Target.PageNumber == i %}active{% endif %}">
                <a href="/{{ Target.RoutePath }}?pageNumber={{ i }}" class="page-link">
                    {{ i }}
                </a>
            </li>
        {% endfor %}

        {% if Target.LastVisiblePageNumber < Target.TotalPages %}
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        {% endif %}

        <li class="page-item {% if Target.NextDisabledCss %}disabled{% endif %}">
            <a href="/{{ Target.RoutePath }}?pageNumber={{ Target.PageNumber | plus: 1 }}" class="page-link">
                »
            </a>
        </li>
    </ul>
</nav>
```

---

## 7. Detail views

A **detail view** is tied to a single content item.

- You can choose which **detail template** a content item uses from the content item's settings.
- In a detail view, `Target` is that content item.

Common usage:

- `Target.Id`
- `Target.PrimaryField`
- `Target.RoutePath`
- `Target.IsPublished`
- `Target.IsDraft`
- `Target.CreationTime`
- `Target.LastModificationTime`
- `Target.PublishedContent.<field>.Text`
- `Target.PublishedContent.<field>.Value`

### 7.1 `.Text` vs `.Value` on fields

Fields on `Target.PublishedContent.<fieldName>` usually expose:

- `.Text`  
- `.Value`

Behavior by field type:

| Field Type       | `.Text`                                    | `.Value`                                |
|------------------|--------------------------------------------|-----------------------------------------|
| Checkbox         | `"true"` or `"false"` (string)             | `true` or `false` (boolean)             |
| Dropdown         | Choice **label** (e.g., "Getting Started") | Choice **developer name** (e.g., "getting_started") |
| Radio            | Choice **label** (e.g., "Beginner")        | Choice **developer name** (e.g., "beginner") |
| Multiple Select  | Comma-separated labels (string)            | Array of developer names                |
| Single Line Text | Text value                                 | Text value (same as .Text)              |
| Long Text        | Text value                                 | Text value (same as .Text)              |
| WYSIWYG          | HTML content                               | HTML content (same as .Text)            |
| Number           | String representation                      | Numeric value                           |
| Date             | Formatted date string                      | DateTime object                         |
| Attachment       | Attachment key                             | Attachment key (same as .Text)          |

**Critical for filtering:**

- When filtering on **dropdown**, **radio**, or **multiple_select** fields, you MUST use the **developer name** (`.Value`), NOT the label (`.Text`)!

Examples:

```liquid
{% comment %} ❌ WRONG - using the label {% endcomment %}
<a href="/articles?filter=category eq 'Getting Started'">  {% comment %} Won't work! {% endcomment %}

{% comment %} ✅ CORRECT - using the developer name {% endcomment %}
<a href="/articles?filter=category eq 'getting_started'">  {% comment %} Works! {% endcomment %}

{% comment %} Dynamic filter using .Value (developer name) but displaying .Text (label) {% endcomment %}
<a href="{{ PathBase }}/articles?filter=category eq '{{ item.PublishedContent.category.Value }}'">
  More in {{ item.PublishedContent.category.Text }}
</a>
```

**General rule:**
- Use `.Value` for logic, filtering, and comparisons
- Use `.Text` for display to users

### 7.2 One-to-one relationships

Given a one-to-one field `author_1` pointing to an `Author` content type:

- `{{ Target.PublishedContent.author_1 }}`  
  Outputs the **primary field** of the related author.

To access nested fields:

```liquid
{{ Target.PublishedContent.author_1.PublishedContent.twitter_handle.Text }}
{{ Target.PublishedContent.author_1.RoutePath }}
```

More complete example:

```liquid
{% if Target.PublishedContent.hide_author_bio.Value == false %}
    <hr/>
    <div class="card mb-3" style="max-width: 100%;">
        <div class="row g-0">
            <div class="col-md-4">
                <img
                    src="{{ Target.PublishedContent.author_1.PublishedContent.profile_pic.Value | attachment_redirect_url }}"
                    class="img-fluid rounded-start"
                    alt="picture of the author"
                />
            </div>

            <div class="col-md-8">
                <div class="card-body">
                    <h5 class="card-title">
                        {{ Target.PublishedContent.author_1 }}
                        {% if Target.PublishedContent.author_1.PublishedContent.twitter_handle.Text %}
                            <span>
                                <a
                                    href="https://twitter.com/{{ Target.PublishedContent.author_1.PublishedContent.twitter_handle.Text }}"
                                    target="_blank"
                                >
                                    @{{ Target.PublishedContent.author_1.PublishedContent.twitter_handle.Text }}
                                </a>
                            </span>
                        {% endif %}
                    </h5>

                    <p class="card-text">
                        {{ Target.PublishedContent.author_1.PublishedContent.bio.Text }}
                    </p>
                </div>
            </div>
        </div>
    </div>
{% endif %}
```

> Limitation: nested related content only goes **1 level deep**. For deeper pulls, use the custom functions described below.

---

## 8. Site Pages & Widget Templates

Site Pages allow you to build pages using **widgets** arranged in **zones**.

### 8.1 What is a Site Page?

A Site Page:
- Has a route path and can be published/unpublished
- Contains one or more **widget zones** (e.g., "hero", "main", "sidebar")
- Each zone can contain multiple **widgets**
- Widgets are rendered using **widget templates**

### 8.2 Site Page Target variables

When rendering a Site Page template:

```liquid
{{ Target.Id }}
{{ Target.Title }}
{{ Target.IsPublished }}
{{ Target.RoutePath }}
```

### 8.3 Rendering widget zones

Use the `render_zone` function to render widgets in a zone:

```liquid
{{ render_zone "hero" }}
{{ render_zone "main" }}
{{ render_zone "sidebar" }}
```

### 8.4 Built-in widget types

| Widget          | Developer Name   | Purpose                                    |
|-----------------|------------------|--------------------------------------------|
| **Hero**        | `hero`           | Large banner sections with headline and CTA |
| **WYSIWYG**     | `wysiwyg`        | Rich text content blocks                   |
| **Card**        | `card`           | Bordered content cards with image and button |
| **CTA**         | `cta`            | Call-to-action sections                    |
| **Content List**| `content_list`   | Dynamic lists of content items             |

### 8.5 Widget template Target variables

Each widget type has its own `Target` context with settings configured by the user.

**Hero Widget:**
```liquid
{{ Target.headline }}
{{ Target.subheadline }}
{{ Target.backgroundColor }}
{{ Target.textColor }}
{{ Target.buttonText }}
{{ Target.buttonUrl }}
{{ Target.buttonStyle }}
{{ Target.alignment }}
{{ Target.minHeight }}
{{ Target.backgroundImage }}
```

**WYSIWYG Widget:**
```liquid
{{ Target.content }}
{{ Target.padding }}
```

**Card Widget:**
```liquid
{{ Target.title }}
{{ Target.description }}
{{ Target.imageUrl }}
{{ Target.buttonText }}
{{ Target.buttonUrl }}
{{ Target.buttonStyle }}
```

**CTA Widget:**
```liquid
{{ Target.headline }}
{{ Target.content }}
{{ Target.buttonText }}
{{ Target.buttonUrl }}
{{ Target.buttonStyle }}
{{ Target.backgroundColor }}
{{ Target.textColor }}
{{ Target.alignment }}
```

**Content List Widget:**
```liquid
{{ Target.headline }}
{{ Target.subheadline }}
{{ Target.contentType }}
{{ Target.pageSize }}
{{ Target.displayStyle }}
{{ Target.showImage }}
{{ Target.showDate }}
{{ Target.showExcerpt }}
{{ Target.linkText }}
{{ Target.linkUrl }}

{% for item in Target.items %}
  {{ item.Id }}
  {{ item.PrimaryField }}
  {{ item.RoutePath }}
  {{ item.CreationTime }}
  {{ item.PublishedContent.field_name }}
{% endfor %}
```

### 8.6 Widget template best practices

**Check for blank values:**
```liquid
{% if Target.headline != blank %}
  <h2>{{ Target.headline }}</h2>
{% endif %}
```

**Provide default values:**
```liquid
<section style="background-color: {{ Target.backgroundColor | default: '#ffffff' }};">
  ...
</section>
```

**Escape user content:**
```liquid
<h1>{{ Target.headline | escape }}</h1>
```

### 8.7 Complete Hero widget example

```liquid
<section class="hero-widget" style="
  background-color: {{ Target.backgroundColor | default: '#0d6efd' }};
  color: {{ Target.textColor | default: '#ffffff' }};
  min-height: {{ Target.minHeight | default: 400 }}px;
  {% if Target.backgroundImage != blank %}
  background-image: url('{{ Target.backgroundImage }}');
  background-size: cover;
  background-position: center;
  {% endif %}
">
  <div class="container py-5">
    <div class="row justify-content-center">
      <div class="col-lg-8 text-{{ Target.alignment | default: 'center' }}">
        
        {% if Target.headline != blank %}
          <h1 class="display-4 fw-bold mb-3">
            {{ Target.headline | escape }}
          </h1>
        {% endif %}
        
        {% if Target.subheadline != blank %}
          <p class="lead mb-4">
            {{ Target.subheadline | escape }}
          </p>
        {% endif %}
        
        {% if Target.buttonText != blank %}
          <a href="{{ Target.buttonUrl | default: '#' }}" 
             class="btn btn-{{ Target.buttonStyle | default: 'light' }} btn-lg"
             {% if Target.buttonOpenNewTab %}target="_blank" rel="noopener"{% endif %}>
            {{ Target.buttonText | escape }}
          </a>
        {% endif %}
        
      </div>
    </div>
  </div>
</section>
```

---

## 9. OData usage in templates (public list views)

Raytha uses **OData-style** query parameters on public list views to allow:

- Additional filtering on top of the base filter
- Sorting
- Pagination
- Search

These operate on the public route of a **list view**, e.g. `/blog`.

### 9.1 Admin-configured base filter

- When you create/edit a list view in the admin portal, Raytha stores an OData filter query internally.
- On public requests, Raytha:
  1. Applies the base filter configured in the view.
  2. Optionally merges additional filters from the **query string**.

You can **disable** client-side filter/sort by checking:

- `Ignore client side filter and sort query parameters`

### 9.2 `filter` query parameter

**CRITICAL RULES:**

1. For filtering on public list views, you MUST use the `filter` parameter with OData syntax
2. You CANNOT use simple query parameters like `?category=value`
3. For dropdown/radio/multiple_select fields, you MUST use the **choice's developer name**, NOT its label

**❌ WRONG - Simple query parameters (will NOT work):**
```
/articles?category=Billing
/articles?difficulty=Beginner
```

**❌ WRONG - Using choice labels instead of developer names:**
```
/articles?filter=category eq 'Getting Started'  ← Label, won't work!
/articles?filter=category eq 'API & Development'  ← Label, won't work!
/articles?filter=difficulty eq 'Beginner'  ← Label, won't work!
```

**✅ CORRECT - OData filter with developer names:**
```
/articles?filter=category eq 'getting_started'  ← Developer name, works!
/articles?filter=category eq 'api_development'  ← Developer name, works!
/articles?filter=difficulty eq 'beginner'  ← Developer name, works!
```

**More examples:**

- Filter by primary field:
  ```
  /blog?filter=PrimaryField eq 'Release of v1.0.4'
  ```

- Filter by creation time:
  ```
  /blog?filter=CreationTime lt '3/1/2023'
  ```

- Combine conditions:
  ```
  /blog?filter=CreationTime lt '3/1/2023' and youtube_video ne ''
  ```

- Use parentheses / nested conditions:
  ```
  /blog?filter=(CreationTime lt '3/1/2023' and youtube_video ne '') or hide_author_bio eq 'true'
  ```

- Use `contains` for partial matches:
  ```
  /blog?filter=contains(title, 'open source')
  ```

- Filter on multiple values (OR condition):
  ```
  /articles?filter=category eq 'Billing' or category eq 'Integrations'
  ```

**You can filter on:**

- System fields:
  - `PrimaryField`
  - `CreationTime`
  - `LastModificationTime`
  - `IsPublished`
  - `IsDraft`
- Any content field by developer name (e.g., `category`, `difficulty`, `author`, etc.)

**Supported operators:**

- Logical: `and`, `or`
- Comparison: `eq` (equals), `ne` (not equal), `lt` (less than), `lte` (less than or equal), `gt` (greater than), `gte` (greater than or equal)
- Functions: `contains(field, '...')`, `startswith(field, '...')`, `endswith(field, '...')`
- `null` checks: e.g. `category ne null`

**Common filter link patterns in Liquid templates:**

```liquid
{% comment %} ❌ WRONG - Using choice labels {% endcomment %}
<a href="{{ PathBase }}/articles?filter=category eq 'Getting Started'">Getting Started</a>
<a href="{{ PathBase }}/articles?filter=difficulty eq 'Beginner'">Beginner</a>

{% comment %} ✅ CORRECT - Using choice developer names (hardcoded) {% endcomment %}
<a href="{{ PathBase }}/articles?filter=category eq 'getting_started'">Getting Started</a>
<a href="{{ PathBase }}/articles?filter=difficulty eq 'beginner'">Beginner</a>

{% comment %} ✅ CORRECT - Dynamic filter using .Value (developer name) {% endcomment %}
<a href="{{ PathBase }}/articles?filter=category eq '{{ Target.PublishedContent.category.Value }}'">
  More in {{ Target.PublishedContent.category.Text }}
</a>

{% comment %} ✅ CORRECT - Multiple conditions with developer names {% endcomment %}
<a href="{{ PathBase }}/articles?filter=category eq 'billing' and difficulty eq 'beginner'">
  Beginner Billing Articles
</a>

{% comment %} ✅ CORRECT - Filtering on text fields uses actual text values {% endcomment %}
<a href="{{ PathBase }}/blog?filter=contains(title, 'API')">API Posts</a>
<a href="{{ PathBase }}/users?filter=email eq 'user@example.com'">Find User</a>
```

**Key distinction:**
- **Choice fields (dropdown/radio/multiple_select)**: Use developer name from `.Value`
- **Text fields (single_line_text/long_text/wysiwyg)**: Use actual text content
- **Number/Date fields**: Use the appropriate value format

### 9.3 `orderby` query parameter

Use the `orderby` parameter to change sort order:

- Single field:

  ```
  /blog?orderby=title asc
  ```

- Multiple fields:

  ```
  /blog?orderby=youtube_video desc,CreationTime asc
  ```

This overrides the sort configured in the admin view.

### 9.4 Pagination: `pageSize` & `pageNumber`

Use:

- `pageSize`
- `pageNumber`

Examples:

- `/blog?pageSize=1&pageNumber=2`
- `/blog?pageSize=5`

### 9.5 Search: `search` parameter

- List view search in admin is based on the columns selected for the view.
- Public side uses the same search across those columns.
- Use simple HTML forms with GET method - no JavaScript required!

**URL example:**

```
/blog?search=release
/articles?search=api authentication
```

**Recommended implementation pattern - Simple HTML form:**

```liquid
{% comment %} Search form on home page - directs to list view {% endcomment %}
<form action="{{ PathBase }}/articles" method="get">
  <input 
    type="search" 
    name="search"
    placeholder="Search for articles..."
    aria-label="Search"
  >
  <button type="submit">Search</button>
</form>

{% comment %} Search form on list view page - stays on same page {% endcomment %}
<form action="{{ PathBase }}/{{ ContentType.DeveloperName }}" method="get">
  <input 
    type="search" 
    name="search"
    placeholder="Search..."
    aria-label="Search"
    value="{{ QueryParams['search'] }}"
  >
  <button type="submit">Search</button>
</form>
```

**Key points:**

1. **Use `method="get"`** - This creates URL query parameters automatically
2. **Use `name="search"`** - Raytha looks for this parameter name
3. **Use `type="search"`** - Better semantics than `type="text"`
4. **Preserve search term** - On list pages, use `value="{{ QueryParams['search'] }}"` to show the current search
5. **No JavaScript needed** - Form submission handles everything

**Alternative: Search without submit button**

If you prefer search to submit on Enter key press only (no button):

```liquid
<form action="{{ PathBase }}/articles" method="get">
  <input 
    type="search" 
    name="search"
    placeholder="Search for articles..."
    aria-label="Search"
  >
</form>
```

> **Note:** If you need to search columns not included in the view configuration, use a custom `filter` with `contains()` instead of `search`. Example: `?filter=contains(title, 'term') or contains(summary, 'term')`

---

## 10. Custom functions (Liquid functions)

> Every function call hits the database. Heavy use in a single template can impact performance.

### 10.1 `get_content_item_by_id(contentItemId)`

- Returns a single content item by its **id**.

Example:

```liquid
{% assign other_related_item = get_content_item_by_id(Target.PublishedContent.related_item.Value) %}

{% if other_related_item %}
    <h3>Related: {{ other_related_item.PrimaryField }}</h3>
    <p>{{ other_related_item.PublishedContent.summary.Text }}</p>
{% endif %}
```

### 10.2 `get_content_items(ContentType='developer_name', Filter='odata', OrderBy='odata', PageNumber=1, PageSize=25)`

- Returns items for a specific content type.
- **IMPORTANT**: Returns an object with `.Items` (array) and `.TotalCount`, not a direct array!
- Parameters (use named parameter syntax with single quotes):
  - `ContentType='name'` (required): developer name of content type.
  - `Filter='odata'` (optional): OData filter string.
  - `OrderBy='field desc'` (optional): OData style order by string.
  - `PageNumber=1` (optional): default 1.
  - `PageSize=25` (optional): default 25.

**Correct syntax examples:**

```liquid
{% comment %} Simple example - get 6 most recent articles {% endcomment %}
{% assign articles = get_content_items(ContentType='articles', OrderBy='CreationTime desc', PageSize=6) %}

{% if articles.TotalCount > 0 %}
    {% for item in articles.Items %}
        {{ item.PrimaryField }}
    {% endfor %}
{% endif %}
```

```liquid
{% comment %} Complex example with filter {% endcomment %}
{% assign filter = "contains(user_guide,'" | append: Target.PrimaryField | append: "')" %}
{% assign items = get_content_items(
    ContentType='posts',
    Filter=filter,
    OrderBy='order_to_appear_in_user_guide asc',
    PageSize=25
) %}

{% if items.TotalCount > 0 %}
    <div id="articles">
        <h2>Table of Contents</h2>
        <ol>
            {% for item in items.Items %}
                <li>
                    <a href="{{ PathBase }}/{{ item.RoutePath }}" target="_blank">
                        {{ item.PrimaryField }}
                    </a>
                </li>
            {% endfor %}
        </ol>
    </div>
{% endif %}
```

**Common mistakes to avoid:**

```liquid
{% comment %} ❌ WRONG - using Ruby-style colon syntax {% endcomment %}
{% assign items = get_content_items_by: "articles", "limit", 6 %}

{% comment %} ❌ WRONG - function name doesn't exist {% endcomment %}
{% assign items = get_content_items_by("articles") %}

{% comment %} ❌ WRONG - iterating result directly instead of .Items {% endcomment %}
{% for item in items %}  {% comment %} Should be: items.Items {% endcomment %}

{% comment %} ❌ WRONG - checking .size instead of .TotalCount {% endcomment %}
{% if items.size > 0 %}  {% comment %} Should be: items.TotalCount {% endcomment %}

{% comment %} ✅ CORRECT - proper named parameters and iteration {% endcomment %}
{% assign items = get_content_items(ContentType='articles', PageSize=6) %}
{% if items.TotalCount > 0 %}
    {% for item in items.Items %}
        {{ item.PrimaryField }}
    {% endfor %}
{% endif %}
```

### 10.3 `get_content_type_by_developer_name(contentTypeDeveloperName)`

- Retrieves content type metadata including field definitions.

Example:

```liquid
{% assign contentType = get_content_type_by_developer_name('posts') %}
{% assign categoriesField = contentType.ContentTypeFields | where: "DeveloperName", "categories" | first %}

{% for choice in categoriesField.Choices %}
    <a href="{{ PathBase }}/{{ choice.DeveloperName }}">
        {{ choice.Label }}
    </a>
{% endfor %}
```

### 10.4 `get_main_menu()` and `get_menu('developerName')`

- `get_main_menu()` returns the navigation menu marked as default.
- `get_menu('developerName')` returns a named menu.

Example:

```liquid
{% assign menu = get_main_menu() %}

<ul class="navbar-nav me-auto mb-2 mb-lg-0">
    {% for menuItem in menu.MenuItems %}
        {% assign menuLabelDownCase = menuItem.Label | downcase %}
        <li class="nav-item">
            <a
                class="{{ menuItem.CssClassName }} {% if Target.RoutePath == menuItem.Label or ContentType.DeveloperName == menuLabelDownCase %}active{% endif %}"
                href="{{ menuItem.Url }}"
                {% if menuItem.OpenInNewTab %}target="_blank"{% endif %}
            >
                {{ menuItem.Label }}
            </a>
        </li>
    {% endfor %}
</ul>

{% assign footerMenu = get_menu('footer') %}
```

Menu item model:

```text
string Id
string Label
string Url
bool IsDisabled
bool OpenInNewTab
string? CssClassName
int Ordinal
bool IsFirstItem
bool IsLastItem
IEnumerable<NavigationMenuItem_RenderModel> MenuItems
```

You can iterate nested `MenuItems` for dropdowns / sub-menus.

### 10.5 `render_zone` (Site Pages only)

Renders all widgets in a named zone:

```liquid
{{ render_zone "hero" }}
{{ render_zone "main" }}
{{ render_zone "sidebar" }}
```

---

## 11. Custom filters

### 11.1 `attachment_redirect_url`

- Input: attachment field `.Value`.
- Output: URL relative to the current site:
  - `/raytha/media-items/objectkey/{key}`
- When requested, Raytha:
  - Issues a **302 redirect** to the underlying storage location.
  - Generates a presigned / SAS URL.
- Useful when you want storage private but accessible via Raytha.

Example:

```liquid
{{ Target.PublishedContent.attachment.Value | attachment_redirect_url }}
```

> Note: Many such links on a page means many extra redirect requests.

### 11.2 `attachment_public_url`

- Input: attachment field `.Value`.
- Output: direct URL to file on the storage provider.
- No presigned / SAS logic. Your storage bucket must allow **anonymous read**.

Example:

```liquid
{{ Target.PublishedContent.attachment.Value | attachment_public_url }}
```

### 11.3 `raytha_attachment_url` (deprecated)

- Old name for `attachment_redirect_url`.
- Scheduled to be removed in v1.0.6.
- Prefer `attachment_redirect_url`.

### 11.4 `organization_time`

- Converts a DateTime into the organization's configured timezone.
- Common with `CreationTime`, `LastModificationTime`, etc.
- Often combined with Liquid's `date` filter.

Example:

```liquid
{{ item.CreationTime | organization_time | date: "%c" }}
```

### 11.5 `groupby`

Common usage:

```liquid
{% assign grouped_items = Target.Items | groupby: "PublishedContent.developer_name" %}

{% for grouped_item in grouped_items %}
    {{ grouped_item.key }}
    {% for item in grouped_item.items %}
        {{ item.PublishedContent.developer_name.Value }}
    {% endfor %}
{% endfor %}
```

- Groups `Target.Items` by `PublishedContent.developer_name`.
- Each `grouped_item` has:
  - `key`
  - `items` (array of items in that group)

### 11.6 `json`

- Dumps any object as JSON-like output.
- Very useful for debugging to see the shape of objects.

Example:

```liquid
{{ Target | json }}
```

Use this during development and remove it from production templates.

### 11.7 `get_navigation_menu` (filter)

Retrieve a navigation menu by developer name:

```liquid
{% assign footer = 'footer_menu' | get_navigation_menu %}
{% for item in footer.Items %}
  <a href="{{ item.Url }}">{{ item.Label }}</a>
{% endfor %}
```

---

## 12. General guidelines for Cursor

When generating Raytha templates:

1. **Use Liquid syntax** compatible with Fluid.
2. For **layout/parent templates**, always include `{% renderbody %}`.
3. Use `Target` appropriately:
   - List views: iterate `Target.Items`.
   - Detail views: treat `Target` as the item.
   - Site Pages: use `render_zone` to output widget zones.
   - Widget templates: access settings via `Target.<settingName>`.
4. Prefer `.Value` for logic, `.Text` for display.
5. Be mindful that function calls (`get_content_items`, etc.) hit the DB.
6. Use OData (`filter`, `orderby`, `pageSize`, `pageNumber`, `search`) only on **public list view URLs**, not inside Liquid.
7. Use `attachment_redirect_url` by default for secured files; `attachment_public_url` only when storage is publicly readable.
8. Use `organization_time` when showing times that should respect the org timezone.
9. Use `json` during development to inspect objects, then remove it in final templates.
10. In widget templates:
    - Always check for blank values before rendering
    - Use the `default` filter for fallback values
    - Escape user content with `| escape`

---

## 13. Common Mistakes to Avoid

### 13.1 Filtering mistakes

❌ **WRONG - Using simple query parameters:**
```liquid
<a href="/articles?category=billing">Billing Articles</a>
<a href="/articles?difficulty=beginner">Beginner Articles</a>
```

❌ **WRONG - Using OData but with choice labels instead of developer names:**
```liquid
<a href="/articles?filter=category eq 'Billing'">Billing</a>
<a href="/articles?filter=category eq 'Getting Started'">Getting Started</a>
<a href="/articles?filter=difficulty eq 'Beginner'">Beginner</a>
```

❌ **WRONG - Using .Text (label) instead of .Value (developer name) in dynamic filters:**
```liquid
<a href="/articles?filter=category eq '{{ item.PublishedContent.category.Text }}'">
  {% comment %} .Text gives "Getting Started", but filter needs "getting_started" {% endcomment %}
</a>
```

✅ **CORRECT - Using OData filter with choice developer names:**
```liquid
<a href="/articles?filter=category eq 'billing'">Billing Articles</a>
<a href="/articles?filter=category eq 'getting_started'">Getting Started</a>
<a href="/articles?filter=difficulty eq 'beginner'">Beginner Articles</a>
```

✅ **CORRECT - Using .Value (developer name) for dynamic filters:**
```liquid
<a href="/articles?filter=category eq '{{ item.PublishedContent.category.Value }}'">
  More {{ item.PublishedContent.category.Text }} Articles
</a>
```

### 13.2 Function call mistakes

❌ **WRONG - Incorrect function name or syntax:**
```liquid
{% assign items = get_content_items_by: "articles", "limit", 6 %}
{% assign items = get_items("articles") %}
```

✅ **CORRECT - Proper named parameters:**
```liquid
{% assign items = get_content_items(ContentType='articles', PageSize=6) %}
```

### 13.3 Iteration mistakes

❌ **WRONG - Iterating function result directly:**
```liquid
{% assign items = get_content_items(ContentType='articles') %}
{% for item in items %}  {% comment %} items is an object, not array! {% endcomment %}
```

✅ **CORRECT - Iterate .Items property:**
```liquid
{% assign items = get_content_items(ContentType='articles') %}
{% for item in items.Items %}  {% comment %} Correct! {% endcomment %}
```

### 13.4 Property check mistakes

❌ **WRONG - Using wrong property names:**
```liquid
{% if items.size > 0 %}  {% comment %} Should be .TotalCount {% endcomment %}
{% if items.length > 0 %}  {% comment %} Should be .TotalCount {% endcomment %}
```

✅ **CORRECT - Using proper property:**
```liquid
{% if items.TotalCount > 0 %}
```

### 13.5 Date formatting mistakes

❌ **WRONG - Using date filter without organization_time:**
```liquid
{{ item.CreationTime | date: "%B %d, %Y" }}  {% comment %} Ignores org timezone! {% endcomment %}
```

✅ **CORRECT - Convert to org timezone first:**
```liquid
{{ item.CreationTime | organization_time | date: "%B %d, %Y" }}
```

### 13.6 URL construction mistakes

❌ **WRONG - Hardcoded URLs without PathBase:**
```liquid
<a href="/articles">Articles</a>
<a href="/{{ item.RoutePath }}">Link</a>
```

✅ **CORRECT - Always include PathBase:**
```liquid
<a href="{{ PathBase }}/articles">Articles</a>
<a href="{{ PathBase }}/{{ item.RoutePath }}">Link</a>
```

### 13.7 Pagination URL mistakes

❌ **WRONG - Not preserving filter in pagination:**
```liquid
<a href="{{ PathBase }}/articles?pageNumber={{ Target.PageNumber | plus: 1 }}">Next</a>
```
This loses any active filters when user clicks pagination!

✅ **CORRECT - Use Target.RoutePath which preserves filters:**
```liquid
<a href="{{ PathBase }}/{{ Target.RoutePath }}?pageNumber={{ Target.PageNumber | plus: 1 }}">Next</a>
```

### 13.8 Search implementation mistakes

❌ **WRONG - Using JavaScript when HTML forms work:**
```liquid
<input type="text" id="search" onkeyup="handleSearch()">
<script>
  function handleSearch() { /* complex JS */ }
</script>
```

❌ **WRONG - Using POST method for search:**
```liquid
<form action="{{ PathBase }}/articles" method="post">
  {% comment %} POST doesn't create URL parameters! {% endcomment %}
```

❌ **WRONG - Wrong parameter name:**
```liquid
<form action="{{ PathBase }}/articles" method="get">
  <input type="search" name="query">  {% comment %} Should be "search" not "query" {% endcomment %}
</form>
```

✅ **CORRECT - Simple HTML form with GET method:**
```liquid
<form action="{{ PathBase }}/articles" method="get">
  <input type="search" name="search" placeholder="Search...">
</form>
```

✅ **CORRECT - Preserve search term on list page:**
```liquid
<form action="{{ PathBase }}/{{ ContentType.DeveloperName }}" method="get">
  <input 
    type="search" 
    name="search"
    value="{{ QueryParams['search'] }}"
    placeholder="Search..."
  >
</form>
```

### 13.9 Widget template mistakes

❌ **WRONG - Not handling empty values:**
```liquid
<h1>{{ Target.headline }}</h1>  {% comment %} Renders empty tag if blank {% endcomment %}
```

✅ **CORRECT - Conditional rendering:**
```liquid
{% if Target.headline != blank %}
  <h1>{{ Target.headline }}</h1>
{% endif %}
```

❌ **WRONG - Forgetting to escape user content:**
```liquid
<h1>{{ Target.headline }}</h1>  {% comment %} XSS vulnerability {% endcomment %}
```

✅ **CORRECT - Escaped:**
```liquid
<h1>{{ Target.headline | escape }}</h1>
```

❌ **WRONG - Hardcoded values instead of settings:**
```liquid
<section style="background-color: #0d6efd;">  {% comment %} Not customizable {% endcomment %}
```

✅ **CORRECT - Uses setting with fallback:**
```liquid
<section style="background-color: {{ Target.backgroundColor | default: '#0d6efd' }};">
```
