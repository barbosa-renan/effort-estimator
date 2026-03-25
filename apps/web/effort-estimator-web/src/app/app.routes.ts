import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home').then(m => m.HomePage),
  },
  {
    path: 'sobre',
    loadComponent: () => import('./pages/about/about').then(m => m.AboutPage),
  },
  { path: '**', redirectTo: '' },
];
