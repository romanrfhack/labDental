import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';

import { WorkOrderPayment } from '../payment.models';

@Component({
  selector: 'app-payment-cancel-action',
  imports: [ReactiveFormsModule],
  template: `
    @if (!isOpen) {
      <button class="danger-button" type="button" [disabled]="isSubmitting" (click)="open()">
        Cancelar
      </button>
    } @else {
      <div class="cancel-action">
        <label class="form-field">
          <span>Motivo de cancelacion</span>
          <textarea maxlength="1000" [formControl]="reason"></textarea>
          @if (reason.touched && reason.hasError('required')) {
            <small class="validation-error">El motivo es obligatorio.</small>
          }
          @if (reason.touched && reason.hasError('maxlength')) {
            <small class="validation-error">Maximo 1000 caracteres.</small>
          }
        </label>
        <div class="page-actions">
          <button class="danger-button" type="button" [disabled]="isSubmitting" (click)="confirm()">
            {{ isSubmitting ? 'Cancelando...' : 'Confirmar' }}
          </button>
          <button class="ghost-button" type="button" [disabled]="isSubmitting" (click)="close()">
            Cerrar
          </button>
        </div>
      </div>
    }
  `
})
export class PaymentCancelActionComponent {
  @Input({ required: true }) payment!: WorkOrderPayment;
  @Input() isSubmitting = false;
  @Output() readonly cancelPayment = new EventEmitter<string>();

  isOpen = false;

  readonly reason = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(1000)]
  });

  open(): void {
    this.isOpen = true;
  }

  close(): void {
    this.isOpen = false;
    this.reason.reset('');
  }

  confirm(): void {
    this.reason.markAsTouched();

    if (this.reason.invalid || this.isSubmitting) {
      return;
    }

    this.cancelPayment.emit(this.reason.value.trim());
  }
}
