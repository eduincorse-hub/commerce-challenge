import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface LoginResponse {
  token: string;
  expiresAt: string;
  username: string;
}

const TOKEN_KEY = 'commerce_token';
const USER_KEY = 'commerce_user';
const EXPIRES_KEY = 'commerce_expires';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = 'https://localhost:7030/api/Auth';

  private readonly tokenSignal = signal<string | null>(this.readValidToken());
  readonly username = signal<string | null>(sessionStorage.getItem(USER_KEY));
  readonly isLoggedIn = computed(() => this.tokenSignal() !== null);

  get token(): string | null {
    return this.tokenSignal();
  }

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/login`, { username, password })
      .pipe(tap(res => this.saveSession(res)));
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    sessionStorage.removeItem(EXPIRES_KEY);
    this.tokenSignal.set(null);
    this.username.set(null);
    this.router.navigateByUrl('/login');
  }

  private saveSession(res: LoginResponse): void {
    sessionStorage.setItem(TOKEN_KEY, res.token);
    sessionStorage.setItem(USER_KEY, res.username);
    sessionStorage.setItem(EXPIRES_KEY, res.expiresAt);
    this.tokenSignal.set(res.token);
    this.username.set(res.username);
  }

  /** Recupera el token guardado solo si todavía no venció. */
  private readValidToken(): string | null {
    const token = sessionStorage.getItem(TOKEN_KEY);
    const expires = sessionStorage.getItem(EXPIRES_KEY);
    if (!token || !expires || new Date(expires).getTime() <= Date.now()) {
      sessionStorage.removeItem(TOKEN_KEY);
      sessionStorage.removeItem(USER_KEY);
      sessionStorage.removeItem(EXPIRES_KEY);
      return null;
    }
    return token;
  }
}