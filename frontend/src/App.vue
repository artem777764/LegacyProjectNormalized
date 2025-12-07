<template>
  <div>
    <NavBar />
    <main class="pt-14 bg-white text-gray-800 min-h-screen">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import NavBar from '@/components/NavBar.vue';
</script>