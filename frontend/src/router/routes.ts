export type RouteItem = {
  path: string;
  name: string;
  label: string;
};

export const routes: RouteItem[] = [
  { path: '/home', name: 'Home', label: 'Home' },
  { path: '/events', name: 'Events', label: 'Events' },
  { path: '/osdr', name: 'Osdr', label: 'Osdr' },
  { path: '/gallery', name: 'Gallery', label: 'Gallery' },
  { path: '/iss', name: 'Iss', label: 'Iss' },
];