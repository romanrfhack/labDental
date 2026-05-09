import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import {
  WorkOrderChangeStatusRequest,
  WorkOrderStatus,
  WorkOrderStatusOption
} from '../work-order.models';

@Component({
  selector: 'app-work-order-status-change',
  imports: [ReactiveFormsModule],
  template: `
    <form class="status-change" [formGroup]="form" (ngSubmit)="submit()">
      <label class="form-field">
        <span>Estado</span>
        <select formControlName="status">
          @for (status of statuses; track status.value) {
            <option [value]="status.value">{{ status.label }}</option>
          }
        </select>
      </label>

      <label class="form-field">
        <span>Notas</span>
        <textarea formControlName="notes" maxlength="1000"></textarea>
        @if (requiresNotes && form.controls.notes.touched && form.controls.notes.hasError('required')) {
          <small class="validation-error">La nota es obligatoria al cancelar.</small>
        }
      </label>

      @if (errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      }

      <button class="secondary-button" type="submit" [disabled]="isSubmitting || isCancelled">
        {{ isSubmitting ? 'Actualizando...' : 'Cambiar estado' }}
      </button>
    </form>
  `
})
export class WorkOrderStatusChangeComponent implements OnChanges, OnInit {
  @Input({ required: true }) currentStatus!: WorkOrderStatus;
  @Input() statuses: WorkOrderStatusOption[] = [];
  @Input() isSubmitting = false;
  @Input() isCancelled = false;
  @Input() errorMessage = '';
  @Output() readonly changeStatus = new EventEmitter<WorkOrderChangeStatusRequest>();

  readonly form = new FormGroup({
    status: new FormControl<WorkOrderStatus>('Received', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)]
    })
  });

  get requiresNotes(): boolean {
    return this.form.controls.status.value === 'Cancelled';
  }

  ngOnInit(): void {
    this.form.controls.status.valueChanges.subscribe(() => this.syncNotesValidator());
  }

  ngOnChanges(): void {
    if (!this.currentStatus) {
      return;
    }

    this.form.controls.status.setValue(this.currentStatus, { emitEvent: false });
    this.syncNotesValidator();
  }

  submit(): void {
    this.syncNotesValidator();
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSubmitting || this.isCancelled) {
      return;
    }

    const value = this.form.getRawValue();
    const notes = this.normalizeOptional(value.notes);

    if (value.status === 'Cancelled' && !window.confirm('Cancelar esta orden de trabajo?')) {
      return;
    }

    this.changeStatus.emit({
      status: value.status,
      notes
    });
  }

  private syncNotesValidator(): void {
    const validators = [Validators.maxLength(1000)];

    if (this.requiresNotes) {
      validators.push(Validators.required);
    }

    this.form.controls.notes.setValidators(validators);
    this.form.controls.notes.updateValueAndValidity({ emitEvent: false });
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();

    return trimmed.length > 0 ? trimmed : null;
  }
}
