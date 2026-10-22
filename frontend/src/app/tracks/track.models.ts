// Mirrors the backend DTOs in TrackManagement.Application/Tracks/Dtos.
// Dates arrive as strings: DateOnly as 'YYYY-MM-DD', DateTimeOffset as ISO 8601.

export const TRACK_STATUSES = ['Draft', 'Submitted', 'Distributed'] as const;

export type TrackStatus = (typeof TRACK_STATUSES)[number];

/** The canonical status for a case-insensitive name (e.g. from a URL), or null if it isn't one. */
export function parseTrackStatus(value: string | null | undefined): TrackStatus | null {
  return TRACK_STATUSES.find((s) => s.toLowerCase() === value?.toLowerCase()) ?? null;
}

export type DistributionStatus = 'Pending' | 'Live' | 'Rejected';

/** What a DSP can answer for a Pending distribution: the only statuses a caller may set. */
export type DistributionOutcome = Exclude<DistributionStatus, 'Pending'>;

/** GET /api/tracks item (TrackListItemDto). */
export interface Track {
  id: string;
  title: string;
  artistId: string;
  artistName: string;
  isrc: string;
  releaseDate: string;
  genre: string;
  status: TrackStatus;
}

/** GET /api/tracks/{id} (TrackDetailDto). */
export interface TrackDetail {
  id: string;
  title: string;
  artistId: string;
  artistName: string;
  isrc: string;
  releaseDate: string;
  genre: string;
  status: TrackStatus;
  distributions: Distribution[];
}

/** TrackDistributionDto. */
export interface Distribution {
  dspId: string;
  dspName: string;
  status: DistributionStatus;
  submittedAt: string;
}

/** Query-string filters for GET /api/tracks. Omitted/empty values don't filter. */
export interface TrackListQuery {
  artistId?: string | null;
  genre?: string | null;
  status?: TrackStatus | null;
}

/** POST /api/tracks (CreateTrackDto). ISRC accepted with or without hyphens. */
export interface CreateTrackRequest {
  title: string;
  artistId: string;
  isrc: string;
  releaseDate: string;
  genre: string;
}

/** POST /api/tracks/{id}/distribute (DistributeTrackDto). */
export interface DistributeTrackRequest {
  dspIds: string[];
}

/** PATCH /api/tracks/{id}/status (ChangeTrackStatusDto). */
export interface ChangeTrackStatusRequest {
  status: TrackStatus;
}

/** PATCH /api/tracks/{id}/distributions/{dspId} (ChangeDistributionStatusDto). */
export interface ChangeDistributionStatusRequest {
  status: DistributionOutcome;
}
