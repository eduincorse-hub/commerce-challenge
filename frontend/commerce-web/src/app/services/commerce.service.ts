import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface QuarantineRow {
  id: number;
  pcCodComercio: string;
  pcNomComRed: string;
  pcRazonSocial: string;
  pcTipDoc: string;
  pcNumDoc: string;
  pcDireccion: string;
  pcTelefono: string;
  pcEmail: string;
  pcProcessDate: string;
  motivo: string;
}

@Injectable({ providedIn: 'root' })
export class CommerceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7030/api/Commerce';

  upload(file: File): Observable<{ registrosInsertados: number }> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<{ registrosInsertados: number }>(`${this.baseUrl}/upload`, form);
  }

  process(processDate: string): Observable<{ registrosCuarentena: number }> {
    const params = new HttpParams().set('processDate', processDate);
    return this.http.post<{ registrosCuarentena: number }>(`${this.baseUrl}/process`, null, { params });
  }

  getQuarantine(processDate?: string): Observable<QuarantineRow[]> {
    let params = new HttpParams();
    if (processDate) params = params.set('processDate', processDate);
    return this.http.get<QuarantineRow[]>(`${this.baseUrl}/quarantine`, { params });
  }
}