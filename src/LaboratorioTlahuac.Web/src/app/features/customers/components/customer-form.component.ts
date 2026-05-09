import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { CustomerDetail, CustomerType, CustomerUpsertRequest } from '../customer.models';

type CustomerControlName =
  | 'type'
  | 'displayName'
  | 'legalName'
  | 'contactName'
  | 'phone'
  | 'whatsApp'
  | 'email'
  | 'address'
  | 'notes';

@Component({
  selector: 'app-customer-form',
  imports: [ReactiveFormsModule],
  template: `
    <form class="feature-page" [formGroup]="form" (ngSubmit)="submit()">
      <div class="field-grid">
        <label class="form-field">
          <span>Tipo</span>
          <select formControlName="type">
            <option value="Doctor">Doctor</option>
            <option value="Clinic">Clinica</option>
            <option value="Other">Otro</option>
          </select>
        </label>

        <label class="form-field">
          <span>Nombre visible</span>
          <input type="text" formControlName="displayName" maxlength="150" />
          @if (hasError('displayName', 'required')) {
            <small class="validation-error">El nombre visible es obligatorio.</small>
          }
          @if (hasError('displayName', 'maxlength')) {
            <small class="validation-error">Maximo 150 caracteres.</small>
          }
        </label>

        <label class="form-field">
          <span>Razon social</span>
          <input type="text" formControlName="legalName" maxlength="200" />
        </label>

        <label class="form-field">
          <span>Contacto</span>
          <input type="text" formControlName="contactName" maxlength="150" />
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
          <span>Direccion</span>
          <textarea formControlName="address" maxlength="500"></textarea>
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
        <button class="primary-button" type="submit" [disabled]="isSubmitting">
          {{ isSubmitting ? 'Guardando...' : submitLabel }}
        </button>
        <button class="ghost-button" type="button" (click)="cancel.emit()">Cancelar</button>
      </div>
    </form>
  `
})
export class CustomerFormComponent implements OnChanges {
  @Input() customer: CustomerDetail | null = null;
  @Input() submitLabel = 'Guardar';
  @Input() isSubmitting = false;
  @Input() errorMessage = '';
  @Output() readonly save = new EventEmitter<CustomerUpsertRequest>();
  @Output() readonly cancel = new EventEmitter<void>();

  readonly form = new FormGroup({
    type: new FormControl<CustomerType>('Doctor', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    displayName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(150)]
    }),
    legalName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(200)]
    }),
    contactName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(150)]
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
    address: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)]
    })
  });

  ngOnChanges(): void {
    if (!this.customer) {
      return;
    }

    this.form.reset({
      type: this.customer.type,
      displayName: this.customer.displayName,
      legalName: this.customer.legalName ?? '',
      contactName: this.customer.contactName ?? '',
      phone: this.customer.phone ?? '',
      whatsApp: this.customer.whatsApp ?? '',
      email: this.customer.email ?? '',
      address: this.customer.address ?? '',
      notes: this.customer.notes ?? ''
    });
  }

  submit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    const value = this.form.getRawValue();

    this.save.emit({
      type: value.type,
      displayName: value.displayName.trim(),
      legalName: this.normalizeOptional(value.legalName),
      contactName: this.normalizeOptional(value.contactName),
      phone: this.normalizeOptional(value.phone),
      whatsApp: this.normalizeOptional(value.whatsApp),
      email: this.normalizeOptional(value.email),
      address: this.normalizeOptional(value.address),
      notes: this.normalizeOptional(value.notes)
    });
  }

  hasError(controlName: CustomerControlName, errorName: string): boolean {
    const control = this.form.controls[controlName];

    return control.touched && control.hasError(errorName);
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();

    return trimmed.length > 0 ? trimmed : null;
  }
}
