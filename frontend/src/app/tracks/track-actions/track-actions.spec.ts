import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { OutputEmitterRef, Type } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ErrorNotifier } from '../../core/http/error-notifier';
import { errorInterceptor } from '../../core/http/error.interceptor';
import { TrackDetail } from '../track.models';
import { ChangeStatusForm } from './change-status-form';
import { DistributeForm } from './distribute-form';
import { DistributionOutcomeActions } from './distribution-outcome';

const api = environment.apiBaseUrl;

const track: TrackDetail = {
  id: 't1',
  title: 'Tok Tok Anthem',
  artistId: 'a1',
  artistName: 'Nour El Sayed',
  isrc: 'EG-NES-26-00002',
  releaseDate: '2026-11-27',
  genre: 'Mahraganat',
  status: 'Submitted',
  distributions: [
    {
      dspId: 'spotify',
      dspName: 'Spotify',
      status: 'Pending',
      submittedAt: '2026-10-15T11:30:00+00:00',
    },
  ],
};

const dsps = [
  { id: 'apple', name: 'Apple Music' },
  { id: 'spotify', name: 'Spotify' },
  { id: 'youtube', name: 'YouTube Music' },
];

let backend: HttpTestingController;

beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(withInterceptors([errorInterceptor])),
      provideHttpClientTesting(),
    ],
  });
  backend = TestBed.inject(HttpTestingController);
});

afterEach(() => backend.verify());

/** Both forms take a `track` input and emit the updated track via `updated`. */
async function render<T extends { updated: OutputEmitterRef<TrackDetail> }>(component: Type<T>) {
  const fixture = TestBed.createComponent(component);
  fixture.componentRef.setInput('track', track);
  const updated: TrackDetail[] = [];
  fixture.componentInstance.updated.subscribe((t) => updated.push(t));
  // Not whenStable(): DistributeForm's DSP request stays pending until the test flushes it.
  fixture.detectChanges();
  return { fixture, el: fixture.nativeElement as HTMLElement, updated };
}

describe('DistributeForm', () => {
  async function renderWithDsps() {
    const rendered = await render(DistributeForm);
    backend.expectOne(`${api}/dsps`).flush(dsps);
    await rendered.fixture.whenStable();
    return rendered;
  }

  const checkboxes = (el: HTMLElement) =>
    Array.from(el.querySelectorAll<HTMLInputElement>('input[type=checkbox]'));
  const button = (el: HTMLElement) => el.querySelector<HTMLButtonElement>('button')!;

  it('lists every DSP; the ones already distributed to are ticked and locked', async () => {
    const { el } = await renderWithDsps();

    const boxes = checkboxes(el);
    expect(boxes.map((b) => b.parentElement?.textContent?.trim())).toEqual([
      'Apple Music',
      'Spotify (already distributed)',
      'YouTube Music',
    ]);
    expect(boxes.map((b) => b.disabled)).toEqual([false, true, false]);
    expect(button(el).disabled).toBe(true);
  });

  it('distributes to the ticked DSPs and emits the updated track', async () => {
    const { fixture, el, updated } = await renderWithDsps();

    checkboxes(el)[0].click();
    checkboxes(el)[2].click();
    await fixture.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));

    const req = backend.expectOne(`${api}/tracks/t1/distribute`);
    expect(req.request.body).toEqual({ dspIds: ['apple', 'youtube'] });
    const response = { ...track, distributions: [...track.distributions] };
    req.flush(response);
    await fixture.whenStable();

    expect(updated).toEqual([response]);
    expect(button(el).disabled).toBe(true); // selection cleared
  });

  it('shows API errors inline, not in the banner', async () => {
    const { fixture, el } = await renderWithDsps();

    checkboxes(el)[0].click();
    await fixture.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    backend
      .expectOne(`${api}/tracks/t1/distribute`)
      .flush(
        { status: 409, detail: 'Track is already distributed.' },
        { status: 409, statusText: 'Conflict' },
      );
    await fixture.whenStable();

    expect(el.querySelector('.form-error')?.textContent).toBe('Track is already distributed.');
    expect(TestBed.inject(ErrorNotifier).message()).toBeNull();
  });

  it('shows validation errors for indexed fields too', async () => {
    const { fixture, el } = await renderWithDsps();

    checkboxes(el)[0].click();
    await fixture.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    backend.expectOne(`${api}/tracks/t1/distribute`).flush(
      {
        title: 'One or more validation errors occurred.',
        errors: { 'dspIds[0]': ["'Dsp Ids' must not be empty."] },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    expect(el.querySelector('.form-error')?.textContent).toBe("'Dsp Ids' must not be empty.");
  });

  it('points to the banner when the session has expired', async () => {
    const { fixture, el } = await renderWithDsps();

    checkboxes(el)[0].click();
    await fixture.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    backend
      .expectOne(`${api}/tracks/t1/distribute`)
      .flush({ status: 401 }, { status: 401, statusText: 'Unauthorized' });

    expect(TestBed.inject(ErrorNotifier).message()).toBe('Your session has expired. Log in again.');
  });
});

describe('ChangeStatusForm', () => {
  const select = (el: HTMLElement) => el.querySelector('select')!;

  async function choose(fixture: ComponentFixture<unknown>, el: HTMLElement, value: string) {
    select(el).value = value;
    select(el).dispatchEvent(new Event('change'));
    await fixture.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
  }

  it('offers every status except the current one', async () => {
    const { el } = await render(ChangeStatusForm);

    const options = Array.from(select(el).options).map((o) => o.value);
    expect(options).toEqual(['', 'Draft', 'Distributed']);
    expect(el.querySelector('button')!.disabled).toBe(true);
  });

  it('changes the status and emits the updated track', async () => {
    const { fixture, el, updated } = await render(ChangeStatusForm);

    await choose(fixture, el, 'Draft');
    const req = backend.expectOne({ method: 'PATCH', url: `${api}/tracks/t1/status` });
    expect(req.request.body).toEqual({ status: 'Draft' });
    req.flush({ ...track, status: 'Draft' });
    await fixture.whenStable();

    expect(updated.map((t) => t.status)).toEqual(['Draft']);
  });

  it('shows a rejected transition inline', async () => {
    const { fixture, el } = await render(ChangeStatusForm);

    await choose(fixture, el, 'Distributed');
    backend.expectOne(`${api}/tracks/t1/status`).flush(
      {
        status: 409,
        detail: 'A track can only become Distributed once at least one distribution is Live.',
      },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    expect(el.querySelector('.form-error')?.textContent).toContain(
      'at least one distribution is Live',
    );
  });
});

describe('DistributionOutcomeActions', () => {
  async function renderOutcome(status: TrackDetail['distributions'][number]['status']) {
    const fixture = TestBed.createComponent(DistributionOutcomeActions);
    fixture.componentRef.setInput('trackId', 't1');
    fixture.componentRef.setInput('distribution', { ...track.distributions[0], status });
    const updated: TrackDetail[] = [];
    fixture.componentInstance.updated.subscribe((t) => updated.push(t));
    await fixture.whenStable();
    return { fixture, el: fixture.nativeElement as HTMLElement, updated };
  }

  const buttons = (el: HTMLElement) => Array.from(el.querySelectorAll('button'));

  it('offers Mark live and Reject for a Pending distribution only', async () => {
    expect(buttons((await renderOutcome('Pending')).el).map((b) => b.textContent?.trim())).toEqual([
      'Mark live',
      'Reject',
    ]);
    expect(buttons((await renderOutcome('Live')).el)).toHaveLength(0);
    expect(buttons((await renderOutcome('Rejected')).el)).toHaveLength(0);
  });

  it.each([
    ['Mark live', 'Live'],
    ['Reject', 'Rejected'],
  ])('"%s" records %s and emits the updated track', async (label, status) => {
    const { fixture, el, updated } = await renderOutcome('Pending');

    buttons(el)
      .find((b) => b.textContent?.trim() === label)!
      .click();
    await fixture.whenStable();
    expect(buttons(el).every((b) => b.disabled)).toBe(true); // no double submit

    const req = backend.expectOne({
      method: 'PATCH',
      url: `${api}/tracks/t1/distributions/spotify`,
    });
    expect(req.request.body).toEqual({ status });
    const response = {
      ...track,
      distributions: [{ ...track.distributions[0], status: status as 'Live' | 'Rejected' }],
    };
    req.flush(response);
    await fixture.whenStable();

    expect(updated).toEqual([response]);
  });

  it('shows API errors inline, not in the banner', async () => {
    const { fixture, el } = await renderOutcome('Pending');

    buttons(el)[0].click();
    backend
      .expectOne(`${api}/tracks/t1/distributions/spotify`)
      .flush(
        { status: 409, detail: "Distribution to DSP 'spotify' is already Live." },
        { status: 409, statusText: 'Conflict' },
      );
    await fixture.whenStable();

    expect(el.querySelector('.form-error')?.textContent).toContain('already Live');
    expect(TestBed.inject(ErrorNotifier).message()).toBeNull();
    expect(buttons(el).some((b) => b.disabled)).toBe(false);
  });
});
