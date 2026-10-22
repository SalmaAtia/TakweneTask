import { HttpContext } from '@angular/common/http';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ApiError } from '../../core/http/api-error';
import { ErrorNotifier } from '../../core/http/error-notifier';
import { HANDLES_ERRORS } from '../../core/http/error.interceptor';
import { FormErrors, asApiError } from '../../core/http/form-errors';
import { DspService } from '../../dsps/dsp.service';
import { TrackDetail } from '../track.models';
import { TrackService } from '../track.service';

/** Sends the track to the DSPs ticked in the list. DSPs it's already on are shown but can't be picked. */
@Component({
  selector: 'app-distribute-form',
  imports: [FormErrors],
  template: `
    <form (submit)="$event.preventDefault(); submit()">
      <h3>Distribute</h3>

      @if (dsps.isLoading()) {
        <p>Loading DSPs…</p>
      } @else if (dsps.error(); as error) {
        <p class="form-error">Couldn't load DSPs: {{ error.message }}</p>
      } @else if (dsps.hasValue()) {
        <fieldset [disabled]="submitting()">
          <legend>DSPs</legend>
          @for (dsp of dsps.value(); track dsp.id) {
            <label>
              <input
                type="checkbox"
                [checked]="isSelected(dsp.id) || isDistributed(dsp.id)"
                [disabled]="isDistributed(dsp.id)"
                (change)="toggle(dsp.id)"
              />
              {{ dsp.name }}
              @if (isDistributed(dsp.id)) {
                <small>(already distributed)</small>
              }
            </label>
          }
        </fieldset>

        <app-form-errors [error]="error()" />

        <button type="submit" [disabled]="selected().size === 0 || submitting()">
          {{ submitting() ? 'Distributing…' : 'Distribute' }}
        </button>
      }
    </form>
  `,
  styleUrl: './track-actions.css',
})
export class DistributeForm {
  private readonly trackService = inject(TrackService);
  private readonly dspService = inject(DspService);
  private readonly notifier = inject(ErrorNotifier);

  readonly track = input.required<TrackDetail>();
  /** The track as returned by the API after a successful call. */
  readonly updated = output<TrackDetail>();

  protected readonly dsps = rxResource({ stream: () => this.dspService.getList() });
  protected readonly selected = signal<ReadonlySet<string>>(new Set());
  protected readonly submitting = signal(false);
  protected readonly error = signal<ApiError | null>(null);

  private readonly distributedIds = computed(
    () => new Set(this.track().distributions.map((d) => d.dspId)),
  );

  protected isDistributed(dspId: string): boolean {
    return this.distributedIds().has(dspId);
  }

  protected isSelected(dspId: string): boolean {
    return this.selected().has(dspId);
  }

  protected toggle(dspId: string): void {
    const next = new Set(this.selected());
    if (!next.delete(dspId)) next.add(dspId);
    this.selected.set(next);
  }

  protected submit(): void {
    this.submitting.set(true);
    this.error.set(null);

    this.trackService
      .distribute(
        this.track().id,
        [...this.selected()],
        new HttpContext().set(HANDLES_ERRORS, true),
      )
      .subscribe({
        next: (track) => {
          this.selected.set(new Set());
          this.submitting.set(false);
          this.updated.emit(track);
        },
        error: (error: unknown) => {
          const apiError = asApiError(error);
          // The interceptor has already logged out; this form disappears with the session.
          if (apiError.status === 401) {
            this.notifier.show('Your session has expired. Log in again.');
          }
          this.error.set(apiError);
          this.submitting.set(false);
        },
      });
  }
}
