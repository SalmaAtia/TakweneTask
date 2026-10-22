import { HttpContext } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ApiError } from '../../core/http/api-error';
import { HANDLES_ERRORS } from '../../core/http/error.interceptor';
import { StatusBadge } from '../status-badge/status-badge';
import { TrackService } from '../track.service';

/**
 * One track and its DSP distributions. Lazy-loaded. The list passes its `?status=` filter along in
 * the URL, and the back link hands it back, so returning keeps the list filtered (also after a refresh).
 */
@Component({
  selector: 'app-track-detail',
  imports: [DatePipe, RouterLink, StatusBadge],
  templateUrl: './track-detail.html',
  styleUrl: './track-detail.css',
})
export class TrackDetail {
  private readonly trackService = inject(TrackService);

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
