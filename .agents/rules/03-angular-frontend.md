---
trigger: glob
globs: "employee-management-ui/**, **/*.ts, **/*.html, **/*.scss"
description: Standards for Angular 11 frontend development, TypeScript, SCSS, and responsive UI.
---

# Angular Frontend Engineering Standards

## 1. Version & Framework Compatibility
- **Installed Stack**: Angular 11.0.9, TypeScript 4.0.2, RxJS 6.6.0, Bootstrap 5.3.
- **Strict Compatibility Rule**:
  - **No Standalone Components**: Do NOT use `standalone: true`, `imports: []` inside `@Component()`, or `loadComponent`.
  - **No `inject()` in Functions/Templates**: Always inject dependencies through the constructor.
  - **No Angular Signals**: Signals (`signal()`, `computed()`, `effect()`) are unsupported in Angular 11.
  - All new components, directives, and pipes must be explicitly registered in `declarations` of `AppModule` or an appropriate feature module.

## 2. Component & Architecture Architecture
- **Feature-Based Organization**:
  - `components/`: UI views and feature pages (`dashboard/`, `employee-list/`, `departments/`, `designations/`, etc.).
  - `services/`: API communication, state management, and business logic.
  - `models/`: Strongly typed TypeScript interfaces and enums.
  - `guards/`: Route protection (`AuthGuard`, `RoleGuard`).
  - `interceptors/`: HTTP interceptors (`JwtInterceptor`).
- **Thin Components**: Keep presentation components lightweight. Delegate all HTTP calls, calculations, and state operations to injectable services.
- **Strict Typing**:
  - Prohibit `any` unless strictly unavoidable for third-party interop.
  - Define interfaces for all API payloads and view models.
  - Always type parameters in callbacks, subscriptions, and event handlers.

## 3. RxJS & Subscription Management
- **Memory Leak Prevention**:
  - Always clean up subscriptions upon component destruction. Use `takeUntil(this.destroy$)`, `Subscription.unsubscribe()`, or the `async` pipe in templates.
- **RxJS Operators**:
  - Avoid nested subscriptions. Use flattening operators (`switchMap`, `mergeMap`, `concatMap`).
  - Use `debounceTime` and `distinctUntilChanged` for search input streams.
- **Centralized HTTP Services**:
  - Never make direct `HttpClient` calls inside components; encapsulate them in dedicated services (`EmployeeService`, `OrgService`, `AuthService`).

## 4. Forms & Validation
- **Reactive Forms**: Use `FormBuilder` and `FormGroup` for non-trivial forms.
- **Comprehensive Validation**:
  - Always pair client-side validators (`Validators.required`, `Validators.email`, `Validators.pattern`) with readable error feedback in templates.
  - Validate touch state (`formControl.touched && formControl.invalid`) before displaying error messages.
  - Mark all fields as touched (`form.markAllAsTouched()`) on invalid submission attempts.

## 5. UI/UX & Responsive Design
- **Consistent Styling**: Use SCSS with Bootstrap 5 utility classes and project CSS variables defined in `styles.scss`.
- **Complete Visual States**: Every view must handle:
  - **Loading State**: Spinner or skeleton loader while fetching data.
  - **Empty State**: Friendly illustration/message when a list or query returns 0 records.
  - **Error State**: Non-blocking alert banner or toast describing API failures.
  - **Success State**: Toast or banner acknowledging successful mutations.
- **Destructive Action Confirmation**: Always require user confirmation before executing deletions, deactivations, or account unlinking.
- **Role-Based Visibility**: Use `*ngIf="authService.hasAnyRole([...])"` to hide administrative controls from unauthorized roles, while recognizing that backend RBAC remains the authoritative boundary.
