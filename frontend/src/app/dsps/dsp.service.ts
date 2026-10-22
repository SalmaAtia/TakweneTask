import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

/** GET /api/dsps item (DspDto). */
export interface Dsp {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class DspService {
  private readonly http = inject(HttpClient);

  /** All DSPs, sorted by name. */
  getList(): Observable<Dsp[]> {
    return this.http.get<Dsp[]>(`${environment.apiBaseUrl}/dsps`);
  }
}
