# ?? Modern Design System - Implementation Guide

Kompletny system designu dla ca�ej aplikacji AirlineManager z konsystentnymi komponentami i stylami.

## ?? Spis Tre�ci

1. [Instalacja](#instalacja)
2. [Komponenty](#komponenty)
3. [Przyk�ady U�ycia](#przyk�ady-u�ycia)
4. [Customizacja](#customizacja)
5. [Best Practices](#best-practices)

## ?? Instalacja

### 1. Dodaj CSS do Layout
```razor
<link rel="stylesheet" href="~/css/modern-design-system.css" asp-append-version="true" />
```

### 2. U�yj w Widokach
Komponenty s� automatycznie dost�pne przez klasy CSS.

## ?? Komponenty

### 1. **Moderne Karty**

#### Basic Card
```html
<div class="card-modern">
    <div class="card-body-modern">
        <h5>Tytu� Karty</h5>
   <p>Tre�� karty</p>
    </div>
</div>
```

#### Gradient Card
```html
<div class="card-modern card-gradient">
    <div class="card-header-modern">
   Header z Gradientem
    </div>
    <div class="card-body-modern">
        Tre�� w bia�ym kolorze
    </div>
</div>
```

#### Glass Effect Card
```html
<div class="card-modern card-glass">
 <div class="card-body-modern">
        Efekt szk�a (glass morphism)
    </div>
</div>
```

### 2. **Moderne Przyciski**

```html
<!-- Primary Gradient -->
<button class="btn-modern btn-gradient-primary">
    <i class="bi bi-check-circle me-2"></i>Save Changes
</button>

<!-- Success Gradient -->
<button class="btn-modern btn-gradient-success">
    <i class="bi bi-plus-circle me-2"></i>Create New
</button>

<!-- Danger Gradient -->
<button class="btn-modern btn-gradient-danger">
    <i class="bi bi-trash me-2"></i>Delete
</button>

<!-- Warning Gradient -->
<button class="btn-modern btn-gradient-warning">
    <i class="bi bi-exclamation-triangle me-2"></i>Warning
</button>

<!-- Info Gradient -->
<button class="btn-modern btn-gradient-info">
  <i class="bi bi-info-circle me-2"></i>Information
</button>
```

### 3. **Moderne Formularze**

#### Input z Ikon�
```html
<div class="input-group-modern">
    <span class="input-group-text">
     <i class="bi bi-person"></i>
    </span>
    <input type="text" class="form-control" placeholder="Username" />
</div>
```

#### Standalone Input
```html
<input type="email" class="form-control-modern" placeholder="Email address" />
```

### 4. **Moderne Alerty**

```html
<!-- Success Alert -->
<div class="alert-modern alert-gradient-success">
    <i class="bi bi-check-circle-fill"></i>
    <div>
     <strong>Success!</strong> Your changes have been saved.
  </div>
</div>

<!-- Warning Alert -->
<div class="alert-modern alert-gradient-warning">
    <i class="bi bi-exclamation-triangle-fill"></i>
    <div>
        <strong>Warning!</strong> This action cannot be undone.
    </div>
</div>

<!-- Danger Alert -->
<div class="alert-modern alert-gradient-danger">
    <i class="bi bi-x-circle-fill"></i>
    <div>
        <strong>Error!</strong> Something went wrong.
    </div>
</div>

<!-- Info Alert -->
<div class="alert-modern alert-gradient-info">
    <i class="bi bi-info-circle-fill"></i>
    <div>
        <strong>Info:</strong> Please review the details below.
    </div>
</div>
```

### 5. **Moderne Tabele**

```html
<table class="table table-modern">
    <thead>
        <tr>
            <th>Name</th>
 <th>Email</th>
          <th>Role</th>
       <th>Actions</th>
     </tr>
    </thead>
    <tbody>
 <tr>
            <td>John Doe</td>
            <td>john@example.com</td>
         <td>
          <span class="badge-modern badge-gradient-primary">Admin</span>
      </td>
        <td>
          <button class="btn btn-sm btn-modern btn-gradient-primary">Edit</button>
     </td>
    </tr>
    </tbody>
</table>
```

### 6. **Moderne Nawigacja (Taby)**

```html
<ul class="nav nav-modern" role="tablist">
    <li class="nav-item">
        <button class="nav-link active">
<i class="bi bi-house me-2"></i>Home
   </button>
    </li>
    <li class="nav-item">
        <button class="nav-link">
   <i class="bi bi-person me-2"></i>Profile
        </button>
    </li>
    <li class="nav-item">
        <button class="nav-link">
  <i class="bi bi-gear me-2"></i>Settings
  </button>
    </li>
</ul>
```

### 7. **Moderne Modale**

```html
<div class="modal fade" tabindex="-1">
    <div class="modal-dialog">
  <div class="modal-content modal-content-modern">
       <div class="modal-header modal-header-modern">
    <h5 class="modal-title">
          <i class="bi bi-info-circle me-2"></i>Modal Title
      </h5>
  <button type="button" class="btn-close btn-close-white"></button>
            </div>
        <div class="modal-body modal-body-modern">
  <p>Modal content goes here...</p>
            </div>
    <div class="modal-footer modal-footer-modern">
      <button class="btn btn-modern btn-gradient-primary">Save</button>
         <button class="btn btn-secondary">Close</button>
   </div>
   </div>
    </div>
</div>
```

### 8. **Moderne Badge**

```html
<span class="badge-modern badge-gradient-primary">New</span>
<span class="badge-modern badge-gradient-success">Active</span>
<span class="badge-modern badge-gradient-warning">Pending</span>
<span class="badge-modern badge-gradient-danger">Blocked</span>
```

### 9. **Moderne Pagination**

```html
<nav>
    <ul class="pagination pagination-modern">
      <li class="page-item"><a class="page-link" href="#">Previous</a></li>
  <li class="page-item"><a class="page-link" href="#">1</a></li>
        <li class="page-item active"><a class="page-link" href="#">2</a></li>
     <li class="page-item"><a class="page-link" href="#">3</a></li>
    <li class="page-item"><a class="page-link" href="#">Next</a></li>
    </ul>
</nav>
```

### 10. **Moderne Dropdown**

```html
<div class="dropdown">
    <button class="btn btn-modern btn-gradient-primary dropdown-toggle">
        Options
  </button>
    <ul class="dropdown-menu dropdown-menu-modern">
        <li><a class="dropdown-item" href="#">Action</a></li>
        <li><a class="dropdown-item" href="#">Another action</a></li>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item" href="#">Separated link</a></li>
    </ul>
</div>
```

## ?? Utility Classes

### Gradient Text
```html
<h1 class="gradient-text">Beautiful Gradient Text</h1>
```

### Hover Lift
```html
<div class="card hover-lift">
    <!-- Karta podnosi si� przy hover -->
</div>
```

### Glass Effect
```html
<div class="glass-effect">
    <!-- Efekt matowego szk�a -->
</div>
```

### Glow Effect
```html
<div class="glow-effect">
    <!-- Delikatny efekt �wiecenia -->
</div>
```

## ?? Animacje

### Fade In
```html
<div class="fade-in">
    <!-- Element pojawia si� z fade -->
</div>
```

### Slide In Up
```html
<div class="slide-in-up">
    <!-- Element wje�d�a od do�u -->
</div>
```

### Scale In
```html
<div class="scale-in">
    <!-- Element pojawia si� ze skalowaniem -->
</div>
```

## ?? Customizacja

### Zmiana Kolor�w Gradientu

```css
:root {
    --gradient-primary-start: #YOUR_COLOR;
    --gradient-primary-end: #YOUR_COLOR;
}
```

### Zmiana Shadows

```css
:root {
    --shadow-lg: 0 20px 60px rgba(0, 0, 0, 0.2);
}
```

### Zmiana Border Radius

```css
:root {
    --radius-lg: 20px;
}
```

### Zmiana Transitions

```css
:root {
    --transition-base: all 0.5s ease;
}
```

## ? Przyk�adowe Layouts

### Dashboard Card Grid
```html
<div class="row g-4">
    <div class="col-md-6 col-lg-3">
        <div class="card-modern hover-lift">
         <div class="card-body-modern text-center">
         <i class="bi bi-people display-4 text-primary mb-3"></i>
        <h3 class="fw-bold">1,234</h3>
              <p class="text-muted mb-0">Total Users</p>
  </div>
        </div>
    </div>
    <!-- Wi�cej kart... -->
</div>
```

### Form with Modern Inputs
```html
<form>
    <div class="mb-3">
      <label class="form-label fw-semibold">Email</label>
      <div class="input-group-modern">
            <span class="input-group-text">
    <i class="bi bi-envelope"></i>
   </span>
            <input type="email" class="form-control" />
        </div>
    </div>
    
    <div class="mb-3">
        <label class="form-label fw-semibold">Password</label>
        <div class="input-group-modern">
      <span class="input-group-text">
   <i class="bi bi-lock"></i>
            </span>
            <input type="password" class="form-control" />
    </div>
  </div>
    
    <button class="btn-modern btn-gradient-primary w-100">
        <i class="bi bi-check-circle me-2"></i>Submit
    </button>
</form>
```

## ?? Responsive Behavior

### Mobile (< 768px)
- Przyciski: pe�na szeroko��
- Cards: mniejsze zaokr�glenia
- Modals: dostosowane rozmiary

### Tablet & Desktop
- Normalne rozmiary i spacing
- Pe�ne efekty hover

## ? Accessibility

- Focus-visible outlines
- Prefers-reduced-motion support
- Semantic HTML
- ARIA labels
- Keyboard navigation

## ?? Best Practices

1. **Konsystencja** - U�ywaj tych samych klas w ca�ej aplikacji
2. **Semantic HTML** - U�ywaj odpowiednich tag�w
3. **Accessibility** - Zawsze dodawaj ARIA labels
4. **Performance** - U�ywaj CSS variables dla customizacji
5. **Mobile First** - Projektuj najpierw dla mobile

## ?? CSS Variables Reference

```css
/* Gradients */
--gradient-primary-start: #667eea
--gradient-primary-end: #764ba2
--gradient-success-start: #11998e
--gradient-success-end: #38ef7d

/* Shadows */
--shadow-sm: 0 2px 8px rgba(0, 0, 0, 0.08)
--shadow-md: 0 4px 16px rgba(0, 0, 0, 0.12)
--shadow-lg: 0 10px 40px rgba(0, 0, 0, 0.15)
--shadow-xl: 0 15px 50px rgba(0, 0, 0, 0.2)

/* Border Radius */
--radius-sm: 8px
--radius-md: 12px
--radius-lg: 16px
--radius-xl: 20px
--radius-xxl: 24px

/* Transitions */
--transition-fast: all 0.2s cubic-bezier(0.4, 0, 0.2, 1)
--transition-base: all 0.3s cubic-bezier(0.4, 0, 0.2, 1)
--transition-slow: all 0.5s cubic-bezier(0.4, 0, 0.2, 1)
```

## ?? Migration Guide

### Z Bootstrap do Modern Design System

#### Przed:
```html
<div class="card">
    <div class="card-header bg-primary text-white">
   Header
    </div>
    <div class="card-body">
      Content
    </div>
</div>
```

#### Po:
```html
<div class="card-modern">
    <div class="card-header-modern">
    Header
    </div>
    <div class="card-body-modern">
     Content
    </div>
</div>
```

## ?? Resources

- **Bootstrap Icons**: https://icons.getbootstrap.com/
- **CSS Variables**: https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties
- **Cubic Bezier**: https://cubic-bezier.com/

## ?? Przyk�ady Implementacji

Zobacz przyk�adowe implementacje w:
- `/Account/Profile` - Pe�na strona profilu
- `/Home/Index` - Strona g��wna
- `/Admin/Dashboard` - Panel administracyjny

## ?? Tips & Tricks

1. ��cz klasy dla lepszych efekt�w:
   ```html
   <div class="card-modern hover-lift glow-effect">
   ```

2. U�ywaj animacji dla lepszego UX:
   ```html
   <div class="fade-in slide-in-up">
   ```

3. Customizuj przez CSS variables:
   ```css
   .my-custom-card {
       --gradient-primary-start: #YOUR_COLOR;
   }
   ```

## ?? Troubleshooting

### Problem: Gradients nie dzia�aj�
**Rozwi�zanie**: Sprawd� czy `modern-design-system.css` jest za�adowany przed `site.css`

### Problem: Animacje s� za szybkie/wolne
**Rozwi�zanie**: Zmie� CSS variable `--transition-base`

### Problem: Dark mode kolory s� �le
**Rozwi�zanie**: Zdefiniuj override w sekcji `[data-bs-theme="dark"]`
