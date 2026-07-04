import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { UserDto } from '../../../../core/models/user.models';
import { UserService } from '../../../../core/services/user.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-users.component.html',
  styles: `
    .page {
      max-width: 48rem;
    }

    ul {
      list-style: none;
      padding: 0;
    }

    li {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid var(--border-color);
    }

    form {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      width: 100%;
    }

    input {
      padding: 0.4rem 0.55rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-input);
      color: var(--text-primary);
    }

    .actions {
      display: flex;
      gap: 0.5rem;
      flex-shrink: 0;
    }

    button {
      padding: 0.35rem 0.65rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-panel);
      color: var(--text-primary);
      cursor: pointer;
    }

    .danger {
      color: #b91c1c;
      border-color: #fecaca;
    }

    .error {
      color: #b91c1c;
    }

    .empty {
      color: #64748b;
    }
  `,
})
export class AdminUsersComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly users = signal<UserDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);

  readonly editForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.minLength(8)],
  });

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.userService
      .getAllUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          this.users.set(users);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }

  startEdit(user: UserDto): void {
    this.editingId.set(user.userId);
    this.editForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      password: '',
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(userId: string): void {
    if (this.editForm.invalid) {
      return;
    }

    const raw = this.editForm.getRawValue();
    this.userService
      .updateUser(userId, {
        email: raw.email,
        firstName: raw.firstName,
        lastName: raw.lastName,
        password: raw.password || 'Placeholder1!',
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.editingId.set(null);
          this.loadUsers();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  removeUser(user: UserDto): void {
    if (!confirm(`Delete user ${user.email}?`)) {
      return;
    }

    this.userService
      .deleteUser(user.userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadUsers(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }
}
