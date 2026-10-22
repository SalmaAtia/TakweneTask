import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { TrackService } from './track.service';

const api = environment.apiBaseUrl;

describe('TrackService', () => {
  let service: TrackService;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TrackService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('sends only the filters that are set', () => {
    service.getList({ status: 'Submitted', genre: '', artistId: null }).subscribe();

    const req = backend.expectOne((r) => r.url === `${api}/tracks`);
    expect(req.request.params.keys()).toEqual(['status']);
    expect(req.request.params.get('status')).toBe('Submitted');
  });

  it('maps each action to its endpoint', () => {
    service.getById('t1').subscribe();
    service.distribute('t1', ['d1']).subscribe();
    service.changeStatus('t1', 'Draft').subscribe();

    expect(backend.expectOne({ method: 'GET', url: `${api}/tracks/t1` })).toBeTruthy();
    expect(
      backend.expectOne({ method: 'POST', url: `${api}/tracks/t1/distribute` }).request.body,
    ).toEqual({ dspIds: ['d1'] });
    expect(
      backend.expectOne({ method: 'PATCH', url: `${api}/tracks/t1/status` }).request.body,
    ).toEqual({ status: 'Draft' });
  });
});
