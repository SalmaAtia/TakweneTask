import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateTrackRequest,
  Track,
  TrackDetail,
  TrackListQuery,
  TrackStatus,
} from './track.models';

@Injectable({ providedIn: 'root' })
export class TrackService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/tracks`;

  /** Pass `new HttpContext().set(HANDLES_ERRORS, true)` when the caller shows the error itself. */
  getList(query: TrackListQuery = {}, context?: HttpContext): Observable<Track[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value) params = params.set(key, value);
    }

    return this.http.get<Track[]>(this.baseUrl, { params, context });
  }

  getById(id: string, context?: HttpContext): Observable<TrackDetail> {
    return this.http.get<TrackDetail>(`${this.baseUrl}/${id}`, { context });
  }

  create(request: CreateTrackRequest): Observable<TrackDetail> {
    return this.http.post<TrackDetail>(this.baseUrl, request);
  }

  distribute(id: string, dspIds: string[], context?: HttpContext): Observable<TrackDetail> {
    return this.http.post<TrackDetail>(`${this.baseUrl}/${id}/distribute`, { dspIds }, { context });
  }

  changeStatus(id: string, status: TrackStatus, context?: HttpContext): Observable<TrackDetail> {
    return this.http.patch<TrackDetail>(`${this.baseUrl}/${id}/status`, { status }, { context });
  }
}
