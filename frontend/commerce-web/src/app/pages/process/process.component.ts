import { Component, inject, signal } from '@angular/core';
import { CommerceService } from '../../services/commerce.service';

@Component({
  selector: 'app-process',
  templateUrl: './process.component.html',
  styleUrl: './process.component.css'
})
export class ProcessComponent {
  private readonly service = inject(CommerceService);

  readonly processDate = signal('');
  readonly loading = signal(false);
  readonly result = signal<number | null>(null);
  readonly error = signal('');

  onDateChange(event: Event): void {
    this.processDate.set((event.target as HTMLInputElement).value);
  }

  process(): void {
    if (!this.processDate()) {
      this.error.set('Selecciona una fecha de proceso.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.result.set(null);

    this.service.process(this.processDate()).subscribe({
      next: res => {
        this.result.set(res.registrosCuarentena);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo procesar la información.');
        this.loading.set(false);
      }
    });
  }
}