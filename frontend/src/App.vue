<template>
  <div>
    <NavBar />
    <main class="pt-16 bg-gradient-to-b from-slate-950 via-slate-900 to-slate-800 text-slate-100 min-h-screen">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import NavBar from '@/components/NavBar.vue';
</script>