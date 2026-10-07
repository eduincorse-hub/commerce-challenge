import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CommerceService, QuarantineRow } from '../../services/commerce.service';

@Component({
  selector: 'app-errors',
  imports: [DatePipe],
  templateUrl: './errors.component.html',
  styleUrl: './errors.component.css'
})
export class ErrorsComponent implements OnInit {
  private readonly service = inject(CommerceService);

  readonly filterDate = signal('');
  readonly rows = signal<QuarantineRow[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');

  ngOnInit(): void {
    this.load();
  }

  onDateChange(event: Event): void {
    this.filterDate.set((event.target as HTMLInputElement).value);
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');

    this.service.getQuarantine(this.filterDate() || undefined).subscribe({
      next: data => {
        this.rows.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo obtener la lista de registros con error.');
        this.loading.set(false);
      }
    });
  }

  clearFilter(): void {
    this.filterDate.set('');
    this.load();
  }
}