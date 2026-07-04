import { Component, Input, OnChanges, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { CustomerDetail, InternalDoctor, InternalDoctorUpsertRequest } from '../customer.models';
import { CustomerService } from '../customer.service';

type ActiveFilter = 'active' | 'inactive';
type InternalDoctorControlName = 'fullName' | 'phone' | 'whatsApp' | 'email' | 'notes';

@Component({
  selector: 'app-internal-doctors-section',
  imports: [FormsModule, ReactiveFormsModule],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h2>Doctores internos</h2>
          <p>Contactos clinicos asociados a esta clinica.</p>
        </div>
        @if (canCreate && !showForm()) {
          <button class="primary-button" type="button" (click)="startCreate()">Nuevo doctor interno</button>
        }
      </header>

      <div class="toolbar">
        <label class="filter-field">
          <span>Estado</span>
          <select name="doctorStatus" [ngModel]="activeFilter()" (ngModelChange)="setActiveFilter($event)">
            <option value="active">Activos</option>
            <option value="inactive">Inactivos</option>
          </select>
        </label>
      </div>

      @if (!showForm() && errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (showForm()) {
        <form class="feature-page" [formGroup]="form" (ngSubmit)="submit()">
          <div class="field-grid">
            <label class="form-field">
              <span>Nombre completo</span>
              <input type="text" formControlName="fullName" maxlength="150" />
              @if (hasError('fullName', 'required')) {
                <small class="validation-error">El nombre completo es obligatorio.</small>
              }
            </label>
            <label class="form-field">
              <span>Telefono</span>
              <input type="tel" formControlName="phone" maxlength="30" />
            </label>
            <label class="form-field">
              <span>WhatsApp</span>
              <input type="tel" formControlName="whatsApp" maxlength="30" />
            </label>
            <label class="form-field">
              <span>Email</span>
              <input type="email" formControlName="email" maxlength="200" />
              @if (hasError('email', 'email')) {
                <small class="validation-error">Captura un email valido.</small>
              }
            </label>
            <label class="form-field full-field">
              <span>Notas</span>
              <textarea formControlName="notes" maxlength="1000"></textarea>
            </label>
          </div>

          @if (errorMessage(); as message) {
            <p class="alert-error" role="alert">{{ message }}</p>
          }

          <div class="page-actions">
            <button class="primary-button" type="submit" [disabled]="isSaving()">
              {{ isSaving() ? 'Guardando...' : editingDoctor() ? 'Guardar doctor' : 'Crear doctor' }}
            </button>
            <button class="ghost-button" type="button" (click)="cancelForm()">Cancelar</button>
          </div>
        </form>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando doctores internos...</p>
      } @else if (doctors().length === 0) {
        <p class="empty-state">No hay doctores internos con el filtro actual.</p>
      } @else {
        <table class="data-table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Contacto</th>
              <th>Email</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            @for (doctor of doctors(); track doctor.id) {
              <tr>
                <td>{{ doctor.fullName }}</td>
                <td>{{ doctor.phone || doctor.whatsApp || '-' }}</td>
                <td>{{ doctor.email || '-' }}</td>
                <td>
                  <span class="status-pill" [class.active]="doctor.isActive" [class.inactive]="!doctor.isActive">
                    {{ doctor.isActive ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td>
                  @if (canEdit) {
                    <div class="page-actions">
                      <button class="secondary-button" type="button" (click)="startEdit(doctor)">Editar</button>
                      <button
                        type="button"
                        [class.danger-button]="doctor.isActive"
                        [class.secondary-button]="!doctor.isActive"
                        (click)="toggleStatus(doctor)"
                      >
                        {{ doctor.isActive ? 'Desactivar' : 'Activar' }}
                      </button>
                    </div>
                  } @else {
                    <span>-</span>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
  `
})
export class InternalDoctorsSectionComponent implements OnChanges {
  @Input() customer: CustomerDetail | null = null;

  readonly doctors = signal<InternalDoctor[]>([]);
  readonly activeFilter = signal<ActiveFilter>('active');
  readonly showForm = signal(false);
  readonly editingDoctor = signal<InternalDoctor | null>(null);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = new FormGroup({
    fullName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(150)]
    }),
    phone: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(30)]
    }),
    whatsApp: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(30)]
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.email, Validators.maxLength(200)]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)]
    })
  });

  constructor(
    private readonly customerService: CustomerService,
    private readonly authService: AuthService
  ) {}

  get canCreate(): boolean {
    return this.authService.hasPermission('customers.create');
  }

  get canEdit(): boolean {
    return this.authService.hasPermission('customers.edit');
  }

  ngOnChanges(): void {
    this.load();
  }

  setActiveFilter(activeFilter: ActiveFilter): void {
    this.activeFilter.set(activeFilter);
    this.load();
  }

  load(): void {
    if (!this.customer || this.customer.type !== 'Clinic') {
      this.doctors.set([]);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.customerService
      .listInternalDoctors(this.customer.id, { isActive: this.activeFilter() === 'active' })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (doctors) => {
          this.doctors.set(doctors);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toErrorMessage(error));
          this.doctors.set([]);
        }
      });
  }

  startCreate(): void {
    this.editingDoctor.set(null);
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.form.reset({
      fullName: '',
      phone: '',
      whatsApp: '',
      email: '',
      notes: ''
    });
  }

  startEdit(doctor: InternalDoctor): void {
    this.editingDoctor.set(doctor);
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.form.reset({
      fullName: doctor.fullName,
      phone: doctor.phone ?? '',
      whatsApp: doctor.whatsApp ?? '',
      email: doctor.email ?? '',
      notes: doctor.notes ?? ''
    });
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingDoctor.set(null);
    this.errorMessage.set(null);
  }

  submit(): void {
    const currentCustomer = this.customer;
    const currentDoctor = this.editingDoctor();

    if (!currentCustomer) {
      return;
    }

    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const request = this.toRequest();
    const save$ = currentDoctor
      ? this.customerService.updateInternalDoctor(currentCustomer.id, currentDoctor.id, request)
      : this.customerService.createInternalDoctor(currentCustomer.id, request);

    save$.pipe(finalize(() => this.isSaving.set(false))).subscribe({
      next: () => {
        this.cancelForm();
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.toErrorMessage(error));
      }
    });
  }

  toggleStatus(doctor: InternalDoctor): void {
    const currentCustomer = this.customer;

    if (!currentCustomer) {
      return;
    }

    if (doctor.isActive && !window.confirm(`Desactivar a ${doctor.fullName}?`)) {
      return;
    }

    this.customerService.updateInternalDoctorStatus(currentCustomer.id, doctor.id, !doctor.isActive).subscribe({
      next: () => this.load(),
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.toErrorMessage(error));
      }
    });
  }

  hasError(controlName: InternalDoctorControlName, errorName: string): boolean {
    const control = this.form.controls[controlName];

    return control.touched && control.hasError(errorName);
  }

  private toRequest(): InternalDoctorUpsertRequest {
    const value = this.form.getRawValue();

    return {
      fullName: value.fullName.trim(),
      phone: this.normalizeOptional(value.phone),
      whatsApp: this.normalizeOptional(value.whatsApp),
      email: this.normalizeOptional(value.email),
      notes: this.normalizeOptional(value.notes)
    };
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();

    return trimmed.length > 0 ? trimmed : null;
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'Revisa los campos capturados.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para modificar doctores internos.';
    }

    return 'No fue posible actualizar doctores internos.';
  }
}
