import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { routes } from './routes';

const routeMap: Record<string, () => Promise<any>> = {
  Home: () => import('@/views/HomeView.vue'),
  Events: () => import('@/views/AstroEventsView.vue'),
  Osdr: () => import('@/views/OsdrDatasetsView.vue'),
  Gallery: () => import('@/views/JwstGalleryView.vue'),
  Iss: () => import('@/views/IssTrendView.vue'),
};

const appRoutes: RouteRecordRaw[] = routes.map(r => ({
  path: r.path,
  name: r.name,
  component: routeMap[r.name] || (() => import('@/views/TempView.vue')),
}));

const router = createRouter({
  history: createWebHistory(),
  routes: appRoutes,
});

export default router;