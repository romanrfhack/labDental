import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import {
  PaymentCreateRequest,
  PaymentMethod,
  PaymentMethodOption
} from '../payment.models';

type PaymentCreateControlName = 'paymentDate' | 'amount' | 'method' | 'reference' | 'notes';

@Component({
  selector: 'app-payment-create-form',
  imports: [ReactiveFormsModule],
  template: `
    <form class="payment-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="field-grid">
        <label class="form-field">
          <span>Fecha de pago</span>
          <input type="date" formControlName="paymentDate" />
          @if (hasError('paymentDate', 'required')) {
            <small class="validation-error">La fecha de pago es obligatoria.</small>
          }
        </label>

        <label class="form-field">
          <span>Monto</span>
          <input type="number" min="0.01" step="0.01" formControlName="amount" />
          @if (hasError('amount', 'required')) {
            <small class="validation-error">El monto es obligatorio.</small>
          }
          @if (hasError('amount', 'min')) {
            <small class="validation-error">El monto debe ser mayor a 0.</small>
          }
        </label>

        <label class="form-field">
          <span>Metodo</span>
          <select formControlName="method">
            <option value="">Selecciona un metodo</option>
            @for (method of methods; track method.value) {
              <option [value]="method.value">{{ method.label }}</option>
            }
          </select>
          @if (hasError('method', 'required')) {
            <small class="validation-error">El metodo es obligatorio.</small>
          }
        </label>

        <label class="form-field">
          <span>Referencia</span>
          <input type="text" maxlength="100" formControlName="reference" />
          @if (hasError('reference', 'maxlength')) {
            <small class="validation-error">Maximo 100 caracteres.</small>
          }
        </label>

        <label class="form-field full-field">
          <span>Observaciones</span>
          <textarea maxlength="1000" formControlName="notes"></textarea>
          @if (hasError('notes', 'maxlength')) {
            <small class="validation-error">Maximo 1000 caracteres.</small>
          }
        </label>
      </div>

      @if (errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      }

      <div class="page-actions">
        <button class="primary-button" type="submit" [disabled]="isSubmitting">
          {{ isSubmitting ? 'Registrando...' : 'Registrar pago' }}
        </button>
      </div>
    </form>
  `
})
export class PaymentCreateFormComponent implements OnChanges {
  @Input() methods: PaymentMethodOption[] = [];
  @Input() isSubmitting = false;
  @Input() errorMessage = '';
  @Input() resetSignal = 0;
  @Output() readonly create = new EventEmitter<PaymentCreateRequest>();

  readonly form = new FormGroup({
    paymentDate: new FormControl(this.today(), {
      nonNullable: true,
      validators: [Validators.required]
    }),
    amount: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(0.01)]
    }),
    method: new FormControl<PaymentMethod | ''>('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    reference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)]
    })
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['methods']) {
      this.ensureDefaultMethod();
    }

    if (changes['resetSignal'] && !changes['resetSignal'].firstChange) {
      this.resetForm();
    }
  }

  submit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    const value = this.form.getRawValue();

    if (!value.amount || !value.method) {
      return;
    }

    this.create.emit({
      paymentDate: value.paymentDate,
      amount: value.amount,
      method: value.method,
      reference: this.normalizeOptional(value.reference),
      notes: this.normalizeOptional(value.notes)
    });
  }

  hasError(controlName: PaymentCreateControlName, errorName: string): boolean {
    const control = this.form.controls[controlName];

    return control.touched && control.hasError(errorName);
  }

  private resetForm(): void {
    this.form.reset({
      paymentDate: this.today(),
      amount: null,
      method: this.methods[0]?.value ?? '',
      reference: '',
      notes: ''
    });
  }

  private ensureDefaultMethod(): void {
    if (!this.form.controls.method.value && this.methods.length > 0) {
      this.form.controls.method.setValue(this.methods[0].value);
    }
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();

    return trimmed.length > 0 ? trimmed : null;
  }

  private today(): string {
    const now = new Date();
    const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60000);

    return localDate.toISOString().slice(0, 10);
  }
}
