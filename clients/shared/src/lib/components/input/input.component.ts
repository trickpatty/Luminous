import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

export type InputSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'lum-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true,
    },
  ],
  template: `
    <div class="w-full">
      @if (label) {
        <label [for]="inputId" class="label">
          {{ label }}
          @if (required) {
            <span class="text-danger">*</span>
          }
        </label>
      }

      <div class="relative">
        @if (prefixIcon) {
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-text-secondary">
            <ng-content select="[prefix]"></ng-content>
          </div>
        }

        <input
          [id]="inputId"
          [type]="type"
          [placeholder]="placeholder"
          [disabled]="disabled"
          [readonly]="readonly"
          [autocomplete]="autocomplete"
          [class]="inputClasses"
          [attr.aria-invalid]="error ? 'true' : null"
          [attr.aria-describedby]="error ? inputId + '-error' : hint ? inputId + '-hint' : null"
          [value]="value"
          (input)="onInput($event)"
          (blur)="onTouched()"
        />

        @if (suffixIcon) {
          <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none text-text-secondary">
            <ng-content select="[suffix]"></ng-content>
          </div>
        }
      </div>

      @if (hint && !error) {
        <p [id]="inputId + '-hint'" class="mt-2 text-body-sm text-text-secondary">{{ hint }}</p>
      }

      @if (error) {
        <p [id]="inputId + '-error'" class="mt-2 text-body-sm text-danger" role="alert">{{ error }}</p>
      }
    </div>
  `,
})
export class InputComponent implements ControlValueAccessor {
  @Input() label?: string;
  @Input() placeholder = '';
  @Input() type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search' = 'text';
  @Input() size: InputSize = 'md';
  @Input() hint?: string;
  @Input() error?: string;
  @Input() disabled = false;
  @Input() readonly = false;
  @Input() required = false;
  @Input() autocomplete = 'off';
  @Input() prefixIcon = false;
  @Input() suffixIcon = false;

  value = '';
  onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  private static counter = 0;
  private uniqueId = ++InputComponent.counter;

  get inputId(): string {
    const labelPart = this.label?.toLowerCase().replace(/\s+/g, '-') || 'field';
    return `input-${labelPart}-${this.uniqueId}`;
  }

  get inputClasses(): string {
    const base = 'input';

    const sizes: Record<InputSize, string> = {
      sm: 'input-sm',
      md: '',
      lg: 'input-lg',
    };

    const errorClass = this.error ? 'input-error' : '';
    const padding = this.prefixIcon ? 'pl-12' : this.suffixIcon ? 'pr-12' : '';

    return `${base} ${sizes[this.size]} ${errorClass} ${padding}`.trim();
  }

  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.value = input.value;
    this.onChange(this.value);
  }

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
