import {
  Component,
  ElementRef,
  forwardRef,
  ViewChild,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-rich-text-editor',
  standalone: true,
  templateUrl: './rich-text-editor.component.html',
  styles: `
    .editor {
      display: block;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      overflow: hidden;
      background-color: var(--bg-input);
    }

    .toolbar {
      display: flex;
      flex-wrap: wrap;
      gap: 0.25rem;
      padding: 0.35rem;
      border-bottom: 1px solid var(--border-color);
      background-color: var(--bg-hover);
    }

    .toolbar button {
      border: 1px solid var(--border-color);
      background-color: var(--bg-input);
      color: var(--text-primary);
      border-radius: 0.25rem;
      padding: 0.2rem 0.45rem;
      cursor: pointer;
      font-size: 0.8rem;
    }

    .editable {
      min-height: 120px;
      padding: 0.65rem 0.75rem;
      outline: none;
      line-height: 1.5;
    }

    .editable:empty::before {
      content: attr(data-placeholder);
      color: #94a3b8;
    }
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RichTextEditorComponent),
      multi: true,
    },
  ],
})
export class RichTextEditorComponent implements ControlValueAccessor {
  @ViewChild('editable') editableRef?: ElementRef<HTMLDivElement>;

  readonly placeholder = signal('Write something…');

  private onChange: (value: string) => void = () => undefined;
  onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    queueMicrotask(() => {
      if (this.editableRef?.nativeElement) {
        this.editableRef.nativeElement.innerHTML = value ?? '';
      }
    });
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (this.editableRef?.nativeElement) {
      this.editableRef.nativeElement.contentEditable = String(!isDisabled);
    }
  }

  format(command: string): void {
    document.execCommand(command, false);
    this.onInput();
    this.editableRef?.nativeElement.focus();
  }

  onInput(): void {
    const html = this.editableRef?.nativeElement.innerHTML ?? '';
    this.onChange(html);
  }
}
