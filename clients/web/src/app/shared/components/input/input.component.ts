import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-input',
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
        <label [for]="inputId" class="block text-sm font-medium text-gray-700 mb-1.5">
          {{ label }}
          @if (required) {
            <span class="text-red-500">*</span>
          }
        </label>
      }

      <div class="relative">
        @if (prefixIcon) {
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
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
          [value]="value"
          (input)="onInput($event)"
          (blur)="onTouched()"
        />

        @if (suffixIcon) {
          <div class="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
            <ng-content select="[suffix]"></ng-content>
          </div>
        }
      </div>

      @if (hint && !error) {
        <p class="mt-1 text-sm text-gray-500">{{ hint }}</p>
      }

      @if (error) {
        <p class="mt-1 text-sm text-red-600">{{ error }}</p>
      }
    </div>
  `,
})
export class InputComponent implements ControlValueAccessor {
  @Input() label?: string;
  @Input() placeholder = '';
  @Input() type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' = 'text';
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

  get inputId(): string {
    return `input-${this.label?.toLowerCase().replace(/\s+/g, '-') || Math.random().toString(36).slice(2)}`;
  }

  get inputClasses(): string {
    const base = 'block w-full px-4 py-3 rounded-lg transition-colors min-h-touch';
    const border = this.error
      ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
      : 'border-gray-300 focus:border-primary-500 focus:ring-primary-500';
    const state = this.disabled
      ? 'bg-gray-50 text-gray-500 cursor-not-allowed'
      : 'bg-white';
    const padding = this.prefixIcon ? 'pl-10' : this.suffixIcon ? 'pr-10' : '';

    return `${base} border ${border} ${state} ${padding} focus:ring-2 placeholder:text-gray-400`;
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
