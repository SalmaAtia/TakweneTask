import { HttpContext } from '@angular/common/http';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { ApiError } from '../../core/http/api-error';
import { ErrorNotifier } from '../../core/http/error-notifier';
import { HANDLES_ERRORS } from '../../core/http/error.interceptor';
import { FormErrors, asApiError } from '../../core/http/form-errors';
import { TRACK_STATUSES, TrackDetail, TrackStatus } from '../track.models';
import { TrackService } from '../track.service';

/**
 * Moves the track to another status. Every other status is offered: the API owns the transition
 * rules and its 409 message (e.g. "needs a Live distribution") is shown inline.
 */
@Component({
  selector: 'app-change-status-form',
  imports: [FormErrors],
  template: `
    <form (submit)="$event.preventDefault(); submit()">
      <h3>Change status</h3>

      <label>
        New status
        <select [disabled]="submitting()" (change)="target.set($any($event.target).value || null)">
          <option value="" [selected]="target() === null">Choose…</option>
          @for (status of targets(); track status) {
            <option [value]="status" [selected]="target() === status">{{ status }}</option>
          }
        </select>
      </label>
      <app-form-errors [error]="error()" field="status" />
      <app-form-errors [error]="error()" [shownElsewhere]="['status']" />

      <button type="submit" [disabled]="target() === null || submitting()">
        {{ submitting() ? 'Saving…' : 'Change status' }}
      </button>
    </form>
  `,
  styleUrl: './track-actions.css',
})
export class ChangeStatusForm {
  private readonly trackService = inject(TrackService);
  private readonly notifier = inject(ErrorNotifier);

  readonly track = input.required<TrackDetail>();
  /** The track as returned by the API after a successful call. */
  readonly updated = output<TrackDetail>();

  protected readonly targets = computed(() =>
    TRACK_STATUSES.filter((s) => s !== this.track().status),
  );
  protected readonly target = signal<TrackStatus | null>(null);
  protected readonly submitting = signal(false);
  protected readonly error = signal<ApiError | null>(null);

  protected submit(): void {
    const status = this.target();
    if (!status) return;

    this.submitting.set(true);
    this.error.set(null);

    this.trackService
      .changeStatus(this.track().id, status, new HttpContext().set(HANDLES_ERRORS, true))
      .subscribe({
        next: (track) => {
          this.target.set(null);
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
