import { Routes } from '@angular/router';
import { TrackList } from './tracks/track-list/track-list';

export const routes: Routes = [
  { path: '', component: TrackList, title: 'Tracks' },
  {
    path: 'tracks/:id',
    loadComponent: () => import('./tracks/track-detail/track-detail').then((m) => m.TrackDetail),
    title: 'Track',
  },
  { path: '**', redirectTo: '' },
];
