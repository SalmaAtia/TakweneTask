import { Injectable, signal } from '@angular/core';

/** The latest API error message, shown in the shell's banner until dismissed or replaced. */
@Injectable({ providedIn: 'root' })
export class ErrorNotifier {
  private readonly _message = signal<string | null>(null);
  readonly message = this._message.asReadonly();

  show(message: string): void {
    this._message.set(message);
  }

  clear(): void {
    this._message.set(null);
  }
}
