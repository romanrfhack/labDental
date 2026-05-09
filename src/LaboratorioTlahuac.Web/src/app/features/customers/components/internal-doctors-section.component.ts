import { Component, Input, OnChanges } from '@angular/core';
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
        @if (canCreate && !showForm) {
          <button class="primary-button" type="button" (click)="startCreate()">Nuevo doctor interno</button>
        }
      </header>

      <div class="toolbar">
        <label class="filter-field">
          <span>Estado</span>
          <select name="doctorStatus" [(ngModel)]="activeFilter" (ngModelChange)="load()">
            <option value="active">Activos</option>
            <option value="inactive">Inactivos</option>
          </select>
        </label>
      </div>

      @if (showForm) {
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

          @if (errorMessage) {
            <p class="alert-error" role="alert">{{ errorMessage }}</p>
          }

          <div class="page-actions">
            <button class="primary-button" type="submit" [disabled]="isSaving">
              {{ isSaving ? 'Guardando...' : editingDoctor ? 'Guardar doctor' : 'Crear doctor' }}
            </button>
            <button class="ghost-button" type="button" (click)="cancelForm()">Cancelar</button>
          </div>
        </form>
      }

      @if (isLoading) {
        <p class="loading-state">Cargando doctores internos...</p>
      } @else if (doctors.length === 0) {
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
            @for (doctor of doctors; track doctor.id) {
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

  doctors: InternalDoctor[] = [];
  activeFilter: ActiveFilter = 'active';
  showForm = false;
  editingDoctor: InternalDoctor | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';

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

  load(): void {
    if (!this.customer || this.customer.type !== 'Clinic') {
      this.doctors = [];
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.customerService
      .listInternalDoctors(this.customer.id, { isActive: this.activeFilter === 'active' })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (doctors) => {
          this.doctors = doctors;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.toErrorMessage(error);
        }
      });
  }

  startCreate(): void {
    this.editingDoctor = null;
    this.showForm = true;
    this.errorMessage = '';
    this.form.reset({
      fullName: '',
      phone: '',
      whatsApp: '',
      email: '',
      notes: ''
    });
  }

  startEdit(doctor: InternalDoctor): void {
    this.editingDoctor = doctor;
    this.showForm = true;
    this.errorMessage = '';
    this.form.reset({
      fullName: doctor.fullName,
      phone: doctor.phone ?? '',
      whatsApp: doctor.whatsApp ?? '',
      email: doctor.email ?? '',
      notes: doctor.notes ?? ''
    });
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingDoctor = null;
    this.errorMessage = '';
  }

  submit(): void {
    if (!this.customer) {
      return;
    }

    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSaving) {
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const request = this.toRequest();
    const save$ = this.editingDoctor
      ? this.customerService.updateInternalDoctor(this.customer.id, this.editingDoctor.id, request)
      : this.customerService.createInternalDoctor(this.customer.id, request);

    save$.pipe(finalize(() => (this.isSaving = false))).subscribe({
      next: () => {
        this.cancelForm();
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.toErrorMessage(error);
      }
    });
  }

  toggleStatus(doctor: InternalDoctor): void {
    if (!this.customer) {
      return;
    }

    if (doctor.isActive && !window.confirm(`Desactivar a ${doctor.fullName}?`)) {
      return;
    }

    this.customerService.updateInternalDoctorStatus(this.customer.id, doctor.id, !doctor.isActive).subscribe({
      next: () => this.load(),
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.toErrorMessage(error);
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
