import { HttpContext } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { HANDLES_ERRORS } from '../../core/http/error.interceptor';
import { StatusBadge } from '../status-badge/status-badge';
import { TRACK_STATUSES, TrackStatus, parseTrackStatus } from '../track.models';
import { TrackService } from '../track.service';

/**
 * Track table with a status filter. The filter lives in the URL (`?status=`), so it survives a
 * refresh and works with back/forward: changing the dropdown navigates, the router binds the query
 * param to the `status` input, and the resource reloads from it.
 */
@Component({
  selector: 'app-track-list',
  imports: [DatePipe, RouterLink, StatusBadge],
  templateUrl: './track-list.html',
  styleUrl: './track-list.css',
})
export class TrackList {
  private readonly trackService = inject(TrackService);
  private readonly router = inject(Router);

  protected readonly statuses = TRACK_STATUSES;

  /** Raw `?status=` query param (withComponentInputBinding). */
  readonly status = input<string>();

  /** Unknown values (e.g. a hand-edited URL) mean "All" rather than a 400 from the API. */
  protected readonly statusFilter = computed(() => parseTrackStatus(this.status()));

  protected readonly tracks = rxResource({
    params: () => ({ status: this.statusFilter() }),
    // The page shows its own error state, so skip the global banner.
    stream: ({ params }) =>
      this.trackService.getList(params, new HttpContext().set(HANDLES_ERRORS, true)),
  });

  protected onStatusChange(value: string): void {
    this.router.navigate([], {
      queryParams: { status: parseTrackStatus(value) },
      queryParamsHandling: 'merge',
    });
  }

  protected openTrack(id: string, event: MouseEvent): void {
    // The title is a real link (keyboard, middle-click); don't navigate twice when it's clicked.
    if ((event.target as HTMLElement).closest('a')) return;
    this.router.navigate(['/tracks', id]);
  }

  protected isSelected(status: TrackStatus | null): boolean {
    return this.statusFilter() === status;
  }
}
