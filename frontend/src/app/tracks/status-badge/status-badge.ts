import { Component, input } from '@angular/core';
import { TrackStatus } from '../track.models';

/** Colored pill for a track status. */
@Component({
  selector: 'app-status-badge',
  template: `<span [class]="'badge badge-' + status().toLowerCase()">{{ status() }}</span>`,
  styles: `
    .badge {
      display: inline-block;
      padding: 0.125rem 0.5rem;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 600;
      white-space: nowrap;
    }
    .badge-draft {
      background: #e5e7eb;
      color: #374151;
    }
    .badge-submitted {
      background: #dbeafe;
      color: #1e40af;
    }
    .badge-distributed {
      background: #dcfce7;
      color: #166534;
    }
  `,
})
export class StatusBadge {
  readonly status = input.required<TrackStatus>();
}
