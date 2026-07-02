import { Component, EventEmitter, Input, OnChanges, OnInit, Output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  CustomerListItem,
  InternalDoctor
} from '../../customers/customer.models';
import { CustomerService } from '../../customers/customer.service';
import { WorkOrderDetail, WorkOrderUpsertRequest } from '../work-order.models';

type WorkOrderControlName =
  | 'customerId'
  | 'internalDoctorId'
  | 'patientName'
  | 'receivedDate'
  | 'referenceNumber'
  | 'workDescription'
  | 'dentalColor'
  | 'firstTrialDate'
  | 'secondTrialDate'
  | 'deliveryDate'
  | 'totalAmount'
  | 'notes';

@Component({
  selector: 'app-work-order-form',
  imports: [ReactiveFormsModule],
  template: `
    <form class="feature-page" [formGroup]="form" (ngSubmit)="submit()">
      <div class="field-grid">
        <label class="form-field">
          <span>Cliente</span>
          <select formControlName="customerId">
            <option value="">Selecciona un cliente</option>
            @for (customer of customers(); track customer.id) {
              <option [value]="customer.id">{{ customer.displayName }} - {{ formatType(customer.type) }}</option>
            }
          </select>
          @if (hasError('customerId', 'required')) {
            <small class="validation-error">El cliente es obligatorio.</small>
          }
        </label>

        @if (selectedCustomer()?.type === 'Clinic') {
          <label class="form-field">
            <span>Doctor interno</span>
            <select formControlName="internalDoctorId">
              <option value="">Sin doctor interno</option>
              @for (doctor of internalDoctors(); track doctor.id) {
                <option [value]="doctor.id">{{ doctor.fullName }}</option>
              }
            </select>
          </label>
        }

        <label class="form-field">
          <span>Paciente</span>
          <input type="text" formControlName="patientName" maxlength="150" />
          @if (hasError('patientName', 'required')) {
            <small class="validation-error">El paciente es obligatorio.</small>
          }
          @if (hasError('patientName', 'maxlength')) {
            <small class="validation-error">Maximo 150 caracteres.</small>
          }
        </label>

        <label class="form-field">
          <span>Fecha recepcion</span>
          <input type="date" formControlName="receivedDate" />
          @if (hasError('receivedDate', 'required')) {
            <small class="validation-error">La fecha de recepcion es obligatoria.</small>
          }
        </label>

        <label class="form-field">
          <span>Referencia externa</span>
          <input type="text" formControlName="referenceNumber" maxlength="80" />
        </label>

        <label class="form-field">
          <span>Color</span>
          <input type="text" formControlName="dentalColor" maxlength="50" />
        </label>

        <label class="form-field full-field">
          <span>Trabajo solicitado</span>
          <textarea formControlName="workDescription" maxlength="1000"></textarea>
          @if (hasError('workDescription', 'required')) {
            <small class="validation-error">El trabajo solicitado es obligatorio.</small>
          }
          @if (hasError('workDescription', 'maxlength')) {
            <small class="validation-error">Maximo 1000 caracteres.</small>
          }
        </label>

        <label class="form-field">
          <span>Primera prueba</span>
          <input type="date" formControlName="firstTrialDate" />
        </label>

        <label class="form-field">
          <span>Segunda prueba</span>
          <input type="date" formControlName="secondTrialDate" />
        </label>

        <label class="form-field">
          <span>Fecha entrega</span>
          <input type="date" formControlName="deliveryDate" />
        </label>

        <label class="form-field">
          <span>Costo total</span>
          <input type="number" min="0" step="0.01" formControlName="totalAmount" />
          @if (hasError('totalAmount', 'min')) {
            <small class="validation-error">El costo no puede ser negativo.</small>
          }
        </label>

        <label class="form-field full-field">
          <span>Observaciones</span>
          <textarea formControlName="notes" maxlength="1000"></textarea>
        </label>
      </div>

      @if (isLoadingCustomers()) {
        <p class="loading-state">Cargando clientes...</p>
      }

      @if (isLoadingDoctors()) {
        <p class="loading-state">Cargando doctores internos...</p>
      }

      @if (localErrorMessage() || errorMessage; as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      <div class="page-actions">
        <button class="primary-button" type="submit" [disabled]="isSubmitting || isLoadingCustomers()">
          {{ isSubmitting ? 'Guardando...' : submitLabel }}
        </button>
        <button class="ghost-button" type="button" (click)="cancel.emit()">Cancelar</button>
      </div>
    </form>
  `
})
export class WorkOrderFormComponent implements OnInit, OnChanges {
  @Input() order: WorkOrderDetail | null = null;
  @Input() submitLabel = 'Guardar';
  @Input() isSubmitting = false;
  @Input() errorMessage: string | null = null;
  @Output() readonly save = new EventEmitter<WorkOrderUpsertRequest>();
  @Output() readonly cancel = new EventEmitter<void>();

  readonly customers = signal<CustomerListItem[]>([]);
  readonly internalDoctors = signal<InternalDoctor[]>([]);
  readonly selectedCustomer = signal<CustomerListItem | null>(null);
  readonly isLoadingCustomers = signal(false);
  readonly isLoadingDoctors = signal(false);
  readonly localErrorMessage = signal('');

  readonly form = new FormGroup({
    customerId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    internalDoctorId: new FormControl('', {
      nonNullable: true
    }),
    patientName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(150)]
    }),
    receivedDate: new FormControl(this.today(), {
      nonNullable: true,
      validators: [Validators.required]
    }),
    referenceNumber: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(80)]
    }),
    workDescription: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(1000)]
    }),
    dentalColor: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(50)]
    }),
    firstTrialDate: new FormControl('', {
      nonNullable: true
    }),
    secondTrialDate: new FormControl('', {
      nonNullable: true
    }),
    deliveryDate: new FormControl('', {
      nonNullable: true
    }),
    totalAmount: new FormControl<number | null>(null, {
      validators: [Validators.min(0)]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)]
    })
  });

  constructor(private readonly customerService: CustomerService) {}

  ngOnInit(): void {
    this.loadCustomers();
    this.form.controls.customerId.valueChanges.subscribe((customerId) => {
      this.onCustomerChanged(customerId, true);
    });
  }

  ngOnChanges(): void {
    if (!this.order) {
      return;
    }

    this.ensureOrderCustomerOption();
    this.form.reset(
      {
        customerId: this.order.customerId,
        internalDoctorId: this.order.internalDoctorId ?? '',
        patientName: this.order.patientName,
        receivedDate: this.order.receivedDate,
        referenceNumber: this.order.referenceNumber ?? '',
        workDescription: this.order.workDescription,
        dentalColor: this.order.dentalColor ?? '',
        firstTrialDate: this.order.firstTrialDate ?? '',
        secondTrialDate: this.order.secondTrialDate ?? '',
        deliveryDate: this.order.deliveryDate ?? '',
        totalAmount: this.order.totalAmount,
        notes: this.order.notes ?? ''
      },
      { emitEvent: false });
    this.onCustomerChanged(this.order.customerId, false);
  }

  submit(): void {
    this.localErrorMessage.set('');
    this.form.markAllAsTouched();

    if (!this.validateDates()) {
      return;
    }

    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    const value = this.form.getRawValue();
    const selectedCustomer = this.selectedCustomer();
    const internalDoctorId = selectedCustomer?.type === 'Clinic'
      ? this.normalizeOptional(value.internalDoctorId)
      : null;

    this.save.emit({
      customerId: value.customerId,
      internalDoctorId,
      patientName: value.patientName.trim(),
      receivedDate: value.receivedDate,
      referenceNumber: this.normalizeOptional(value.referenceNumber),
      workDescription: value.workDescription.trim(),
      dentalColor: this.normalizeOptional(value.dentalColor),
      firstTrialDate: this.normalizeOptional(value.firstTrialDate),
      secondTrialDate: this.normalizeOptional(value.secondTrialDate),
      deliveryDate: this.normalizeOptional(value.deliveryDate),
      totalAmount: value.totalAmount,
      notes: this.normalizeOptional(value.notes)
    });
  }

  hasError(controlName: WorkOrderControlName, errorName: string): boolean {
    const control = this.form.controls[controlName];

    return control.touched && control.hasError(errorName);
  }

  formatType(type: CustomerListItem['type']): string {
    if (type === 'Clinic') {
      return 'Clinica';
    }

    if (type === 'Other') {
      return 'Otro';
    }

    return 'Doctor';
  }

  private loadCustomers(): void {
    this.isLoadingCustomers.set(true);
    this.localErrorMessage.set('');

    this.customerService
      .list({ isActive: true, pageSize: 100 })
      .pipe(finalize(() => this.isLoadingCustomers.set(false)))
      .subscribe({
        next: (response) => {
          this.customers.set(response.items);
          this.ensureOrderCustomerOption();
          this.onCustomerChanged(this.form.controls.customerId.value, false);
        },
        error: () => {
          this.customers.set([]);
          this.localErrorMessage.set('No fue posible cargar clientes activos.');
        }
      });
  }

  private onCustomerChanged(customerId: string, clearDoctor: boolean): void {
    const selectedCustomer = this.customers().find((customer) => customer.id === customerId) ?? null;
    this.selectedCustomer.set(selectedCustomer);

    if (clearDoctor) {
      this.form.controls.internalDoctorId.setValue('', { emitEvent: false });
    }

    if (selectedCustomer?.type !== 'Clinic') {
      this.internalDoctors.set([]);
      this.form.controls.internalDoctorId.setValue('', { emitEvent: false });
      return;
    }

    this.loadInternalDoctors(selectedCustomer.id);
  }

  private loadInternalDoctors(customerId: string): void {
    this.isLoadingDoctors.set(true);
    this.localErrorMessage.set('');

    this.customerService
      .listInternalDoctors(customerId, { isActive: true })
      .pipe(finalize(() => this.isLoadingDoctors.set(false)))
      .subscribe({
        next: (doctors) => {
          this.internalDoctors.set(doctors);
          this.ensureOrderInternalDoctorOption();
        },
        error: () => {
          this.internalDoctors.set([]);
          this.localErrorMessage.set('No fue posible cargar doctores internos.');
        }
      });
  }

  private ensureOrderCustomerOption(): void {
    const order = this.order;

    if (!order || this.customers().some((customer) => customer.id === order.customerId)) {
      return;
    }

    this.customers.update((customers) => [
      ...customers,
      {
        id: order.customerId,
        type: order.customerType,
        displayName: order.customerDisplayName,
        contactName: null,
        phone: null,
        whatsApp: null,
        email: null,
        isActive: true
      }
    ].sort((left, right) => left.displayName.localeCompare(right.displayName)));
  }

  private ensureOrderInternalDoctorOption(): void {
    const order = this.order;

    if (!order
        || !order.internalDoctorId
        || !order.internalDoctorFullName
        || this.internalDoctors().some((doctor) => doctor.id === order.internalDoctorId)) {
      return;
    }

    const internalDoctorId = order.internalDoctorId;
    const internalDoctorFullName = order.internalDoctorFullName;

    this.internalDoctors.update((doctors) => [
      ...doctors,
      {
        id: internalDoctorId,
        customerId: order.customerId,
        fullName: internalDoctorFullName,
        phone: null,
        whatsApp: null,
        email: null,
        notes: null,
        isActive: true,
        createdAtUtc: order.createdAtUtc,
        updatedAtUtc: order.updatedAtUtc
      }
    ].sort((left, right) => left.fullName.localeCompare(right.fullName)));
  }

  private validateDates(): boolean {
    const value = this.form.getRawValue();
    const receivedDate = value.receivedDate;

    if (value.firstTrialDate && value.firstTrialDate < receivedDate) {
      this.localErrorMessage.set('La primera prueba no puede ser anterior a la recepcion.');
      return false;
    }

    if (value.secondTrialDate && value.secondTrialDate < receivedDate) {
      this.localErrorMessage.set('La segunda prueba no puede ser anterior a la recepcion.');
      return false;
    }

    if (value.firstTrialDate && value.secondTrialDate && value.secondTrialDate < value.firstTrialDate) {
      this.localErrorMessage.set('La segunda prueba no puede ser anterior a la primera prueba.');
      return false;
    }

    if (value.deliveryDate && value.deliveryDate < receivedDate) {
      this.localErrorMessage.set('La fecha de entrega no puede ser anterior a la recepcion.');
      return false;
    }

    return true;
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();

    return trimmed.length > 0 ? trimmed : null;
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
