<template>
  <nav class="w-full fixed top-0 z-40 bg-slate-900/60 backdrop-blur-sm border-b border-white/6">
    <div class="max-w-6xl mx-auto px-4">
      <div class="h-16 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <router-link to="/" class="text-xl font-semibold text-indigo-200">Space Pulse</router-link>
        </div>

        <div class="hidden md:flex items-center space-x-6">
          <router-link
            v-for="item in navItems"
            :key="item.path"
            :to="item.path"
            class="py-2 px-1 text-sm transition-all"
            :class="isActive(item.path) ? 'text-white font-semibold' : 'text-slate-400 hover:text-white'"
          >
            {{ item.label }}
          </router-link>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import { routes } from '@/router/routes';
import type { RouteItem } from '@/router/routes';

const route = useRoute();
const navItems = computed<RouteItem[]>(() => routes);

function isActive(path: string) {
  return route.path === path;
}
</script>

<style scoped>

</style>
