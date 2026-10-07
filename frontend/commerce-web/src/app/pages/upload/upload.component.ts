import { Component, inject, signal } from '@angular/core';
import { CommerceService } from '../../services/commerce.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.css'
})
export class UploadComponent {
  private readonly service = inject(CommerceService);

  readonly file = signal<File | null>(null);
  readonly headers = signal<string[]>([]);
  readonly rows = signal<string[][]>([]);
  readonly error = signal('');
  readonly message = signal('');
  readonly loading = signal(false);

  onFileSelected(event: Event): void {
    this.reset();
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!/^commerce_\d{8}\.csv$/i.test(file.name)) {
      this.error.set('El nombre del archivo debe ser commerce_DDMMYYYY.csv');
      return;
    }
    if (file.size === 0) {
      this.error.set('El archivo está vacío.');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const text = String(reader.result).replace(/^\uFEFF/, '');
      const lines = text.split(/\r?\n/).filter(l => l.trim() !== '');
      if (lines.length < 2) {
        this.error.set('El archivo no contiene registros.');
        return;
      }
      this.file.set(file);
      this.headers.set(this.parseLine(lines[0]));
      this.rows.set(lines.slice(1).map(l => this.parseLine(l)));
    };
    reader.readAsText(file);
  }

  send(): void {
    const file = this.file();
    if (!file) return;

    this.loading.set(true);
    this.error.set('');
    this.message.set('');

    this.service.upload(file).subscribe({
      next: res => {
        this.message.set(`Archivo enviado. Registros insertados: ${res.registrosInsertados}`);
        this.file.set(null);
        this.headers.set([]);
        this.rows.set([]);
        this.loading.set(false);
      },
        error: err => {
        this.error.set(err.error?.message ?? 'No se pudo enviar el archivo.');
        this.file.set(null);
        this.headers.set([]);
        this.rows.set([]);
        this.loading.set(false);
      }
    });
  }

  private reset(): void {
    this.file.set(null);
    this.headers.set([]);
    this.rows.set([]);
    this.error.set('');
    this.message.set('');
  }

  /** Separa una línea CSV respetando los campos entre comillas. */
  private parseLine(line: string): string[] {
    const result: string[] = [];
    let current = '';
    let inQuotes = false;
    for (let i = 0; i < line.length; i++) {
      const c = line[i];
      if (c === '"') {
        if (inQuotes && line[i + 1] === '"') { current += '"'; i++; }
        else inQuotes = !inQuotes;
      } else if (c === ',' && !inQuotes) {
        result.push(current);
        current = '';
      } else {
        current += c;
      }
    }
    result.push(current);
    return result;
  }
}