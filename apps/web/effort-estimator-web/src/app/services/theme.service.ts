import { inject, Injectable, signal, effect } from '@angular/core';
import { DOCUMENT } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _document = inject(DOCUMENT);

  readonly isDark = signal(this._loadPreference());

  constructor() {
    effect(() => this._applyTheme(this.isDark()));
  }

  toggle(): void {
    this.isDark.update(v => !v);
  }

  private _loadPreference(): boolean {
    const saved = localStorage.getItem('theme');
    if (saved) return saved === 'dark';
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  private _applyTheme(dark: boolean): void {
    this._document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
    localStorage.setItem('theme', dark ? 'dark' : 'light');
  }
}
