import { HttpClient, HttpParams } from '@angular/common/http';
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

  getList(query: TrackListQuery = {}): Observable<Track[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value) params = params.set(key, value);
    }

    return this.http.get<Track[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<TrackDetail> {
    return this.http.get<TrackDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateTrackRequest): Observable<TrackDetail> {
    return this.http.post<TrackDetail>(this.baseUrl, request);
  }

  distribute(id: string, dspIds: string[]): Observable<TrackDetail> {
    return this.http.post<TrackDetail>(`${this.baseUrl}/${id}/distribute`, { dspIds });
  }

  changeStatus(id: string, status: TrackStatus): Observable<TrackDetail> {
    return this.http.patch<TrackDetail>(`${this.baseUrl}/${id}/status`, { status });
  }
}
