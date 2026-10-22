import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Placeholder: track details, distribution and status changes come next. Lazy-loaded. */
@Component({
  selector: 'app-track-detail',
  imports: [RouterLink],
  template: `
    <p><a routerLink="/">&larr; Back to tracks</a></p>
    <h2>Track</h2>
    <p>
      Details for track <code>{{ id() }}</code> go here.
    </p>
  `,
})
export class TrackDetail {
  /** From the `:id` route parameter (withComponentInputBinding). */
  readonly id = input.required<string>();
}
