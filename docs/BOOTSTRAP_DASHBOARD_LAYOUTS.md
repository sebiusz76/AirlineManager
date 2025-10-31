# Bootstrap Dashboard Layouts - Dokumentacja

## Data: 2024
## Status: ? Ukoñczono

---

## ?? Podsumowanie

Projekt zosta³ wzbogacony o dedykowane layouty w stylu panelu administracyjnego dla stref **Admin** i **User**, wzorowane na Bootstrap Dashboard (https://getbootstrap.com/docs/5.3/examples/dashboard/).

---

## ?? Utworzone Pliki

### 1. **Admin Layout**

#### `Areas/Admin/Views/Shared/_Layout.cshtml`
**Charakterystyka:**
- Sticky top navbar z ciemnym t³em (`bg-dark`)
- Fixed sidebar z nawigacj¹ po lewej stronie
- Sekcja SuperAdmin (tylko dla u¿ytkowników z rol¹ SuperAdmin)
- Maintenance mode alert na górze strony
- User dropdown w prawym górnym rogu
- PageActions section (opcjonalna sekcja dla akcji na stronie)
- 2-kolumnowy layout (sidebar + content)

**Funkcje:**
- Dashboard navigation
- Users management
- Application Logs
- Configuration (SuperAdmin)
- Data Retention (SuperAdmin)
- Maintenance Mode (SuperAdmin)
- Link do User Dashboard
- Link powrotny do g³ównej strony

#### `wwwroot/css/areas/Admin/_Layout.css`
**Rozmiar:** 3.5 KB  
**Zawiera:**
- Fixed sidebar positioning
- Active link highlighting (border-left indicator)
- Smooth animations (slideInLeft)
- Responsive breakpoints
- Dark mode support
- Custom scrollbar styling
- Hover effects

---

### 2. **User Layout**

#### `Areas/User/Views/Shared/_Layout.cshtml`
**Charakterystyka:**
- Sticky top navbar z primary gradient
- Fixed sidebar z nawigacj¹ u¿ytkownika
- Sekcja Account z linkami do profilu
- Conditional Admin section (jeœli u¿ytkownik ma rolê Admin/SuperAdmin)
- User dropdown w prawym górnym rogu
- PageActions section (opcjonalna)
- 2-kolumnowy layout (sidebar + content)

**Funkcje:**
- Dashboard
- My Flights (placeholder)
- Bookings (placeholder)
- Payment Methods (placeholder)
- Loyalty Program (placeholder)
- Profile Settings
- Active Sessions
- Login History
- Admin Panel link (conditional)
- Link powrotny do g³ównej strony

#### `wwwroot/css/areas/User/_Layout.css`
**Rozmiar:** 4 KB  
**Zawiera:**
- Fixed sidebar positioning
- Primary gradient navbar
- Dashboard stat cards styling
- Card hover effects
- Responsive design
- Dark mode support
- Custom scrollbar
- Animations

---

## ?? Struktura Layoutów

### **Bootstrap Dashboard Pattern**

```
???????????????????????????????????????????
?  Header (Sticky Top)     ?
?  [Logo] [User Menu]         ?
???????????????????????????????????????????
???????????????????????????????????????????
?            ?     ?
?  Sidebar   ?  Main Content        ?
?  (Fixed)   ?  ????????????????????????  ?
?     ?  ?  Page Title     ?  ?
?  • Nav 1   ?  ?  [Actions]      ?  ?
?  • Nav 2   ?  ????????????????????????  ?
?  • Nav 3   ?  ?          ?  ?
?            ?  ?  Content Area        ?  ?
?  ??????    ??          ?  ?
?  Section   ?  ?       ?  ?
?  • Nav 4   ?  ?   ?  ?
?     ?  ????????????????????????  ?
???????????????????????????????????????????
```

---

## ?? Kluczowe Funkcje

### **1. Responsive Design**

| Breakpoint | Sidebar | Content | Navbar |
|------------|---------|---------|--------|
| **Desktop (>768px)** | Fixed, visible | 9-10 columns | Sticky top |
| **Tablet (768px)** | Fixed, 3 columns | 9 columns | Sticky top |
| **Mobile (<768px)** | Collapsible | Full width | Hamburger menu |

### **2. Active Link Detection**
```razor
class="@(ViewContext.RouteData.Values["controller"]?.ToString() == "Dashboard" ? "active" : "")"
```
- Automatyczne podœwietlanie aktywnej strony
- Visual indicator (border-left dla aktywnego linku)
- Font-weight 600 dla aktywnego elementu

### **3. PageActions Section**
```razor
@section PageActions {
    <div class="btn-group">
        <button class="btn btn-sm btn-primary">Add New</button>
     <button class="btn btn-sm btn-outline-secondary">Export</button>
    </div>
}
```
- Optional section w ka¿dym widoku
- Wyœwietlana w prawym górnym rogu obok tytu³u
- Idealna dla akcji specyficznych dla strony

### **4. Dark Mode Support**
- Wszystkie warianty motywów (dark, dark-slate, dark-midnight)
- Automatyczne dostosowanie kolorów sidebara
- Invert shadows dla dark mode
- Custom scrollbar theming

### **5. User Dropdown Menu**
**Admin:**
- Profile
- Theme
- Sign out

**User:**
- Profile
- Active Sessions
- Theme
- Sign out

---

## ?? U¿ycie

### **Tworzenie Nowego Widoku w Admin Area**

1. Utwórz plik w `Areas/Admin/Views/[Controller]/[Action].cshtml`
2. Layout zostanie automatycznie zastosowany przez `_ViewStart.cshtml`
3. Opcjonalnie dodaj sekcjê PageActions:

```razor
@{
    ViewData["Title"] = "My Admin Page";
}

@section PageActions {
  <button class="btn btn-sm btn-primary">
        <i class="bi bi-plus"></i> Add New
    </button>
}

<!-- Your content here -->
```

### **Tworzenie Nowego Widoku w User Area**

1. Utwórz plik w `Areas/User/Views/[Controller]/[Action].cshtml`
2. Layout zostanie automatycznie zastosowany przez `_ViewStart.cshtml`
3. Opcjonalnie dodaj sekcjê PageActions

---

## ?? Style CSS

### **Sidebar Styling**

```css
.sidebar {
    position: fixed;
    top: 0;
    bottom: 0;
    left: 0;
    z-index: 100;
    padding: 48px 0 0;
}

.sidebar .nav-link.active {
    color: var(--bs-primary);
    background-color: var(--bs-secondary-bg);
    border-left-color: var(--bs-primary);
    font-weight: 600;
}
```

### **Navbar Gradient (User)**

```css
.bg-primary.navbar {
 background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
}
```

### **Animations**

```css
@keyframes slideInLeft {
 from {
        transform: translateX(-100%);
  opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}
```

---

## ?? Mobile Experience

### **Hamburger Menu**
- Sidebar collapsible na urz¹dzeniach mobilnych
- Button position absolute w navbar
- Touch-friendly navigation
- Smooth collapse/expand animation

### **Responsive Adjustments**
```css
@media (max-width: 767.98px) {
    .sidebar {
        position: static;
        padding: 0;
    }
    
    main {
        padding-top: 0;
    }
}
```

---

## ?? Security Features

### **Role-Based Navigation**

**Admin Layout:**
```razor
@if (User.IsInRole("SuperAdmin"))
{
    <!-- SuperAdmin only links -->
}
```

**User Layout:**
```razor
@if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
{
    <!-- Admin panel link -->
}
```

### **Maintenance Mode Alert**
- Widoczny tylko dla SuperAdmin
- Sticky na górze strony
- Link do zarz¹dzania trybem maintenance

---

## ?? Ró¿nice miêdzy Layoutami

| Feature | Admin Layout | User Layout |
|---------|-------------|-------------|
| **Navbar Color** | Dark (`bg-dark`) | Primary Gradient |
| **Focus** | Administration | User Portal |
| **SuperAdmin Section** | ? Yes | ? No |
| **Maintenance Alert** | ? Yes | ? No |
| **My Flights** | ? No | ? Yes |
| **Loyalty Program** | ? No | ? Yes |
| **System Config** | ? Yes | ? No |
| **Users Management** | ? Yes | ? No |

---

## ?? Przyk³ady U¿ycia

### **Admin Dashboard Card**
```html
<div class="col-xl-3 col-md-6">
    <div class="card">
      <div class="card-body">
     <div class="d-flex justify-content-between">
     <div>
          <h6 class="text-muted">Total Users</h6>
   <h2>1,429</h2>
    <span class="badge bg-success">+12%</span>
                </div>
      <div style="font-size: 3rem; opacity: 0.2;">
        <i class="bi bi-people-fill"></i>
         </div>
         </div>
        </div>
    </div>
</div>
```

### **User Dashboard Stat Card**
```html
<div class="col-xl-3 col-md-6">
    <div class="card dashboard-stat-card">
<div class="card-body">
            <div class="d-flex justify-content-between">
 <div>
        <p class="stat-label">Upcoming Flights</p>
   <h2 class="stat-value">2</h2>
                </div>
                <div class="stat-icon">
   <i class="bi bi-airplane-fill"></i>
       </div>
    </div>
        </div>
    </div>
</div>
```

---

## ?? Integration z Istniej¹cym Projektem

### **Automatyczne Zastosowanie Layoutów**

Layouty s¹ automatycznie stosowane dziêki `_ViewStart.cshtml` w ka¿dym Area:

```razor
@{
    Layout = "_Layout";
}
```

### **Zachowana Kompatybilnoœæ**
- ? Wszystkie istniej¹ce widoki dzia³aj¹ bez zmian
- ? Cookie consent banner zachowany
- ? Toast notifications dzia³aj¹
- ? Theme system zintegrowany
- ? SweetAlert2 dostêpny

---

## ?? Korzyœci

### **1. Professional UI/UX**
- Industry-standard dashboard interface
- Familiar navigation pattern
- Clean, modern design

### **2. Separation of Concerns**
- Admin ma dedykowany layout i style
- User ma dedykowany layout i style
- Clear visual distinction

### **3. Maintainability**
- £atwe dodawanie nowych stron
- Consistent design w obrêbie area
- Dedicated CSS files

### **4. Responsive**
- Mobile-first approach
- Touch-friendly
- Hamburger menu na mobile

### **5. Extensible**
- £atwo dodaæ nowe sekcje
- PageActions section dla custom buttons
- Template dla przysz³ych areas

---

## ? Status

- ? **Build successful** - zero b³êdów kompilacji
- ? **Admin Layout** - w pe³ni funkcjonalny
- ? **User Layout** - w pe³ni funkcjonalny
- ? **CSS Styling** - kompletne z animations
- ? **Responsive Design** - mobile/tablet/desktop
- ? **Dark Mode** - wszystkie warianty wspierane
- ? **Role-Based Navigation** - security implemented
- ? **Cookie Consent** - zachowany w obu layoutach

---

## ?? Future Enhancements

1. ? Collapsible sidebar sections
2. ? Breadcrumbs navigation
3. ? Search in sidebar
4. ? Notifications center
5. ? Quick settings panel
6. ? Chart.js integration
7. ? Real-time updates via SignalR
8. ? User preferences (remember sidebar state)

---

**Autor**: GitHub Copilot  
**Data utworzenia**: 2024  
**Wersja**: 1.0
