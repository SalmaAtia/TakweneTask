import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { AuthService } from './core/auth/auth.service';
import { ErrorNotifier } from './core/http/error-notifier';

describe('App', () => {
  beforeEach(async () => {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('renders the header', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const header = (fixture.nativeElement as HTMLElement).querySelector('header');
    expect(header?.textContent).toContain('Track Management');
  });

  it('shows the latest API error in a dismissible banner', async () => {
    const fixture = TestBed.createComponent(App);
    const element = fixture.nativeElement as HTMLElement;

    TestBed.inject(ErrorNotifier).show('Track not found.');
    await fixture.whenStable();
    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Track not found.');

    element.querySelector<HTMLButtonElement>('[role="alert"] button')!.click();
    await fixture.whenStable();
    expect(element.querySelector('[role="alert"]')).toBeNull();
  });

  it('shows "Log in" when logged out and "Log out" when logged in', async () => {
    const fixture = TestBed.createComponent(App);
    const header = () => (fixture.nativeElement as HTMLElement).querySelector('header')!;
    await fixture.whenStable();
    expect(header().querySelector('a.session')?.textContent).toBe('Log in');

    sessionStorage.setItem(
      'trackManagement.auth',
      JSON.stringify({
        accessToken: 'jwt',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
      }),
    );
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([])] });
    const loggedIn = TestBed.createComponent(App);
    await loggedIn.whenStable();
    const button = (loggedIn.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      'button.session',
    )!;
    expect(button.textContent).toBe('Log out');

    button.click();
    await loggedIn.whenStable();
    expect(TestBed.inject(AuthService).isLoggedIn()).toBe(false);
    expect((loggedIn.nativeElement as HTMLElement).querySelector('a.session')?.textContent).toBe(
      'Log in',
    );
  });
});
