import { HttpContext } from '@angular/common/http';
import { Component, inject, input, output, signal } from '@angular/core';
import { ApiError } from '../../core/http/api-error';
import { ErrorNotifier } from '../../core/http/error-notifier';
import { HANDLES_ERRORS } from '../../core/http/error.interceptor';
import { FormErrors, asApiError } from '../../core/http/form-errors';
import { Distribution, DistributionOutcome, TrackDetail } from '../track.models';
import { TrackService } from '../track.service';

/**
 * "Mark live" / "Reject" for one Pending distribution: records the DSP's answer (in a real system
 * the DSP would report it). Either outcome is final, so nothing is shown once it isn't Pending.
 */
@Component({
  selector: 'app-distribution-outcome',
  imports: [FormErrors],
  template: `
    @if (distribution().status === 'Pending') {
      <div class="buttons">
        <button type="button" [disabled]="saving() !== null" (click)="record('Live')">
          {{ saving() === 'Live' ? 'Saving…' : 'Mark live' }}
        </button>
        <button type="button" [disabled]="saving() !== null" (click)="record('Rejected')">
          {{ saving() === 'Rejected' ? 'Saving…' : 'Reject' }}
        </button>
      </div>
      <app-form-errors [error]="error()" />
    }
  `,
  styles: `
    .buttons {
      display: flex;
      gap: 0.5rem;
    }
    button {
      padding: 0.125rem 0.5rem;
    }
  `,
})
export class DistributionOutcomeActions {
  private readonly trackService = inject(TrackService);
  private readonly notifier = inject(ErrorNotifier);

  readonly trackId = input.required<string>();
  readonly distribution = input.required<Distribution>();
  /** The track as returned by the API after a successful call. */
  readonly updated = output<TrackDetail>();

  /** The outcome being saved, or null when idle. */
  protected readonly saving = signal<DistributionOutcome | null>(null);
  protected readonly error = signal<ApiError | null>(null);

  protected record(outcome: DistributionOutcome): void {
    this.saving.set(outcome);
    this.error.set(null);

    this.trackService
      .changeDistributionStatus(
        this.trackId(),
        this.distribution().dspId,
        outcome,
        new HttpContext().set(HANDLES_ERRORS, true),
      )
      .subscribe({
        next: (track) => {
          this.saving.set(null);
          this.updated.emit(track);
        },
        error: (error: unknown) => {
          const apiError = asApiError(error);
          // The interceptor has already logged out; these buttons disappear with the session.
          if (apiError.status === 401) {
            this.notifier.show('Your session has expired. Log in again.');
          }
          this.error.set(apiError);
          this.saving.set(null);
        },
      });
  }
}
