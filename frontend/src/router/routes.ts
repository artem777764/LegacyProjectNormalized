export type RouteItem = {
  path: string;
  name: string;
  label: string;
};

export const routes: RouteItem[] = [
  { path: '/', name: 'Home', label: 'Главная' },
  { path: '/about', name: 'About', label: 'О нас' },
  { path: '/gallery', name: 'Gallery', label: 'Gallery' },
  { path: '/iss', name: 'Iss', label: 'Iss' },
];