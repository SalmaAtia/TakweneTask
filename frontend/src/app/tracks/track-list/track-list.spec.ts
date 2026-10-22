import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { environment } from '../../../environments/environment';
import { errorInterceptor } from '../../core/http/error.interceptor';
import { ErrorNotifier } from '../../core/http/error-notifier';
import { Track } from '../track.models';
import { TrackList } from './track-list';

const tracksUrl = `${environment.apiBaseUrl}/tracks`;

@Component({ template: 'detail' })
class DetailStub {}

const tracks: Track[] = [
  {
    id: 't1',
    title: 'Tok Tok Anthem',
    artistId: 'a1',
    artistName: 'Nour El Sayed',
    isrc: 'EG-NES-26-00002',
    releaseDate: '2026-11-27',
    genre: 'Mahraganat',
    status: 'Submitted',
  },
  {
    id: 't2',
    title: 'Cedar Heart',
    artistId: 'a3',
    artistName: 'Lina Haddad',
    isrc: 'LB-LHD-26-00002',
    releaseDate: '2026-11-20',
    genre: 'Arabic Pop',
    status: 'Distributed',
  },
];

describe('TrackList', () => {
  let harness: RouterTestingHarness;
  let backend: HttpTestingController;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter(
          [
            { path: '', component: TrackList },
            { path: 'tracks/:id', component: DetailStub },
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
  const select = () => el().querySelector('select')!;

  async function choose(value: string) {
    select().value = value;
    select().dispatchEvent(new Event('change'));
  }

  /** Navigates, answers the list request (returning it for inspection), and renders. */
  async function load(url: string, body: Track[] = tracks) {
    await harness.navigateByUrl(url);
    const req = backend.expectOne((r) => r.url === tracksUrl);
    req.flush(body);
    await harness.fixture.whenStable();
    return req.request;
  }

  it('shows a loading state until the API responds', async () => {
    await harness.navigateByUrl('/');
    expect(text()).toContain('Loading tracks');

    backend.expectOne(tracksUrl).flush(tracks);
    await harness.fixture.whenStable();
    expect(text()).not.toContain('Loading tracks');
  });

  it('renders a row per track with title, artist, genre, status badge and release date', async () => {
    await load('/');

    const rows = el().querySelectorAll('tbody tr');
    expect(rows).toHaveLength(2);

    const cells = Array.from(rows[0].querySelectorAll('td')).map((td) => td.textContent?.trim());
    expect(cells).toEqual([
      'Tok Tok Anthem',
      'Nour El Sayed',
      'Mahraganat',
      'Submitted',
      'Nov 27, 2026',
    ]);
    expect(rows[0].querySelector('app-status-badge span')?.className).toContain('badge-submitted');
  });

  it('shows an empty state', async () => {
    await load('/', []);
    expect(text()).toContain('No tracks yet.');
    expect(el().querySelector('table')).toBeNull();
  });

  it('shows an error state with retry, without the global banner', async () => {
    await harness.navigateByUrl('/');
    backend
      .expectOne(tracksUrl)
      .flush({ title: 'Server error', status: 500 }, { status: 500, statusText: 'Server Error' });
    await harness.fixture.whenStable();

    expect(el().querySelector('[role="alert"]')?.textContent).toContain('Server error');
    expect(TestBed.inject(ErrorNotifier).message()).toBeNull();

    el().querySelector<HTMLButtonElement>('[role="alert"] button')!.click();
    (await vi.waitFor(() => backend.expectOne(tracksUrl))).flush(tracks);
    await harness.fixture.whenStable();
    expect(el().querySelectorAll('tbody tr')).toHaveLength(2);
  });

  it('reads the filter from ?status= (case-insensitive) and sends it to the API', async () => {
    const request = await load('/?status=submitted', [tracks[0]]);

    expect(request.params.get('status')).toBe('Submitted');
    expect(select().value).toBe('Submitted');
  });

  it('treats an unknown ?status= as All', async () => {
    const request = await load('/?status=bogus');

    expect(request.params.has('status')).toBe(false);
    expect(select().value).toBe('');
  });

  it('writes the selected status to the URL and reloads', async () => {
    const router = TestBed.inject(Router);
    await load('/');

    // Not whenStable(): it would wait for the list request, which only resolves once flushed.
    await choose('Draft');
    await vi.waitFor(() => expect(router.url).toBe('/?status=Draft'));
    const req = await vi.waitFor(() => backend.expectOne((r) => r.url === tracksUrl));
    expect(req.request.params.get('status')).toBe('Draft');
    req.flush([]);
    await harness.fixture.whenStable();
    expect(text()).toContain('No Draft tracks.');

    await choose('');
    await vi.waitFor(() => expect(router.url).toBe('/'));
    (await vi.waitFor(() => backend.expectOne(tracksUrl))).flush(tracks);
  });

  it('navigates to the detail page when a row is clicked', async () => {
    await load('/');

    el()
      .querySelectorAll<HTMLElement>('tbody tr')[1]
      .querySelector('td:nth-child(3)')!
      .dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await harness.fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/tracks/t2');
  });

  it('carries the status filter to the detail page (row click and title link)', async () => {
    await load('/?status=Submitted');

    const row = el().querySelectorAll<HTMLElement>('tbody tr')[1];
    expect(row.querySelector('a')!.getAttribute('href')).toBe('/tracks/t2?status=Submitted');

    row.querySelector('td:nth-child(3)')!.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/tracks/t2?status=Submitted');
  });
});
