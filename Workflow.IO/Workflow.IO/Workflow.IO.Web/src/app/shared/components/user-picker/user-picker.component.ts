import {
  Component,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';

import { UserLookup } from '../../../core/models/user.models';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-user-picker',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './user-picker.component.html',
  styles: `
    .picker {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      position: relative;
    }

    label {
      font-weight: 600;
    }

    input {
      padding: 0.5rem 0.65rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-input);
      color: var(--text-primary);
    }

    .suggestions {
      list-style: none;
      margin: 0;
      padding: 0;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-panel);
      max-height: 200px;
      overflow-y: auto;
    }

    .suggestions button {
      width: 100%;
      text-align: left;
      border: none;
      background: transparent;
      color: var(--text-primary);
      padding: 0.5rem 0.75rem;
      cursor: pointer;
    }

    .suggestions button:hover {
      background-color: var(--bg-hover);
    }

    .selected {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .hint {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin: 0;
    }
  `,
})
export class UserPickerComponent {
  readonly label = input('Find user by email');
  readonly placeholder = input('Search email…');

  readonly userSelected = output<UserLookup>();

  private readonly userService = inject(UserService);
  private readonly search$ = new Subject<string>();

  searchText = '';
  readonly suggestions = signal<UserLookup[]>([]);
  readonly selected = signal<UserLookup | null>(null);
  readonly searching = signal(false);

  constructor() {
    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          this.searching.set(true);
          return this.userService.lookupUsers(query);
        }),
      )
      .subscribe({
        next: (users) => {
          this.suggestions.set(users);
          this.searching.set(false);
        },
        error: () => {
          this.suggestions.set([]);
          this.searching.set(false);
        },
      });
  }

  onSearchChange(value: string): void {
    this.selected.set(null);
    if (value.trim().length < 2) {
      this.suggestions.set([]);
      return;
    }

    this.search$.next(value.trim());
  }

  pick(user: UserLookup): void {
    this.selected.set(user);
    this.searchText = user.email;
    this.suggestions.set([]);
    this.userSelected.emit(user);
  }

  clear(): void {
    this.selected.set(null);
    this.searchText = '';
    this.suggestions.set([]);
  }
}
