import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly username = signal('');
  readonly password = signal('');
  readonly loading = signal(false);
  readonly error = signal('');

  onUsername(event: Event): void {
    this.username.set((event.target as HTMLInputElement).value);
  }

  onPassword(event: Event): void {
    this.password.set((event.target as HTMLInputElement).value);
  }

  submit(event: Event): void {
    event.preventDefault();

    if (!this.username().trim() || !this.password()) {
      this.error.set('Ingresa usuario y contraseña.');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.auth.login(this.username().trim(), this.password()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/upload');
      },
      error: err => {
        this.error.set(
          err.status === 401
            ? 'Usuario o contraseña incorrectos.'
            : 'No se pudo iniciar sesión. Intenta de nuevo.'
        );
        this.loading.set(false);
      }
    });
  }
}