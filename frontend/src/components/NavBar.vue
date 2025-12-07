<template>
  <nav class="w-full fixed top-0 z-10 bg-gray-100 shadow-md">
    <div class="max-w-6xl mx-auto px-4">
      <div class="h-14 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <router-link to="/" class="text-lg font-semibold text-gray-800">Space Pulse</router-link>
        </div>

        <div class="hidden md:flex items-center space-x-6">
          <router-link
            v-for="item in navItems"
            :key="item.path"
            :to="item.path"
            class="py-2 px-1 text-sm transition-all"
            :class="isActive(item.path) ? 'text-gray-900 font-semibold' : 'text-gray-600 hover:text-gray-800'"
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