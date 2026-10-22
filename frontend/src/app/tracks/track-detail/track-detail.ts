import { HttpContext } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ApiError } from '../../core/http/api-error';
import { HANDLES_ERRORS } from '../../core/http/error.interceptor';
import { StatusBadge } from '../status-badge/status-badge';
import { ChangeStatusForm } from '../track-actions/change-status-form';
import { DistributeForm } from '../track-actions/distribute-form';
import { TrackService } from '../track.service';

/**
 * One track and its DSP distributions. Lazy-loaded. The list passes its `?status=` filter along in
 * the URL, and the back link hands it back, so returning keeps the list filtered (also after a refresh).
 */
@Component({
  selector: 'app-track-detail',
  imports: [DatePipe, RouterLink, StatusBadge, DistributeForm, ChangeStatusForm],
  templateUrl: './track-detail.html',
  styleUrl: './track-detail.css',
})
export class TrackDetail {
  private readonly trackService = inject(TrackService);
  protected readonly auth = inject(AuthService);
  protected readonly router = inject(Router);

  /** From the `:id` route parameter (withComponentInputBinding). */
  readonly id = input.required<string>();

  protected readonly track = rxResource({
    params: () => this.id(),
    // The page shows its own not-found and error states, so skip the global banner.
    stream: ({ params: id }) =>
      this.trackService.getById(id, new HttpContext().set(HANDLES_ERRORS, true)),
  });

  protected readonly notFound = computed(() => {
    const error = this.track.error();
    return error instanceof ApiError && error.status === 404;
  });
}
