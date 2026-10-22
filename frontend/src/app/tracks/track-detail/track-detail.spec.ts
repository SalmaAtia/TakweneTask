import { formatDate } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { environment } from '../../../environments/environment';
import { errorInterceptor } from '../../core/http/error.interceptor';
import { ErrorNotifier } from '../../core/http/error-notifier';
import { TrackDetail as TrackDetailModel } from '../track.models';
import { TrackDetail } from './track-detail';

const trackUrl = (id: string) => `${environment.apiBaseUrl}/tracks/${id}`;

@Component({ template: 'list' })
class ListStub {}

const track: TrackDetailModel = {
  id: 't1',
  title: 'Tok Tok Anthem',
  artistId: 'a1',
  artistName: 'Nour El Sayed',
  isrc: 'EG-NES-26-00002',
  releaseDate: '2026-11-27',
  genre: 'Mahraganat',
  status: 'Submitted',
  distributions: [
    { dspId: 'd1', dspName: 'Spotify', status: 'Live', submittedAt: '2026-10-15T11:30:00+00:00' },
    {
      dspId: 'd3',
      dspName: 'YouTube Music',
      status: 'Pending',
      submittedAt: '2026-10-15T11:35:00+00:00',
    },
  ],
};

describe('TrackDetail', () => {
  let harness: RouterTestingHarness;
  let backend: HttpTestingController;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter(
          [
            { path: '', component: ListStub },
            { path: 'tracks/:id', component: TrackDetail },
          ],
          withComponentInputBinding(),
        ),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    backend = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => backend.verify());

  const el = () => harness.routeNativeElement!;
  const text = () => el().textContent ?? '';

  async function load(url: string, respond: (id: string) => void) {
    await harness.navigateByUrl(url);
    respond(url.split('/')[2].split('?')[0]);
    await harness.fixture.whenStable();
  }

  const ok = (body: TrackDetailModel) => (id: string) =>
    backend.expectOne(trackUrl(id)).flush(body);

  it('shows a loading state until the API responds', async () => {
    await harness.navigateByUrl('/tracks/t1');
    expect(text()).toContain('Loading track');

    backend.expectOne(trackUrl('t1')).flush(track);
    await harness.fixture.whenStable();
    expect(text()).not.toContain('Loading track');
  });

  it('shows all track info', async () => {
    await load('/tracks/t1', ok(track));

    expect(el().querySelector('h2')?.textContent).toBe('Tok Tok Anthem');
    const info = Object.fromEntries(
      Array.from(el().querySelectorAll('dt')).map((dt) => [
        dt.textContent?.trim(),
        dt.nextElementSibling?.textContent?.trim(),
      ]),
    );
    expect(info).toEqual({
      Artist: 'Nour El Sayed',
      ISRC: 'EG-NES-26-00002',
      Genre: 'Mahraganat',
      'Release date': 'Nov 27, 2026',
      Status: 'Submitted',
    });
  });

  it('lists distributions with DSP name, status badge and submitted time in local time', async () => {
    await load('/tracks/t1', ok(track));

    const rows = el().querySelectorAll('tbody tr');
    expect(rows).toHaveLength(2);
    const cells = Array.from(rows[0].querySelectorAll('td')).map((td) => td.textContent?.trim());
    // formatDate without a timezone uses the machine's local zone, like the page does.
    expect(cells).toEqual([
      'Spotify',
      'Live',
      formatDate(track.distributions[0].submittedAt, 'medium', 'en-US'),
    ]);
    expect(rows[0].querySelector('app-status-badge span')?.className).toContain('badge-live');
    expect(rows[1].querySelector('app-status-badge span')?.className).toContain('badge-pending');
  });

  it('says "Not distributed yet" when there are no distributions', async () => {
    await load('/tracks/t1', ok({ ...track, status: 'Draft', distributions: [] }));

    expect(text()).toContain('Not distributed yet.');
    expect(el().querySelector('table')).toBeNull();
  });

  it('shows a not-found state for an unknown id, without the global banner', async () => {
    await load('/tracks/nope', (id) =>
      backend
        .expectOne(trackUrl(id))
        .flush(
          { status: 404, detail: "Track 'nope' was not found." },
          { status: 404, statusText: 'Not Found' },
        ),
    );

    expect(text()).toContain('Track not found');
    expect(text()).toContain('nope');
    expect(TestBed.inject(ErrorNotifier).message()).toBeNull();
  });

  it('shows other errors with a retry', async () => {
    await load('/tracks/t1', (id) =>
      backend
        .expectOne(trackUrl(id))
        .flush({ title: 'Server error' }, { status: 500, statusText: 'Error' }),
    );
    expect(el().querySelector('[role="alert"]')?.textContent).toContain('Server error');

    el().querySelector<HTMLButtonElement>('[role="alert"] button')!.click();
    (await vi.waitFor(() => backend.expectOne(trackUrl('t1')))).flush(track);
    await harness.fixture.whenStable();
    expect(el().querySelector('h2')?.textContent).toBe('Tok Tok Anthem');
  });

  it('links back to the list with the same status filter', async () => {
    await load('/tracks/t1?status=Submitted', ok(track));

    const back = el().querySelector<HTMLAnchorElement>('a')!;
    expect(back.getAttribute('href')).toBe('/?status=Submitted');

    back.click();
    await vi.waitFor(() => expect(TestBed.inject(Router).url).toBe('/?status=Submitted'));
  });

  it('links back to the unfiltered list when there was no filter', async () => {
    await load('/tracks/t1', ok(track));

    expect(el().querySelector('a')!.getAttribute('href')).toBe('/');
  });
});
